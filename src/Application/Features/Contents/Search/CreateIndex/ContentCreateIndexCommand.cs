using Application.Features.Searchs;
using MediatR;

namespace Application.Features.Contents.Search.CreateIndex;

public record class ContentCreateIndexCommand() : IRequest<Result<IndexCreatedDto>>;