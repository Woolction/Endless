using Microsoft.EntityFrameworkCore;

namespace Domain.Components;

[Owned]
public class RefreshToken
{
    public required string Token { get; set; } = string.Empty;
    public DateTime ValidityPeriod { get; set; }
}