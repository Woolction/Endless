using Application.Features.Contents.Dtos;
using MediatR;

namespace Application.Features.Contents.Random;

public record class ContentRandomQuery() : IRequest<Result<ContentFeedDto[]>>;