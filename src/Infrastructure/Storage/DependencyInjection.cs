using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces.Services;
using Storage.Services;

namespace Storage;

public static class DependencyInjection
{
    public static void AddStorageInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IStorage, R2Storage>();
    }
}