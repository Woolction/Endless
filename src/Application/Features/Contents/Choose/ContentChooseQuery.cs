using Application.Features.Contents.Dtos;
using MediatR;

namespace Application.Features.Contents.Choose;

public record class ContentChooseQuery(Guid ContentId) : IRequest<Result<ContentDto>>;