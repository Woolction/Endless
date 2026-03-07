namespace Backend.API.Middleware;

public class ContentSecurityPolicy
{
    private readonly RequestDelegate next;

    public ContentSecurityPolicy(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("Content-Security-Policy",
            "default - src 'self'; script - src 'self'; style - src 'self'; img - src 'self' data: https:; font - src 'self' https:; connect - src 'self'; frame - ancestors 'none';");

        await next.Invoke(context);
    }
}