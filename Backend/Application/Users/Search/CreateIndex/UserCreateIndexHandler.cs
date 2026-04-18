using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch;
using Application.Searchs;
using Domain.Rows.Users;
using MediatR;

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
            await client.Indices.DeleteAsync("users", cancellationToken);

        var response = await client.Indices.CreateAsync("users", a => a
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
            .Properties<UserSearchIndex>(p => p
                .Keyword(k => k.UserId)
                .Keyword(k => k.Slug)
                .Keyword(k => k.Email)
                .Text(k => k.Description)
                .Date(d => d.RegistryData)
                .IntegerNumber(k => k.Role)
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

        if (!response.IsValidResponse || !response.IsSuccess())
            return Result<IndexCreatedDto>.Failure(500, response.DebugInformation);

        return Result<IndexCreatedDto>.Success(201, new IndexCreatedDto(
            response.DebugInformation));
    }
}