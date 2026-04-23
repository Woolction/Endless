using Elastic.Clients.Elasticsearch;

namespace Domain.Rows.Users;

public class UserSearchRow
{
    public List<UserSearchIndexRow> SearchedUsers { get; set; } = new();
}