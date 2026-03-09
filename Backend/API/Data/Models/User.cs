using Backend.API.Data.Components;

namespace Backend.API.Data.Models;

public class User
{
    public Guid Id { get; set; }
    public RefreshToken? RefreshToken { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime RegistryData { get; set; }

    public PrivateType PrivateType { get; set; } = PrivateType.Request;
    public UserRole Role { get; set; } = UserRole.User;

    public List<SavedContent> SavedContents { get; set; } = new List<SavedContent>();
    public List<LikedContent> LikedContents { get; set; } = new List<LikedContent>();

    public long TotalLikes { get; set; }

    public List<Comment> Comments { get; set; } = new List<Comment>();
    public List<Content> Contents { get; set; } = new List<Content>();

    public List<UserSubscribtion> Followers { get; set; } = new List<UserSubscribtion>();
    public List<UserSubscribtion> Following { get; set; } = new List<UserSubscribtion>();

    public List<DomainOwner> OwnedDomains { get; set; } = new List<DomainOwner>();
    public List<DomainSubscription> DomainSubscriptions { get; set; } = new List<DomainSubscription>();
}