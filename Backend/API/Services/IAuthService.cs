using Backend.API.Data.Models;
using Backend.API.Dtos;

namespace Backend.API.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> RegistryAsync(AuthRequestDto requestDto);
    Task<AuthResponseDto?> LoginAsync(AuthRequestDto requestDto);
    Task<AuthResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto requestDto);
}