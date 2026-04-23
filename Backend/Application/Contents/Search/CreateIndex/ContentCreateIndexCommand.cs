using Application.Searchs;
using MediatR;

namespace Application.Contents.Search.CreateIndex;

public record class ContentCreateIndexCommand() : IRequest<Result<IndexCreatedDto>>;