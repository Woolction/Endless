using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Managers;

public static class CookiesManager
{
    public static void CraeteTokensInCookies(this ControllerBase controller, string accessToken, string refreshToken)
    {
        controller.Response.Cookies.Append("AccessToken", accessToken, new CookieOptions()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddMinutes(30)
        });

        controller.Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(30)
        });
    }

    public static void DeleteTokensInCookies(this ControllerBase controller)
    {
        controller.Response.Cookies.Delete("AccessToken");
        controller.Response.Cookies.Delete("RefreshToken");
    }
}