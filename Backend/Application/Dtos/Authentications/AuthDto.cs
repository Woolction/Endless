namespace Application.Dtos.Authentications;

public record class AuthDto(
    Guid UserId, string Token, string RefreshToken);