using MediatR;

namespace Application.Features.Contents.Delete;

public record class ContentDeleteCommand(
    Guid UserId, Guid ContentId) : IRequest<Result<Null>>;