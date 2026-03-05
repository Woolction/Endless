using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Backend.API.Data.Context;
using Backend.API.Data.Model;
using Backend.API.Services;
using Scalar.AspNetCore;

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

        //DbContext
        builder.Services.AddDbContext<EndlessContext>(context =>
            context.UseNpgsql(builder.Configuration.GetConnectionString("DB")));

        //Custom Services
        builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        builder.Services.AddSingleton<AuthService>();

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