using Application.Searchs;
using Application.Users.Dtos;

namespace Application.Users.Search;

public record class UserSearchDto(
    SearchedUserDto[] UserDtos);