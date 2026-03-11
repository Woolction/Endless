using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Middleware;
using Backend.API.Services;
using Scalar.AspNetCore;
using System.Text;

namespace Backend.API.Extensions;

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

        // Custum builder.Services
        
        //      Scoped
        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        //      Singleton
        builder.Services.AddSingleton<IInteractionService, InteractionService>();
        builder.Services.AddSingleton<IRecommendationService, RecommendationService>();

        //      Transient
    }

    public static void MiddlewareRegistry(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        app.UseMiddleware<ContentSecurityPolicy>();

        app.UseRouting();

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