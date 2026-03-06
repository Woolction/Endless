using Backend.API.Data.Models;
using Backend.API.Dtos;

namespace Backend.API.Services;

public interface IAuthService
{
    Task<User?> RegistryAsync(AuthRequestDto requestDto);
    Task<AuthResponseDto?> LoginAsync(AuthRequestDto requestDto);
    Task<AuthResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto requestDto);
}