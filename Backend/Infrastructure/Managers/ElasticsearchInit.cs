using Domain.Entities;
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
            )));
        }

        hasIndex = await client.Indices.ExistsAsync("channels");

        if (!hasIndex.Exists)
        {
            await client.Indices.CreateAsync("channels", a =>
                a.Mappings(m =>
                    m.Properties<Channel>(p => p
                        .Keyword(u => u.Id)
                        .Text(u => u.Name)
                        .Text(u => u.Description)
            )));
        }

        hasIndex = await client.Indices.ExistsAsync("contents");

        if (!hasIndex.Exists)
        {
            await client.Indices.CreateAsync("contents", a =>
                a.Mappings(m =>
                    m.Properties<Content>(p => p
                        .Keyword(u => u.Id)
                        .Text(u => u.Title)
                        .Text(u => u.Description)
            )));
        }
    }
}