using Domain.Entities;
using Elastic.Clients.Elasticsearch;

namespace Domain.Rows.Users;

public class UserSearchRow
{
    public List<User>? SearchedUsers { get; set; }

    public long TotalLikes { get; set; }
    public long CommentsCount { get; set; }
    public long ContentsCount { get; set; }
    public long FollowersCount { get; set; }
    public long FollowingCount { get; set; }
    public long OwnedChannelsCount { get; set; }
    public long ChannelSubscriptionsCount { get; set; }

    public FieldValue[]? LastValues { get; set; }
}