namespace Backend.API.Dtos;

public record class RegistryResponseDto(
    Guid NewUserId, string Token, string RefreshToken);