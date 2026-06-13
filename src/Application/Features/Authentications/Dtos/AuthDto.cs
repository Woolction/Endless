using Application.Features.Users.Update;

namespace Application.Features.Authentications.Dtos;

public record class AuthDto(
    UserUpdateDto UserDto, string Token, string RefreshToken);