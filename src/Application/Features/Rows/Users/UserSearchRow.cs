using Elastic.Clients.Elasticsearch;

namespace Application.Features.Rows.Users;

public class UserSearchRow
{
    public List<UserSearchIndexRow> SearchedUsers { get; set; } = new();
}