namespace Application.Authentications.Dtos;

public record class AuthDto(
    Guid UserId, string Token, string RefreshToken);