namespace Backend.API.Dtos;

public record class ChangedContentDto(
    DomainResponseDto? DomainResponseDto,
    ContentResponseDto ContentResponseDto,
    UserResponseDto? UserResponseDto);