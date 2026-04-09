namespace Application.Users.Create;

public record class UsersCreateCommand(
    int Count,
    string Password = "123"
    );