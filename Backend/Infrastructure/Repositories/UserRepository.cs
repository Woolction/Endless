using Domain.Interfaces.Repositories;
using Elastic.Clients.Elasticsearch;
using Domain.Rows.Users;
using Domain.Entities;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ElasticsearchClient client;
    private readonly string[] value;

    public UserRepository(ElasticsearchClient client)
    {
        this.client = client;

        value = [
            "name^5",
            "name.edge^3",
            "name.ngram^0.3"
        ];
    }

    public async Task CreateSearchIndex(User user)
    {
        UserSearchIndex index = new(user);

        var response = await client.IndexAsync(index, r => r
            .Index("users")
            .Id(index.UserId));

        if (!response.IsValidResponse)
        {
            throw new Exception(response.DebugInformation);
        }
    }

    public async Task<UserSearchRow> SearchUsersByName(string name, ICollection<FieldValue> lastValues, CancellationToken cancellationToken)
    {
        var search = new SearchRequestDescriptor<UserSearchIndex>()
            .Indices("users")
            .Query(q => q
                .MultiMatch(m => {
                    m.Query(name)
                    .Fields(value)
                    .MinimumShouldMatch(1)
                    .Type(TextQueryType.BestFields);
                    
                    if (name.Length >= 3)
                        m.Fuzziness("AUTO");
                    }))
            .Size(20)
            .Sort(s => s
                .Score(s => s.Order(SortOrder.Desc)))
            .TrackScores(true);

        if (lastValues.Count > 0)
            search = search.SearchAfter(lastValues);

        var result = await client.SearchAsync<UserSearchIndex>(search, cancellationToken);

        if (result.Hits.Count == 0)
            return new UserSearchRow();

        List<UserSearchIndexRow> searchedUsers = result.Hits
            .Select(h => {
                Console.WriteLine($"{h.Id} - {h.Score}");

                return new UserSearchIndexRow()
                {
                    SearchedUser = h.Source ?? new(),
                    Score = h.Score ?? 0
                };
            })
            .ToList();

        var lastHit = result.Hits.Last();

        return new UserSearchRow()
        {
            SearchedUsers = searchedUsers
        };
    }
}
