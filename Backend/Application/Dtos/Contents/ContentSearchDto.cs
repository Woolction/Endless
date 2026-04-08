using Application.Dtos.Searchs;

namespace Application.Dtos.Contents;

public record class ContentSearchDto(
    ContentDto[] ContentsDto, SearchDto? LastSimiratity);