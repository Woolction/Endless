namespace Application.Users.Registry;

public record class RegistryDto(
    Guid NewUserId, string Token, string RefreshToken);