using Application.Features.Searchs;
using Application.Interfaces.Repositories;
using Elastic.Clients.Elasticsearch;
using MediatR;

namespace Application.Features.Contents.Search.CreateIndex;

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