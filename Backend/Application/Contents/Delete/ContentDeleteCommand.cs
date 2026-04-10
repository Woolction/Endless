using MediatR;

namespace Application.Contents.Delete;

public record class ContentDeleteCommand(
    Guid UserId, Guid ContentId) : IRequest<Result<Null>>;