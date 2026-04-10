using Application.Searchs;
using Application.Contents.Dtos;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Rows.Contents;

namespace Application.Contents.Search;

public class ContentSearchingHandler : IRequestHandler<ContentSearchQuery, Result<ContentSearchDto>>
{
    private readonly IContentRepository contentRepository;
    private readonly ILogger<ContentSearchingHandler> logger;

    public ContentSearchingHandler(IContentRepository contentRepository, ILogger<ContentSearchingHandler> logger)
    {
        this.contentRepository = contentRepository;
        this.logger = logger;
    }

    public async Task<Result<ContentSearchDto>> Handle(ContentSearchQuery query, CancellationToken cancellationToken)
    {
        bool hasLastSearch = query.LastSearch != null;

        SearchDto searchDto = hasLastSearch == true ? query.LastSearch! : new SearchDto();

        ContentSearchRow[] result = await contentRepository.SearchContentsByName(query.Name, hasLastSearch, searchDto.LastScore);

        ContentDto[] contentDtos = result.Select(c => new ContentDto(
            c.ContentId, c.ChannelId, c.CreatorId, c.Title, c.Slug, c.Description,
            c.CreatedDate, c.ContentType, 0, c.ContentUrl, c.PrewievPhotoUrl,
            c.SavesCount, c.LikesCount, c.CommentsCount, c.DisLikersCount, c.ViewsCount
        )).ToArray();

        if (contentDtos.Length < 1)
            return Result<ContentSearchDto>.Failure(404, $"Content with name: {query.Name} not found");

        var last = result.Last();

        logger.LogInformation("Search returned Contents {Count} results for {Query}",
           contentDtos.Length, query.Name);

        return Result<ContentSearchDto>.Success(
            200, new ContentSearchDto(contentDtos, new SearchDto() { LastScore = last.Score }));
    }
}