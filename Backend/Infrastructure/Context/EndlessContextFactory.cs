using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public class EndlessContextFactory : IDesignTimeDbContextFactory<EndlessContext>
{
    public EndlessContext CreateDbContext(string[] args)
    {
        string pathBase = Path.Combine(Directory.GetCurrentDirectory(), "../API");

        var config = new ConfigurationBuilder()
            .SetBasePath(pathBase)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
            
        var optionsBuilder = new DbContextOptionsBuilder<EndlessContext>();

        optionsBuilder.UseNpgsql(config.GetConnectionString("DB"));

        return new EndlessContext(optionsBuilder.Options);
    }
}