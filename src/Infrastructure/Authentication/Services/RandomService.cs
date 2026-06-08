using Microsoft.AspNetCore.WebUtilities;
using Application.Interfaces.Services;
using System.Security.Cryptography;

namespace Authentication.Services;

public class RandomService : IRandomService
{    
    public string GenerateToken(int length)
    {
        byte[] randombytes = RandomNumberGenerator.GetBytes(length);

        return WebEncoders.Base64UrlEncode(randombytes);
    }
}