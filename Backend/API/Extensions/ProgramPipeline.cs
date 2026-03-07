
using System.Text;
using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

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

        //authorization builder.Services
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(nameof(UserRole.User), policy =>
            {
                policy.RequireRole(nameof(UserRole.User), nameof(UserRole.Creator), nameof(UserRole.Admin));
            })
            .AddPolicy(nameof(UserRole.Creator), policy =>
            {
                policy.RequireRole(nameof(UserRole.Creator), nameof(UserRole.Admin));
            });

        builder.Services.AddDbContext<EndlessContext>(context =>
            context.UseNpgsql(DbKey));

        //custum builder.Services
        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        builder.Services.AddScoped<IAuthService, AuthService>();
    }

    public static void MiddlewareRegistry(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseCookiePolicy();
    }
    
    public static void EndPointsRegistry(this WebApplication app)
    {
        app.MapControllers();
        
        //app.MapGet("/", () => "Hello World!");
    }
}