using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Backend.API.Data.Context;
using Backend.API.Data.Model;
using Backend.API.Services;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Routing.Tree;
using System.Text;

namespace Backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //Services
        builder.Services.AddValidation();
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        IConfiguration jwtSettings = builder.Configuration.GetSection("JwtSettings");

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
                ValidateIssuerSigningKey = true
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

        //DbContext
        builder.Services.AddDbContext<EndlessContext>(context =>
            context.UseNpgsql(builder.Configuration.GetConnectionString("DB")));

        //Custom Services
        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        var app = builder.Build();

        //EndPoints
        app.MapControllers();
        app.MapGet("/", () => "Hello World!");

        //Middleware
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.Run();
    }
}