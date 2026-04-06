using Contracts.Dtos.Searchs;

namespace Contracts.Dtos.Users;

public record class UserSearchQuery(
    UserDto[] UserResponses, SearchDto? LastSearch);