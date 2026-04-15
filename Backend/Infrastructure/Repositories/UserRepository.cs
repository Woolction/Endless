using Domain.Interfaces.Repositories;
using Infrastructure.Connector;
using System.Data;
using Dapper;
using Domain.Rows.Users;
using Domain.Entities;
using Elastic.Clients.Elasticsearch;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ElasticsearchClient elasticsearch;
    private readonly DbConnectorFactory connector;

    public UserRepository(DbConnectorFactory connector, ElasticsearchClient elasticsearch)
    {
        this.elasticsearch = elasticsearch;
        this.connector = connector;
    }

    public async Task CreateSearchIndex(User user)
    {
        //await elasticsearch.IndexAsync<User>();
    }

    public async Task<IEnumerable<UserSearchRow>> SearchUsersByName(string name, bool hasLastSearch, double lastScore, Guid lastId, CancellationToken cancellationToken)
    {
        using IDbConnection db = connector.CreateConnection();

        // u.""Name"" % @name = TRUE for speed

        var sql = @"
        WITH ranked AS (
            WITH filtered AS (
                SELECT * FROM ""Users"" u
                WHERE (u.""Name"" ILike @pattern) = TRUE
                LIMIT 100
            )
            SELECT
                u.""Id"", u.""Name"", u.""Slug"", 
                u.""Description"", u.""RegistryData"", 
                u.""Email"", u.""Role"", u.""AvatarPhotoUrl"",
                
                (SELECT COUNT(*) FROM ""Comments"" c WHERE c.""CommentatorId"" = u.""Id"") AS CommentsCount,
                (SELECT COUNT(*) FROM ""Contents"" c WHERE c.""CreatorId"" = u.""Id"") AS ContentsCount,
                (SELECT COUNT(*) FROM ""UserFollowings"" f WHERE f.""FollowerId"" = u.""Id"") AS FollowersCount,
                (SELECT COUNT(*) FROM ""UserFollowings"" f WHERE f.""FollowedUserId"" = u.""Id"") AS FollowingCount,
                (SELECT COUNT(*) FROM ""ChannelOwners"" c WHERE c.""OwnerId"" = u.""Id"") AS OwnedChannelsCount,
                (SELECT COUNT(*) FROM ""ChannelSubscriptions"" s WHERE s.""SubscriberId"" = u.""Id"") AS ChannelSubscriptionsCount,
        
                similarity(u.""Name"", @name) * 0.75 +
                (1.0 / (levenshtein(u.""Name"", @name) + 1)) * 0.25 AS ""Score""
            FROM filtered u
            
        )
        SELECT * FROM ranked
        WHERE(
            (@hasLastSearch = FALSE) OR
            (""Score"", ""Id"") < (@lastScore, @lastId)
        )
        ORDER BY ""Score"" DESC, ""Id"" ASC
        LIMIT 20;
        ";

        CommandDefinition command = new(
            sql, new
            {
                name,
                pattern = $"%{name}%",
                hasLastSearch,
                lastScore,
                lastId
            },
            cancellationToken: cancellationToken);

        IEnumerable<UserSearchRow> result = await db.QueryAsync<UserSearchRow>(command);

        return result;
    }
}
