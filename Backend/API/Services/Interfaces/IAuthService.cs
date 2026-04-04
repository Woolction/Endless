using Backend.API.Dtos;

namespace Backend.API.Services.Interfaces;

public interface IAuthService
{
    Task<RegistryResponseDto?> RegistryAsync(AuthRequestDto requestDto);
    Task<AuthResponseDto?> LoginAsync(AuthRequestDto requestDto);
    Task<AuthResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto requestDto);
}