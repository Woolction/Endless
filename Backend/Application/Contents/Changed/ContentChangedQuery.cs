using Application.Contents.Dtos;
using MediatR;

namespace Application.Contents.Changed;

public record class ContentChangedQuery(Guid ContentId) : IRequest<Result<ChangedContentDto>>;