using Application.Features.Users.Dtos;

namespace Application.Features.Users.Search;

public record class SearchedUserDto(
    UserDto User, double Score);