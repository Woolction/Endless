using Domain.Entities;
using Elastic.Clients.Elasticsearch;

namespace Domain.Rows.Users;

public class UserSearchRow
{
    public List<UserSearchIndex>? SearchedUsers { get; set; }
    public FieldValue[]? LastValues { get; set; }
}