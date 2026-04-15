using Elastic.Clients.Elasticsearch;
using MediatR;

namespace Application.Users.Search;

public record class UserSearchQuery(
    string Name, FieldValue[]? LastValues) : IRequest<Result<UserSearchDto>>;