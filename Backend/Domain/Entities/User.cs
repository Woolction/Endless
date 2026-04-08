using Domain.Common;

namespace Domain.Entities;

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

    public List<ChannelOwner> OwnedChannels { get; set; } = new List<ChannelOwner>();
    public List<ChannelSubscription> SubscripedChannels { get; set; } = new List<ChannelSubscription>(); //Following the Channel

    public List<UserGenreVector> Vectors { get; set; } = new List<UserGenreVector>();
    public List<UserInteractionContent> UserInterations { get; set; } = new List<UserInteractionContent>();

    public void SetName(string name)
    {
        Name = name;
    }

    public void SetSlug(string slug)
    {
        Slug = slug;
    }

    public void SetEmail(string email)
    {
        if (!email.Contains('@'))
        {
            throw new Exception("Email dont have @");
        }

        Email = email;
    }

    public void SetPassword(string hashedPassword)
    {
        PasswordHash = hashedPassword;
    }
}