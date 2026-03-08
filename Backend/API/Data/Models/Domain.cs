
namespace Backend.API.Data.Models;

public class Domain
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    
    public long TotalViews { get; set; }
    public long TotalLikes { get; set; }

    public List<DomainSubscription> Subsrcibers { get; set; } = new List<DomainSubscription>();

    public List<DomainOwner> Owners { get; set; } = new List<DomainOwner>();
    public List<Content> Contents { get; set; } = new List<Content>();
}