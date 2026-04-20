using Elastic.Clients.Elasticsearch.IndexManagement;
using Domain.Interfaces.Repositories;
using Elastic.Clients.Elasticsearch;
using Domain.Rows.Contents;
using Domain.Entities;
using Npgsql.Internal;
using Elastic.Clients.Elasticsearch.Analysis;

namespace Infrastructure.Repositories;

public class ContentRepository : IContentRepository
{
    private readonly ElasticsearchClient client;
    private readonly string indexName;
    private readonly string[] synonymus;

    public ContentRepository(ElasticsearchClient client)
    {
        this.client = client;

        indexName = "contents";

        synonymus = [];
    }

    public async Task<CreateIndexResponse> CreateMapping(CancellationToken cancellationToken)
    {
        var hasIndex = await client.Indices.ExistsAsync(indexName, cancellationToken);

        if (hasIndex.Exists)
            await client.Indices.DeleteAsync(indexName, cancellationToken);

        return await client.Indices.CreateAsync(indexName, c => c
            .Settings(s => s
                .Analysis(a => a
                    .Analyzers(a => a
                        .Custom("smart_analyzer", c => c
                            .Tokenizer("standard")
                            .Filter([
                                "lowercase",
                                "asciifolding",
                                "possessive_stemmer",
                                "stop",
                                "stemmer",
                                "shingle"]))
                        .Custom("smart_search", c => c
                            .Tokenizer("standard")
                            .Filter([
                                "lowercase",
                                "asciifolding",
                                "possessive_stemmer",
                                "stop",
                                "stemmer",
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
                .Properties<ContentSearchRow>(p => p
                    .Keyword(k => k.ContentId)
                    .Keyword(k => k.CreatorId)
                    .Keyword(k => k.ChannelId)
                    .Keyword(k => k.ContentUrl)
                    .Keyword(k => k.PrewievPhotoUrl)
                    .Keyword(l => l.Slug)
                    .Date(d => d.CreatedDate)
                    .LongNumber(l => l.ViewsCount)
                    .LongNumber(l => l.LikesCount)
                    .LongNumber(l => l.SavesCount)
                    .LongNumber(l => l.CommentsCount)
                    .LongNumber(l => l.DisLikersCount)
                    .IntegerNumber(i => i.ContentType)
                    .IntegerNumber(i => i.DurationSeconds)
                    .SemanticText(st => st.Title)
                    .Text("description", t => t
                        .Analyzer("smart_analyzer"))
                    .Text("title", t => t
                        .Analyzer("smart_analyzer")
                        .SearchAnalyzer("smart_search")))),
            cancellationToken);
    }

    public Task<IndexResponse> CreateSearchIndex(Content content, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<DeleteResponse> DeleteSearchIndex(Guid contentId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ContentSearchRow> SearchContentsByName(string name, ICollection<FieldValue> lastValues, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}