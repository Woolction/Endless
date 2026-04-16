using Domain.Entities;
using Domain.Rows.Channels;
using Domain.Rows.Contents;
using Domain.Rows.Users;
using Elastic.Clients.Elasticsearch;

namespace Infrastructure.Managers;

public static class ElasticsearchInit
{
    public static async Task CreateIndex(ElasticsearchClient client)
    {
        var hasIndex = await client.Indices.ExistsAsync("users");

        if (!hasIndex.Exists)
        {
            await client.Indices.CreateAsync("users", a =>
                a.Mappings(m =>
                    m.Properties<UserSearchIndex>(p => p
                        .Keyword(u => u.UserId)
                        .Text(u => u.Name)
                        .Text(u => u.Description)
                        .Text(u => u.RegistryData)
            )));
        }

        hasIndex = await client.Indices.ExistsAsync("channels");

        if (!hasIndex.Exists)
        {
            await client.Indices.CreateAsync("channels", a =>
                a.Mappings(m =>
                    m.Properties<ChannelSearchIndex>(p => p
                        .Keyword(c => c.ChannelId)
                        .Text(c => c.Name)
                        .Text(c => c.Description)
                        .Date(c => c.CreatedDate)
            )));
        }

        hasIndex = await client.Indices.ExistsAsync("contents");

        if (!hasIndex.Exists)
        {
            await client.Indices.CreateAsync("contents", a =>
                a.Mappings(m =>
                    m.Properties<ContentSearchIndex>(p => p
                        .Keyword(c => c.ContentId)
                        .Keyword(c => c.ChannelId)
                        .Keyword(c => c.CreatorId)
                        .Text(c => c.Title)
                        .Text(c => c.Description)
                        .Date(c => c.CreatedDate)
            )));
        }
    }
}