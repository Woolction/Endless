using Application.Features.Contents.Dtos;
using MediatR;

namespace Application.Features.Contents.Recommendate;

public record class ContentRecommendationQuery(Guid UserId) : IRequest<Result<ContentFeedDto[]>>;