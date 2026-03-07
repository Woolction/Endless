using Backend.API.Data.Components;

namespace Backend.API.Data.Models;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime DateOfBirth { get; set; }
    public DateTime RegistryData { get; set; }
    public RefreshToken? RefreshToken { get; set; }

    public List<SavedContent> SavedContents { get; set; } = new List<SavedContent>();
    public List<LikedContent> LikedContents { get; set; } = new List<LikedContent>();

    public long TotalLikes { get; set; }

    public List<Comment> Comments { get; set; } = new List<Comment>();
    public List<Content> Contents { get; set; } = new List<Content>();

    public List<User> Subscribes { get; set; } = new List<User>();
    public List<User> Subscriptions { get; set; } = new List<User>();

    public List<Domain> Domains { get; set; } = new List<Domain>();
    public List<Domain> SignedDomains { get; set; } = new List<Domain>();
}