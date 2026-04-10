using MediatR;

namespace Application.Users.Delete;

public record class UserDeleteCommand(
    Guid UserId) : IRequest<Result<Null>>;