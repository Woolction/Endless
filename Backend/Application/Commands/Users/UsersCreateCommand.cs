namespace Application.Commands.Users;

public record class UsersCreateCommand(
    int Count,
    string Password = "123"
    );