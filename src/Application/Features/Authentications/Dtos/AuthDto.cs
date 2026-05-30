using Application.Features.Users.Dtos;

namespace Application.Features.Authentications.Dtos;

public record class AuthDto(
    UserDto UserDto, string Token, string RefreshToken);