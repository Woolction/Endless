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

    public long SavedContentsCount { get; set; }
    public long LikedContentsCount { get; set; }
    
    public long CommentsCount { get; set; }
    public long ContentsCount { get; set; }

    public long FollowersCount { get; set; }
    public long FollowingCount { get; set; }

    public long OwnedDomainsCount { get; set; }
    public long DomainSubscriptionsCount { get; set; }

    public long VectorsCount { get; set; }
    public long UserInterationsCount { get; set; }

    public List<SavedContent> SavedContents { get; set; } = new List<SavedContent>();
    public List<LikedContent> LikedContents { get; set; } = new List<LikedContent>();

    public List<Comment> Comments { get; set; } = new List<Comment>();
    public List<Content> Contents { get; set; } = new List<Content>();

    public List<UserSubscribtion> Followers { get; set; } = new List<UserSubscribtion>();
    public List<UserSubscribtion> Following { get; set; } = new List<UserSubscribtion>();

    public List<DomainOwner> OwnedDomains { get; set; } = new List<DomainOwner>();
    public List<DomainSubscription> DomainSubscriptions { get; set; } = new List<DomainSubscription>(); //Following the Domain

    public List<UserGenreVector> Vectors { get; set; } = new List<UserGenreVector>();
    public List<UserInterationContent> UserInterations { get; set; } = new List<UserInterationContent>();
}