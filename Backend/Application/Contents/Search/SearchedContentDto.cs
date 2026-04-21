using Application.Contents.Dtos;
using Application.Searchs;

namespace Application.Contents.Search;

public record class SearchedContentDto(
    ContentDto ContentDto, double Score);