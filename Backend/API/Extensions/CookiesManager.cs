using Backend.API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Extensions;

public static class CookiesManager
{
    public static void CraeteTokensInCookies(this ControllerBase controller, AuthResponseDto responseDto)
    {
        controller.Response.Cookies.Append("AccessToken", responseDto.Token, new CookieOptions()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddMinutes(30)
        });

        controller.Response.Cookies.Append("RefreshToken", responseDto.RefreshToken, new CookieOptions()
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