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

    public string? AvatarPhotoUrl { get; set; }

    public long TotalLikes { get; set; }

    public List<DisLikedContent> DisLikedContents { get; set; } = new List<DisLikedContent>();
    public List<SavedContent> SavedContents { get; set; } = new List<SavedContent>();
    public List<LikedContent> LikedContents { get; set; } = new List<LikedContent>();
    public List<Content> Contents { get; set; } = new List<Content>();

    public List<DisLikedComment> DisLikedComments { get; set; } = new List<DisLikedComment>();
    public List<LikedComment> LikedComments { get; set; } = new List<LikedComment>();
    public List<Comment> Comments { get; set; } = new List<Comment>();

    public List<UserFollowing> Followers { get; set; } = new List<UserFollowing>();
    public List<UserFollowing> Following { get; set; } = new List<UserFollowing>();

    public List<DomainOwner> OwnedDomains { get; set; } = new List<DomainOwner>();
    public List<DomainSubscription> SubscripedDomains { get; set; } = new List<DomainSubscription>(); //Following the Domain

    public List<UserGenreVector> Vectors { get; set; } = new List<UserGenreVector>();
    public List<UserInterationContent> UserInterations { get; set; } = new List<UserInterationContent>();
}