using Backend.API.Extensions;

namespace Backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.ServicesRegistry();

        var app = builder.Build();

        app.MiddlewareRegistry();
        app.EndPointsRegistry();

        app.Run();
    }
}
