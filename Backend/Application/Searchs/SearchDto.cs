using Elastic.Clients.Elasticsearch;

namespace Application.Searchs;

public class SearchDto
{
    public double LastScore;
    public Guid LastId;
}