namespace Application.Users.Create.Registry;

public record class RegistryDto(
    Guid NewUserId, string Token, string RefreshToken);