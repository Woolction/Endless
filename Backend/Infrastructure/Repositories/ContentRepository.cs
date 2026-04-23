using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Domain.Interfaces.Repositories;
using Elastic.Clients.Elasticsearch;
using Domain.Rows.Contents;
using Elastic.Transport;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class ContentRepository : IContentRepository
{
    private readonly ElasticsearchClient client;
    
    private readonly string indexName;

    private readonly string[] synonymus;
    private readonly string[] value;

    public ContentRepository(ElasticsearchClient client)
    {
        this.client = client;

        indexName = "contents";

        synonymus = [];

        value = [
            "title^3",
            "description^2"];
    }

    public async Task<CreateIndexResponse> CreateMapping(CancellationToken cancellationToken)
    {
        var hasIndex = await client.Indices.ExistsAsync(indexName, cancellationToken);

        if (hasIndex.Exists)
            await client.Indices.DeleteAsync(indexName, cancellationToken);

        var response = await client.Indices.CreateAsync(indexName, c => c
            .Settings(s => s
                .Analysis(a => a
                    .Analyzers(a => a
                        .Custom("smart_analyzer", c => c
                            .Tokenizer("standard")
                            .Filter([
                                "lowercase",
                                "asciifolding",
                                "possessive_stemmer",
                                "stemmer",
                                "stop"])) //, "shingle"
                        .Custom("smart_search", c => c
                            .Tokenizer("standard")
                            .Filter([
                                "lowercase",
                                "asciifolding",
                                "possessive_stemmer",
                                "stemmer",
                                "stop",
                                "synonym_graph"])))
                    .TokenFilters(t => t
                        .Lowercase("lowercase")
                        .AsciiFolding("asciifolding")
                        .Stemmer("possessive_stemmer", s => s.
                            Language("possessive_english"))
                        .Stop("stop", s => s.
                            Stopwords(StopWordLanguage.English))
                        .Stemmer("stemmer", s => s
                            .Language("english"))
                        .Shingle("shingle", sh => sh
                            .MinShingleSize(2)
                            .MaxShingleSize(3)
                            .OutputUnigrams(true))
                        .SynonymGraph("synonym_graph", sg => sg
                            .Synonyms(synonymus)))))
            .Mappings(m => m
                .Properties<ContentSearchIndex>(p => p
                    .Keyword(k => k.ContentId)
                    .Keyword(k => k.CreatorId)
                    .Keyword(k => k.ChannelId)
                    .Keyword(k => k.ContentUrl)
                    .Keyword(k => k.PrewievPhotoUrl)
                    .Keyword(l => l.Slug)
                    .Date(d => d.CreatedDate)
                    .LongNumber(l => l.ViewsCount)
                    .IntegerNumber(i => i.ContentType)
                    .IntegerNumber(i => i.DurationSeconds)
                    .Text("description", t => t
                        .Analyzer("smart_analyzer")
                        .SearchAnalyzer("smart_search"))
                    .Text("title", t => t
                        .Analyzer("smart_analyzer")
                        .SearchAnalyzer("smart_search")))),
            cancellationToken);

        return response;
    }

    public async Task<IndexResponse> CreateSearchIndex(Content content, VideoMetaData videoMeta, CancellationToken cancellationToken)
    {
        ContentSearchIndex index = new(content, videoMeta);

        var response = await client.IndexAsync(index, c => c
            .Index(indexName)
            .Id(index.ContentId), cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new Exception(response.DebugInformation);
        }

        return response;
    }

    public async Task<DeleteResponse> DeleteSearchIndex(Guid contentId, CancellationToken cancellationToken)
    {
        DeleteRequest request = new(indexName, contentId);

        var response = await client.DeleteAsync(
            request, cancellationToken);

        if (!response.IsValidResponse)
            throw new Exception(response.DebugInformation);

        if (response.Result == Result.NotFound)
            throw new Exception("Doucument not found");

        return response;
    }

    public async Task<ContentSearchRow> SearchContentsByName(string name, ICollection<FieldValue> lastValues, CancellationToken cancellationToken)
    {
        var request = new SearchRequestDescriptor<ContentSearchIndex>()
            .Indices(indexName)
            .Query(q => q
                .FunctionScore(f => f
                    .Query(q => q
                        .Bool(b => b
                            .Should(
                                s => s.Match(t => t
                                    .Field(f => f.Title)
                                    .Fuzziness("AUTO")
                                    .Query(name)
                                    .Boost(5)),
                                s => s.MultiMatch(m => m
                                    .Type(TextQueryType.BestFields)
                                    .MinimumShouldMatch(1)
                                    .Fuzziness("AUTO")
                                    .Fields(value)
                                    .Query(name)),
                                s => s.MatchPhrase(m => m
                                    .Field(f => f.Title)
                                    .Query(name)
                                    .Boost(3)
                                    .Slop(2)))))
                    .Functions(f => f
                        .FieldValueFactor(fv => fv
                            .Modifier(FieldValueFactorModifier.Log1p)
                            .Field(f => f.ViewsCount)
                            .Factor(0.1)))
                    .BoostMode(FunctionBoostMode.Sum)
                    .MaxBoost(3)))
            .Size(20)
            .Sort(s => s
                .Score(s => s.Order(SortOrder.Desc)))
            .TrackScores(true);

        if (lastValues.Count > 0)
            request = request.SearchAfter(lastValues);

        var result = await client.SearchAsync<ContentSearchIndex>(request, cancellationToken);

        if (result.Hits.Count == 0)
            return new ContentSearchRow();

        List<ContentSearchIndexRow> searchedContents = result.Hits
            .Select(h =>
            {
                Console.WriteLine($"Content: {h.Id} - {h.Score}");

                return new ContentSearchIndexRow()
                {
                    SearchedContent = h.Source ?? new(),
                    Score = h.Score ?? 0
                };
            })
            .ToList();

        return new ContentSearchRow()
        {
            SearchedContents = searchedContents
        };
    }
}