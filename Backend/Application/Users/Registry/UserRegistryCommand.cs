namespace Application.Users.Registry;

public record class UserRegistryCommand(
    string? Name, string Email, string Password);