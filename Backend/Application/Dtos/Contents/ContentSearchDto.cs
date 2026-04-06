using Application.Dtos.Searchs;

namespace Application.Dtos.Contents;

public record class ContentSearchQuery(
    ContentDto[] ContentsDto, SearchDto? LastSimiratity);