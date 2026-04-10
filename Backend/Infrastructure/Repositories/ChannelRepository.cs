using System.Data;
using Contracts.Rows;
using Dapper;
using Domain.Interfaces.Repositories;
using Infrastructure.Connector;
using Infrastructure.Context;

namespace Infrastructure.Repositories;

public class ChannelRepository : IChannelRepository
{
    private readonly DbConnectorFactory connector;

    public ChannelRepository(DbConnectorFactory connector)
    {
        this.connector = connector;
    }

    public async Task<IEnumerable<ChannelSearchRow>> SearchChannelsByName(string name, bool hasLastSearch, double lastScore, Guid lastId, CancellationToken token)
    {
        using IDbConnection db = connector.CreateConnection();

        string sql = @"
        WITH ranked AS (
            WITH filtered AS (
                SELECT * FROM ""Channels"" c
                WHERE (c.""Name"" ILIKE @pattern) = TRUE
                LIMIT 100
            )
            SELECT 
                c.""Id"", c.""Name"", c.""Slug"", c.""Description"", 
                c.""CreatedDate"", c.""AvatarPhotoUrl"", 
                c.""TotalLikes"", c.""TotalViews"",

                (SELECT COUNT(*) FROM ""ChannelSubscriptions"" s WHERE s.""ChannelId"" = c.""Id"") AS SubscribersCount,
                (SELECT COUNT(*) FROM ""ChannelOwners"" o WHERE o.""ChannelId"" = c.""Id"") AS OwnersCount,
                (SELECT COUNT(*) FROM ""Contents"" co WHERE co.""ChannelId"" = c.""Id"") AS ContentsCount,

                similarity(c.""Name"", @name) * 0.75 +
                (1.0 / (levenshtein(c.""Name"", @name) + 1)) * 0.25 AS ""Score""
            FROM filtered c
        ) 
        SELECT * FROM ranked
        WHERE(
            (@hasLastSearch = FALSE) OR
            (""Score"", ""Id"") < (@lastScore, @lastId)
        )
        ORDER BY ""Score"" DESC, ""Id"" ASC
        LIMIT 20
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
            cancellationToken: token);

        IEnumerable<ChannelSearchRow> result = await db.QueryAsync<ChannelSearchRow>(command);

        return result;
    }
}