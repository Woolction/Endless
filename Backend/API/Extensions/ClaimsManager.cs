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
}
