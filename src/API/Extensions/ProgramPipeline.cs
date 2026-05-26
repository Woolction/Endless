using Microsoft.AspNetCore.Authentication.JwtBearer;
using Infrastructure.Services.RabbitConsumers;
using Domain.Common.Interfaces.Repositories;
using Infrastructure.Services.Background;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.RateLimiting;
using Application.Contents.Video.Upload;
using Domain.Common.Interfaces.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Elastic.Clients.Elasticsearch;
using Domain.Common.Interfaces.Db;
using Infrastructure.Repositories;
using Infrastructure.Connector;
using Infrastructure.Services;
using Application.Icon.Upload;
using Infrastructure.Context;
using System.Security.Claims;
using Domain.Common.Enums;
using Scalar.AspNetCore;
using Domain.Entities;
using API.Middleware;
using System.Text;
using Application;

namespace API.Extensions;

public static class ProgramPipeline
{
    public static void ServicesRegistry(this WebApplicationBuilder builder)
    {
        IConfiguration configuration = builder.Configuration;

        IConfiguration jwtSettings = configuration.GetSection("JwtSettings");
        string DbKey = configuration.GetConnectionString("DB")!;

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Limits.MaxRequestBodySize = long.MaxValue;
        });
        builder.Services.Configure<FormOptions>(o =>
        {
            o.MultipartBodyLengthLimit = long.MaxValue;
        });

        // SignalR
        builder.Services.AddSignalR();

        // Logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // Cors
        builder.Services.AddCors(options =>
        {
            /*options.AddPolicy("Frontend", policy =>
            {
                policy.WithOrigins("http://localhost:5100");
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.AllowCredentials();
            });*/
        });

        // Authentication
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
                RoleClaimType = ClaimTypes.Role,
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

        // Custum Services

        // Db context
        builder.Services.AddDbContext<EndlessContext>(context =>
            context.UseNpgsql(DbKey));

        builder.Services.AddScoped<IAppDbContext>(provider =>
            provider.GetRequiredService<EndlessContext>());

        builder.Services.AddSingleton<DbConnectorFactory>();

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

        // RabbitMQ
        builder.Services.AddSingleton<RabbitConnectorFactory>();

        builder.Services.AddSingleton<IRabbitMqConnector>(provider =>
            provider.GetRequiredService<RabbitConnectorFactory>());

        //      Scoped
        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        // Repositories
        builder.Services.AddScoped<IUserVectorsRepository, UserVectorsRepository>();
        builder.Services.AddScoped<IChannelRepository, ChannelRepository>();
        builder.Services.AddScoped<IContentRepository, ContentRepository>();
        builder.Services.AddScoped<IGenreRepository, GenreRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        //      Singleton
        builder.Services.AddSingleton<IRecommendationService, RecommendationService>();
        builder.Services.AddSingleton<IInteractionService, InteractionService>();

        builder.Services.AddSingleton<IRandomService, RandomService>();
        builder.Services.AddSingleton<IFfmpegService, FfmpegService>();
        builder.Services.AddSingleton<IR2Service, R2Service>();

        builder.Services.AddSingleton<VideoUploadPublisher>();
        builder.Services.AddSingleton<IconUploadPublisher>();

        //      Transient
        builder.Services.AddTransient<IConsumer, VideoUploadingConsumer>();
        builder.Services.AddTransient<IConsumer, IconUploadingConsumer>();

        // Background Services

        builder.Services.AddHostedService<RabbitMqConsumers>();
    }

    public static void MiddlewareRegistry(this WebApplication app)
    {
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

        // Static Files
        var provider = new FileExtensionContentTypeProvider();

        provider.Mappings[".m3u8"] = "application/x-mpegURL";
        provider.Mappings[".ts"] = "video/mp2t";

        string storagePath = "/storage";

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(storagePath),
            RequestPath = storagePath,
            ContentTypeProvider = provider
        });

        app.UseMiddleware<ContentSecurityPolicy>();

        app.UseRouting();

        //app.UseCors("Frontend");
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseCookiePolicy();

        app.UseRateLimiter();
    }

    public static void EndPointsRegistry(this WebApplication app)
    {
        app.MapControllers();

        //app.MapGet("/", () => Results.Redirect("/index.html"));
    }
}