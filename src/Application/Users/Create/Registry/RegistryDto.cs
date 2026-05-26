using Application.Users.Dtos;

namespace Application.Users.Create.Registry;

public record class RegistryDto(
    Guid Id, string Token, string RefreshToken);