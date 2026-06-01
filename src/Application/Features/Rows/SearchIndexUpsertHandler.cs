using System.Text.Json;
using Application.Features.Rows.Channels;
using Application.Features.Rows.Contents;
using Application.Features.Rows.Users;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Features.Rows;

public class SearchIndexUpsertHandler : IRequestHandler<SearchIndexUpsertMessage, Result<Null>>
{
    private readonly ILogger<SearchIndexUpsertHandler> logger;
    private readonly IServiceScopeFactory scopeFactory;

    public SearchIndexUpsertHandler(ILogger<SearchIndexUpsertHandler> logger, IServiceScopeFactory scopeFactory)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    public async Task<Result<Null>> Handle(SearchIndexUpsertMessage message, CancellationToken token)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            IndexResponse? response = null;

            if (message.Type == nameof(User))
            {
                response = await UpsertUserIndex(
                    scope, message, token);
            }
            else if (message.Type == nameof(Channel))
            {
                response = await UpsertChannelIndex(
                    scope, message, token);
            }
            else if (message.Type == nameof(Content))
            {
                response = await UpsertContentIndex(
                    scope, message, token);
            }
            
            if (response != null)
                logger.LogInformation("upsert index response: {response}", response);

            return Result<Null>.Success(200, new Null());
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while executing the request");

            return Result<Null>.Failure(500, e.Message);
        }
    }

    private Task<IndexResponse> UpsertUserIndex(AsyncServiceScope scope, SearchIndexUpsertMessage message, CancellationToken token)
    {
        return scope.ServiceProvider
            .GetRequiredService<IUserRepository>()
            .CreateSearchIndex(
                JsonSerializer.Deserialize<UserSearchIndex>(message.SearchIndexJson), token);
    }
    
    private Task<IndexResponse> UpsertChannelIndex(AsyncServiceScope scope, SearchIndexUpsertMessage message, CancellationToken token)
    {
        return scope.ServiceProvider
            .GetRequiredService<IChannelRepository>()
            .CreateSearchIndex(
                JsonSerializer.Deserialize<ChannelSearchIndex>(message.SearchIndexJson), token);
    }
    
    private Task<IndexResponse> UpsertContentIndex(AsyncServiceScope scope, SearchIndexUpsertMessage message, CancellationToken token)
    {
        return scope.ServiceProvider
            .GetRequiredService<IContentRepository>()
            .CreateSearchIndex(
                JsonSerializer.Deserialize<ContentSearchIndex>(message.SearchIndexJson), token);
    }
}