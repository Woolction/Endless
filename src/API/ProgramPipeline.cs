using Application.Features.Contents.Video.Upload;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.RateLimiting;
using Application.Features.Image.Upload;
using Microsoft.AspNetCore.StaticFiles;
using Application.Features.Rows;
using Scalar.AspNetCore;
using Authentication;
using API.Middleware;
using Recommendation;
using Application;
using Persistence;
using Messaging;
using Storage;
using Media;

namespace API;

public static class ProgramPipeline
{
    public static void ServicesRegistry(this WebApplicationBuilder builder)
    {
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

        // Infrastructures
        builder.Services.AddAuthenticationInfrastructure(builder.Configuration);
        builder.Services.AddPersistenceInfrastructure(builder.Configuration);
        builder.Services.AddRecommendationInfrastructure();
        builder.Services.AddMessagingInfrastructure();
        builder.Services.AddStorageInfrastructure();
        builder.Services.AddMediaInfrastructure();

        // MediatR
        builder.Services.AddMediatR(cf =>
            cf.RegisterServicesFromAssembly(typeof(AppMaker).Assembly));

        builder.Services.AddSingleton<SearchIndexUpsertPublisher>();
        builder.Services.AddSingleton<VideoUploadPublisher>();
        builder.Services.AddSingleton<ImageUploadPublisher>();
    }

    public static void MiddlewareRegistry(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();

            using var scope = app.Services.CreateScope();

            //EndlessContext context = scope.ServiceProvider.GetRequiredService<EndlessContext>();
            //context.Database.Migrate();
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

        app.MapControllers();
    }

    public static void EndPointsRegistry(this WebApplication app)
    {

        //app.MapGet("/", () => Results.Redirect("/index.html"));
    }
}