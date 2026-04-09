namespace Application.Authentications.Update;

public record class RefreshTokenCommand(
    string Token);