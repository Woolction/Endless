using Application.Searchs;
using Application.Contents.Dtos;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Rows.Contents;
using Elastic.Clients.Elasticsearch;

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

        SearchedContentDto[] contentDtos = result.SearchedContents.Select(c => new SearchedContentDto(new ContentDto(
            c.SearchedIndex.ContentId, c.SearchedIndex.ChannelId, c.SearchedIndex.CreatorId, c.SearchedIndex.Title,
            c.SearchedIndex.Slug, c.SearchedIndex.Description, c.SearchedIndex.CreatedDate, c.SearchedIndex.ContentType.ToString(),
            c.SearchedIndex.DurationSeconds, c.SearchedIndex.ContentUrl, c.SearchedIndex.PrewievPhotoUrl,
            0, 0, 0, 0, c.SearchedIndex.ViewsCount), c.Score)).ToArray();

        if (contentDtos.Length < 1)
            return Result<SearchedContentDto[]>.Failure(404, $"Content with name: {query.Name} not found");

        logger.LogInformation("Search returned Contents {Count} results for {Query}",
           contentDtos.Length, query.Name);

        return Result<SearchedContentDto[]>.Success(
            200, contentDtos);
    }
}