using Application.Contents.Dtos;
using MediatR;

namespace Application.Contents.Choose;

public record class ContentChooseQuery(Guid ContentId) : IRequest<Result<ChangedContentDto>>;