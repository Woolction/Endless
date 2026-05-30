using MediatR;

namespace Application.Features.Users.Search;

public record class UserSearchQuery(
    string Name, double? LastScore) : IRequest<Result<SearchedUserDto[]>>;