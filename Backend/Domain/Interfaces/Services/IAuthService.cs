using Domain.Entities;

namespace Domain.Interfaces.Services;

public interface IAuthService
{
    Task<string[]> CreateTokenResponse(User user);
}