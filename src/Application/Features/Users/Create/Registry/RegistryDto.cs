using Application.Features.Users.Dtos;

namespace Application.Features.Users.Create.Registry;

public record class RegistryDto(
    Guid Id, string Token, string RefreshToken);