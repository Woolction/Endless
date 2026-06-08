using Application.Interfaces.Services;
using Media.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Media;

public static class DependencyInjection
{
    public static void AddMediaInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IFfmpegService, FfmpegService>();
    }
}