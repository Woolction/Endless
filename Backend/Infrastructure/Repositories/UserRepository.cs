using Domain.Interfaces.Repositories;
using Elastic.Clients.Elasticsearch;
using Domain.Rows.Users;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ElasticsearchClient client;

    public UserRepository(ElasticsearchClient client)
    {
        this.client = client;
    }

    public async Task CreateSearchIndex(User user)
    {
        UserSearchIndex index = new(user);

        await client.IndexAsync(index, r =>
            r.Index("users"));
    }

    public async Task<UserSearchRow> SearchUsersByName(string name, FieldValue[]? lastValues, CancellationToken cancellationToken)
    {
        var search = new SearchRequestDescriptor<UserSearchIndex>()
            .Indices("users")
            .Size(20)
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Name)
                    .Query(name)))
            .Sort(s => s
                .Score()
                .Field(f => f.UserId, SortOrder.Asc));

        if (lastValues != null)
            search = search.SearchAfter(lastValues);

        var result = await client.SearchAsync<UserSearchIndex>(search, cancellationToken);

        if (result.Hits.Count == 0)
        {
            return new UserSearchRow()
            {
                SearchedUsers = null,
                LastValues = null
            };
        }

        var lastHit = result.Hits.Last();

        UserSearchRow usersRow = new()
        {
            SearchedUsers = result.Documents.ToList(),
            LastValues = lastHit.Sort?.ToArray()
        };

        return usersRow;
    }
}
