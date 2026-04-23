using Application.Users.Dtos;
using MediatR;

namespace Application.Users.Create.Many;

public record class UsersCreateCommand(
    string[] Names,
    string Password = "123"
    ) : IRequest<Result<UserDto[]>>;