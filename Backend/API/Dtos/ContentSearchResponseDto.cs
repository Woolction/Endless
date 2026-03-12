namespace Backend.API.Dtos;

public record class ContentSearchResponseDto(ContentResponseDto[] ContentsDto, double? LastSimiratity);