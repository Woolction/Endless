using Application.Users.Dtos;

namespace Application.Authentications.Dtos;

public record class AuthDto(
    UserDto UserDto, string Token, string RefreshToken);