using Domain.Entities;

namespace Application.Interfaces.Services;

public interface IAuthService
{
    Task<string[]> CreateTokenResponse(User user);
}