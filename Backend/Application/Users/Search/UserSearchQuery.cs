using Elastic.Clients.Elasticsearch;
using MediatR;

namespace Application.Users.Search;

public record class UserSearchQuery(
    string Name, double? LastScore) : IRequest<Result<SearchedUserDto[]>>;