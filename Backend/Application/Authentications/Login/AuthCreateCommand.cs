namespace Application.Authentications.Login;

public record class AuthCreateCommand(
    string? Name, string Email, string Password);