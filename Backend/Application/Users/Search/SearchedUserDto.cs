using Application.Users.Dtos;

namespace Application.Users.Search;

public record class SearchedUserDto(
    UserDto User, double Score);