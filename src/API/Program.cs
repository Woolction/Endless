namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.ServicesRegistry();

        var app = builder.Build();

        app.Urls.Add("http://0.0.0.0:8080");

        app.MiddlewareRegistry();
        app.EndPointsRegistry();

        app.Run();
    }
}
