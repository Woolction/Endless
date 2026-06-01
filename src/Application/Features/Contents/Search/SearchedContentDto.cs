using Application.Features.Contents.Dtos;

namespace Application.Features.Contents.Search;

public record class SearchedContentDto(
    ContentDto ContentDto, double Score);