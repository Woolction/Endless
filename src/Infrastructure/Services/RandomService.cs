using System.Security.Cryptography;
using Domain.Common.Interfaces.Services;
using Microsoft.AspNetCore.WebUtilities;

namespace Infrastructure.Services;

public class RandomService : IRandomService
{    
    public string GenerateToken(int length)
    {
        byte[] randombytes = RandomNumberGenerator.GetBytes(length);

        return WebEncoders.Base64UrlEncode(randombytes);
    }
}