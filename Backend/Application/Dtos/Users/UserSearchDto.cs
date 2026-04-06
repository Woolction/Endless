using Application.Dtos.Searchs;

namespace Application.Dtos.Users;

public record class UserSearchQuery(
    UserDto[] UserResponses, SearchDto? LastSearch);