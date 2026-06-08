using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Application.Interfaces.Services;               
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Authentication.Services;
using Domain.Common.Enums;
using Domain.Entities;
using System.Text;

namespace Authentication;

public static class DependencyInjection
{
    public static void AddAuthenticationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtOptions.ConfigSection);

        // Authentication
        services.AddAuthentication(jwtSection);

        // Authorization
        services.AddAuthorization();

        // Services
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRandomService, RandomService>();

        services.Configure<JwtOptions>(
            jwtSection);
    }

    private static void AddAuthentication(this IServiceCollection services, IConfigurationSection jwtSection)
    {
        services.AddAuthentication(options =>
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
                ValidIssuer = jwtSection["Issuer"],
                ValidAudience = jwtSection["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SecretKey"]!)),
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
    }
    
    private static void AddAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
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
    }
}