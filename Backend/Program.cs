using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Services;
using Scalar.AspNetCore;
using System.Text;

namespace Backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        #region --Services--
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        IConfiguration jwtSettings = builder.Configuration.GetSection("JwtSettings");

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

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(nameof(UserRole.User), policy =>
            {
                policy.RequireRole(nameof(UserRole.User), nameof(UserRole.Creator), nameof(UserRole.Admin));
            })
            .AddPolicy(nameof(UserRole.Creator), policy =>
            {
                policy.RequireRole(nameof(UserRole.Creator), nameof(UserRole.Admin));
            });
        #endregion +----------+

        #region --DbContext--

        builder.Services.AddDbContext<EndlessContext>(context =>
            context.UseNpgsql(builder.Configuration.GetConnectionString("DB")));

        #endregion +----------+

        #region --Custom Services--

        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        #endregion +----------+

        var app = builder.Build();

        #region --Middleware--
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        #endregion +----------+

        #region --EndPoints--
        app.MapControllers();
        app.MapGet("/", () => "Hello World!");

        #endregion +----------+

        app.Run();
    }
}
