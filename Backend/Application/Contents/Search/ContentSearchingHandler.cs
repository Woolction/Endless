using Domain.Interfaces.Repositories;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
using Application.Contents.Dtos;
using Domain.Rows.Contents;
using MediatR;

namespace Application.Contents.Search;

public class ContentSearchingHandler : IRequestHandler<ContentSearchQuery, Result<SearchedContentDto[]>>
{
    private readonly IContentRepository contentRepository;
    private readonly ILogger<ContentSearchingHandler> logger;

    public ContentSearchingHandler(IContentRepository contentRepository, ILogger<ContentSearchingHandler> logger)
    {
        this.contentRepository = contentRepository;
        this.logger = logger;
    }

    public async Task<Result<SearchedContentDto[]>> Handle(ContentSearchQuery query, CancellationToken cancellationToken)
    {
        ICollection<FieldValue> lastValue = [];

        if (query.LastScore != null)
            lastValue.Add(FieldValue.Double(
                query.LastScore.Value));

        ContentSearchRow result = await contentRepository.SearchContentsByName(query.Name, lastValue, cancellationToken);

        SearchedContentDto[] contentDtos = result.SearchedContents
            .Select(c => new SearchedContentDto(new ContentDto(
                c.SearchedContent.ContentId, c.SearchedContent.ChannelId, c.SearchedContent.CreatorId,
                c.SearchedContent.Title, c.SearchedContent.Slug, c.SearchedContent.Description,
                c.SearchedContent.CreatedDate, c.SearchedContent.ContentType.ToString(),
                c.SearchedContent.DurationSeconds, c.SearchedContent.ContentUrl, c.SearchedContent.PrewievPhotoUrl,
                0, 0, 0, 0, c.SearchedContent.ViewsCount), c.Score)).ToArray();

        if (contentDtos.Length < 1)
            return Result<SearchedContentDto[]>.Failure(404, $"Content with name: {query.Name} not found");

        logger.LogInformation("Search returned Contents {Count} results for {Query}",
           contentDtos.Length, query.Name);

        return Result<SearchedContentDto[]>.Success(
            200, contentDtos);
    }
}