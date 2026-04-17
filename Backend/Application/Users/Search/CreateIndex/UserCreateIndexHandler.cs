using Application.Searchs;
using Domain.Rows.Users;
using Elastic.Clients.Elasticsearch;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Users.Search.CreateIndex;

public class UserCreateIndexHandler : IRequestHandler<UserCreateIndexCommand, Result<IndexCreatedDto>>
{
    private readonly ElasticsearchClient client;
    public UserCreateIndexHandler(ElasticsearchClient client)
    {
        this.client = client;
    }
    
    public async Task<Result<IndexCreatedDto>> Handle(UserCreateIndexCommand request, CancellationToken cancellationToken)
    {
        var hasIndex = await client.Indices.ExistsAsync("users", cancellationToken);

        if (hasIndex.Exists)
            await client.Indices.DeleteAsync("users");

        var response = await client.Indices.CreateAsync("users", a =>
            a.Mappings(m =>
                m.Properties<UserSearchIndex>(p => p
                    .Keyword(u => u.UserId)
                    .Text(u => u.Name)
                    .Text(u => u.Description)
                    .Date(u => u.RegistryData)
        )), cancellationToken);

        if (!response.IsValidResponse || !response.IsSuccess())
            return Result<IndexCreatedDto>.Failure(500, response.DebugInformation);

        return Result<IndexCreatedDto>.Success(201, new IndexCreatedDto(
            response.DebugInformation));
    }
}