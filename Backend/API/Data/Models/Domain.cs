
namespace Backend.API.Data.Models;

public class Domain
{
    public Guid Id { get; set; }
    public string DomainName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
    public long TotalViews { get; set; }
    public long TotalLikes { get; set; }

    public List<User> SignedUsers { get; set; } = new List<User>();

    public List<DomainOwner> Owners { get; set; } = new List<DomainOwner>();
    public List<Content> Contents { get; set; } = new List<Content>();
}