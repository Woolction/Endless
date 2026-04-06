namespace Application.Commands.Authentications;

public record class AuthCreateCommand(
    string? Name, string Email, string Password);