using Elastic.Clients.Elasticsearch;
using Domain.Rows.Contents;
using Application.Searchs;
using MediatR;
using Domain.Interfaces.Repositories;

namespace Application.Contents.Search.CreateIndex;

public class ContentCreateIndexHandler : IRequestHandler<ContentCreateIndexCommand, Result<IndexCreatedDto>>
{
    private readonly IContentRepository contentRepository;

    public ContentCreateIndexHandler(IContentRepository contentRepository)
    {
        this.contentRepository = contentRepository;
    }

    public async Task<Result<IndexCreatedDto>> Handle(ContentCreateIndexCommand cmd, CancellationToken cancellationToken)
    {
        var response = await contentRepository.CreateMapping(cancellationToken);

        if (!response.IsValidResponse || !response.IsSuccess())
            return Result<IndexCreatedDto>.Failure(500, response.DebugInformation);

        return Result<IndexCreatedDto>.Success(201, new IndexCreatedDto(response.DebugInformation));
    }
}