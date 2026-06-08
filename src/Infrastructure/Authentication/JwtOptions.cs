namespace Authentication;

public class JwtOptions
{
    public static string ConfigSection { get; private set; } = "JwtSettings";
    public string SecretKey { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int ExpireMinutes { get; set; }

}