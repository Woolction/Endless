using Application.Contents.Dtos;
using MediatR;

namespace Application.Contents.Random;

public record class ContentRandomQuery() : IRequest<Result<ContentDto[]>>;