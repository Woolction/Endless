using Microsoft.EntityFrameworkCore;

namespace Backend.API.Data.Components;

[Owned]
public class RefreshToken
{
    public required string Token { get; set; } = string.Empty;
    public DateTime ValidityPeriod { get; set; }
}