using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Domain.Interfaces.Repositories;
using Elastic.Clients.Elasticsearch;
using Domain.Rows.Channels;
using Domain.Entities;
using System.Data;

namespace Infrastructure.Repositories;

public class ChannelRepository : IChannelRepository
{
    private readonly ElasticsearchClient client;

    private readonly string indexName;
    private readonly string[] value;

    public ChannelRepository(ElasticsearchClient client)
    {
        this.client = client;

        indexName = "channels";

        value = [
            "name^5",
            "name.edge^3",
            "name.ngram^0.3"
        ];
    }

    public async Task<CreateIndexResponse> CreateMapping(CancellationToken cancellationToken)
    {
        var hasIndex = await client.Indices.ExistsAsync(indexName, cancellationToken);

        if (hasIndex.Exists)
            await client.Indices.DeleteAsync(indexName, cancellationToken);

        return await client.Indices.CreateAsync(indexName, a => a
            .Settings(s => s
                .MaxNgramDiff(8)
                .Analysis(a => a
                    .Tokenizers(t => t
                        .EdgeNGram("edge_tokenizer", eg => eg
                            .MinGram(2)
                            .MaxGram(10)
                            .TokenChars(TokenChar.Letter, TokenChar.Digit))
                        .NGram("ngram_tokenizer", ng => ng
                            .MinGram(2)
                            .MaxGram(7)
                            .TokenChars(TokenChar.Letter, TokenChar.Digit)))
                    .Analyzers(an => an
                        .Custom("edge_analyzer", ca => ca
                            .Tokenizer("edge_tokenizer")
                            .Filter(["lowercase"]))
                        .Custom("ngram_analyzer", ca => ca
                            .Tokenizer("ngram_tokenizer")
                            .Filter(["lowercase"])))))
        .Mappings(m => m
            .Properties<ChannelSearchIndex>(p => p
                .Keyword(k => k.ChannelId)
                .Keyword(k => k.Slug)
                .Keyword(k => k.AvatarPhotoUrl)
                .Text(k => k.Description)
                .Date(d => d.CreatedDate)
                .LongNumber(k => k.TotalViews)
                .LongNumber(k => k.TotalLikes)
                .Text("name", t => t
                    .Analyzer("edge_analyzer")
                    .SearchAnalyzer("standard")
                    .Fields(f => f
                        .Text("edge", t => t.
                            Analyzer("edge_analyzer"))
                        .Text("ngram", nt => nt
                            .Analyzer("ngram_analyzer")))
                    ))), cancellationToken);
    }

    public async Task<IndexResponse> CreateSearchIndex(Channel channel, CancellationToken cancellationToken)
    {
        ChannelSearchIndex index = new(channel);

        var response = await client.IndexAsync(index, r => r
            .Index(indexName)
            .Id(index.ChannelId), cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new Exception(response.DebugInformation);
        }

        return response;
    }

    public async Task<DeleteResponse> DeleteSearchIndex(Guid channelId, CancellationToken cancellationToken)
    {
        DeleteRequest request = new(indexName, channelId);

        var response = await client.DeleteAsync(
            request, cancellationToken);

        if (!response.IsValidResponse)
            throw new Exception(response.DebugInformation);

        if (response.Result == Result.NotFound)
            throw new Exception("Doucument not found");

        return response;
    }

    public async Task<ChannelSearchRow> SearchChannelsByName(string name, ICollection<FieldValue> lastValues, CancellationToken cancellationToken)
    {
        var search = new SearchRequestDescriptor<ChannelSearchIndex>()
            .Indices(indexName)
            .Query(q => q
                .MultiMatch(m => {
                    m.Query(name)
                    .Fields(value)
                    .MinimumShouldMatch(1)
                    .Type(TextQueryType.BestFields);

                    if (name.Length >= 3)
                        m.Fuzziness("AUTO");
                    }))
            .Size(20)
            .Sort(s => s
                .Score(s => s.Order(SortOrder.Desc)))
            .TrackScores(true);

        if (lastValues.Count > 0)
            search = search.SearchAfter(lastValues);

        var result = await client.SearchAsync<ChannelSearchIndex>(search, cancellationToken);

        if (result.Hits.Count == 0)
            return new ChannelSearchRow();

        List<ChannelSearchIndexRow> searchedChannels = result.Hits
            .Select(h =>
            {
                Console.WriteLine($"Channel {h.Id} - {h.Score}");

                return new ChannelSearchIndexRow()
                {
                    SearchedChannel = h.Source ?? new(),
                    Score = h.Score ?? 0
                };
            })
            .ToList();

        return new ChannelSearchRow()
        {
            SearchedChannels = searchedChannels
        };
    }
}