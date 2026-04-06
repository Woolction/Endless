using Contracts.Dtos.Searchs;

namespace Contracts.Dtos.Contents;

public record class ContentSearchQuery(
    ContentDto[] ContentsDto, SearchDto? LastSimiratity);