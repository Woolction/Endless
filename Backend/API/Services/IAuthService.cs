using Backend.API.Data.Model;
using Backend.API.Dtos;

namespace Backend.API.Services;

public interface IAuthService
{
    Task<User?> RegistryAsync(AuthRequestDto requestDto);
    Task<string?> LoginAsync(AuthRequestDto requestDto);
}