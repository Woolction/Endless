using Application.Queries.Searchs;
using Application.Dtos.Contents;
using Application.Dtos.Searchs;
using Domain.Interfaces.Repositories;

namespace Application.Handlers;

public class ContentSearchingHandler
{
    private readonly IContentRepository contentRepository;

    public ContentSearchingHandler(IContentRepository contentRepository)
    {
        this.contentRepository = contentRepository;
    }

    public async Task<Result<ContentSearchDto>> Handle(SearchQuery query)
    {
        bool hasLastSearch = query.LastSearch != null;

        SearchDto searchDto = hasLastSearch == true ? query.LastSearch! : new SearchDto();

        dynamic[] result = await contentRepository.SearchContentsByName(query.Name, hasLastSearch, searchDto.LastScore);

        ContentDto[] channelDtos = result.Select(c => new ContentDto(
            c.content_id, c.channel_id, c.creator_id, c.title, c.slug, c.description,
            c.created_date, c.content_type, 0, c.content_url, c.prewiev_photo_url,
            c.saves_count, c.likes_count, c.comments_count, c.dis_likers_count, c.views_count
        )).ToArray();

        if (channelDtos.Length < 1)
            return Result<ContentSearchDto>.Failure(404, $"Content with name: {query.Name} not found");

        var last = result.Last();

        return Result<ContentSearchDto>.Success(
            200, new ContentSearchDto(channelDtos, new SearchDto() { LastScore = last.lastScore }));
    }
}