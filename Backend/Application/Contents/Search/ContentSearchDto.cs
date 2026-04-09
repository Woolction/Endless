using Application.Contents.Dtos;
using Application.Searchs;

namespace Application.Contents.Search;

public record class ContentSearchDto(
    ContentDto[] ContentsDto, SearchDto? LastSimiratity);