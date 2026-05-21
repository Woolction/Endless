using System.Security.Cryptography;
using Domain.Common.Interfaces.Services;

namespace Infrastructure.Services;

public class RandomService : IRandomService
{    
    public string GenerateToken(int length)
    {
        byte[] randomNumber = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }
}