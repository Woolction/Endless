using Application.Searchs;
using MediatR;

namespace Application.Users.Search;

public record class UserSearchQuery(
    string Name, SearchDto? LastSearch) : IRequest<Result<UserSearchDto>>;