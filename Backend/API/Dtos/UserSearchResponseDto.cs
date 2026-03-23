namespace Backend.API.Dtos;

public record class UserSearchResponseDto(
    UserResponseDto[] UserResponses, SearchDto? LastSearch);