namespace Contracts.Dtos.Users;

public record class RegistryDto(
    Guid NewUserId, string Token, string RefreshToken);