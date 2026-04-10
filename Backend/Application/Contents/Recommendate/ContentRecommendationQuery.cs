using Application.Contents.Dtos;
using MediatR;

namespace Application.Contents.Recommendate;

public record class ContentRecommendationQuery(Guid UserId) : IRequest<Result<ContentRecoDto[]>>;