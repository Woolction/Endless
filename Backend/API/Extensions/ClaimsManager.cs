using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Extensions;

public static class ClaimsManager
{
    public static Guid GetIDFromClaim(this ControllerBase controller)
    {
        string id = controller.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        return new(id);
    }

    public static string GetEmailFromClaim(this ControllerBase controller)
    {
        string email = controller.User.FindFirst(ClaimTypes.Email)!.Value;

        return email;
    }
}
