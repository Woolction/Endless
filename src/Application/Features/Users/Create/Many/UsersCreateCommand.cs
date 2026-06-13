using Application.Features.Users.Update;
using MediatR;

namespace Application.Features.Users.Create.Many;

public record class UsersCreateCommand(
    string[] Names,
    string Password = "123"
    ) : IRequest<Result<UserUpdateDto[]>>;