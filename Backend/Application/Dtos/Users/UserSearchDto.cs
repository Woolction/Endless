using Application.Dtos.Searchs;

namespace Application.Dtos.Users;

public record class UserSearchDto(
    UserDto[] UserDtos, SearchDto? LastSearch);