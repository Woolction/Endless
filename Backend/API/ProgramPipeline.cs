using Microsoft.AspNetCore.Authentication.JwtBearer;
using API.Middleware;
using Domain.Interfaces.Repositories;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Domain.Interfaces.Services;
using Infrastructure.Connector;
using Infrastructure.Services;
using Infrastructure.Context;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Domain.Common;
using Domain.Entities;
using Scalar.AspNetCore;
using System.Text;
using Domain.Interfaces;
using Elastic.Clients.Elasticsearch;
using Application;
using Infrastructure.Managers;

namespace API;

public static class ProgramPipeline
{
    public static void ServicesRegistry(this WebApplicationBuilder builder)
    {
        IConfiguration configuration = builder.Configuration;

        IConfiguration jwtSettings = configuration.GetSection("JwtSettings");
        string DbKey = configuration.GetConnectionString("DB")!;

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        // Logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // Cors
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy.WithOrigins("http://localhost:5100");
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.AllowCredentials();
            });
        });

        // Authentication
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer("Bearer", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
            };

            options.Events = new JwtBearerEvents()
            {
                OnMessageReceived = context =>
                {
                    string token = context.Request.Cookies["AccessToken"]!;

                    if (!string.IsNullOrEmpty(token))
                        context.Token = token;

                    return Task.CompletedTask;
                }
            };
        });

        // Authorization builder.Services
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(nameof(UserRole.Admin), policy =>
            {
                policy.RequireRole(nameof(UserRole.Admin));
            })
            .AddPolicy(nameof(UserRole.Creator), policy =>
            {
                policy.RequireRole(nameof(UserRole.Creator), nameof(UserRole.Admin));
            })
            .AddPolicy(nameof(UserRole.User), policy =>
            {
                policy.RequireRole(nameof(UserRole.User), nameof(UserRole.Creator), nameof(UserRole.Admin));
            });

        // Rate limiter
        builder.Services.AddRateLimiter(options =>
        {
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                await context.HttpContext.Response.WriteAsync("Too many requests", cancellationToken: token);
            };
            options.AddSlidingWindowLimiter("LoginLimit", options =>
            {
                options.PermitLimit = 5;
                options.QueueLimit = 0;
                options.SegmentsPerWindow = 6;
                options.Window = TimeSpan.FromMinutes(1);
            });
            options.AddTokenBucketLimiter("RegistryLimit", options =>
            {
                options.QueueLimit = 0;
                options.TokenLimit = 3;
                options.TokensPerPeriod = 1;
                options.ReplenishmentPeriod = TimeSpan.FromDays(1);
                options.AutoReplenishment = true;
            });
        });

        // Db context
        builder.Services.AddDbContext<EndlessContext>(context =>
            context.UseNpgsql(DbKey));

        builder.Services.AddScoped<IAppDbContext>(provider =>
            provider.GetRequiredService<EndlessContext>());

        // ElasticSearch
        builder.Services.AddSingleton(sp =>
        {
            var settings = new ElasticsearchClientSettings(new Uri("http://search:9200"))
                .DefaultIndex("users");

            return new ElasticsearchClient(settings);
        });

        // MediatR
        builder.Services.AddMediatR(cf =>
            cf.RegisterServicesFromAssembly(typeof(AppMaker).Assembly));

        // Custum Services

        //      Scoped
        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        // Repositories
        builder.Services.AddScoped<IUserVectorsRepository, UserVectorsRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IChannelRepository, ChannelRepository>();
        builder.Services.AddScoped<IContentRepository, ContentRepository>();
        builder.Services.AddScoped<IGenreRepository, GenreRepository>();

        //      Singleton
        builder.Services.AddSingleton<DbConnectorFactory>();

        builder.Services.AddSingleton<IInteractionService, InteractionService>();
        builder.Services.AddSingleton<IRecommendationService, RecommendationService>();

        builder.Services.AddSingleton<IR2Service, R2Service>();
        builder.Services.AddSingleton<IFfmpegService, FfmpegService>();

        //      Transient
    }

    public static async Task MiddlewareRegistry(this WebApplication app)
    {
        // Static Files
        var provider = new FileExtensionContentTypeProvider();

        provider.Mappings[".m3u8"] = "application/vnd.apple.mpegurl";
        provider.Mappings[".ts"] = "video/mp2t";

        app.UseStaticFiles(new StaticFileOptions
        {
            ContentTypeProvider = provider
        });

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();

            using (var scope = app.Services.CreateScope())
            {
                EndlessContext context = scope.ServiceProvider.GetRequiredService<EndlessContext>();
                context.Database.Migrate();
            }
        }
        else
        {
            app.UseHttpsRedirection();
        }

        app.UseMiddleware<ContentSecurityPolicy>();

        app.UseRouting();

        app.UseCors("Frontend");
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseCookiePolicy();

        app.UseRateLimiter();
    }

    public static void EndPointsRegistry(this WebApplication app)
    {
        app.MapControllers();

        //app.MapGet("/", () => "Hello World!");
    }
}