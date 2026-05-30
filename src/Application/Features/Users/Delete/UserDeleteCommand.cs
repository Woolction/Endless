using MediatR;

namespace Application.Features.Users.Delete;

public record class UserDeleteCommand(
    Guid UserId) : IRequest<Result<Null>>;