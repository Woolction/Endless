using Application.Users.Dtos;
using MediatR;

namespace Application.Users.Create;

public record class UsersCreateCommand(
    int Count,
    string Password = "123"
    ) : IRequest<Result<UserDto[]>>;