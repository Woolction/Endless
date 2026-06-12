using Application.Features.Imagess;
using Domain.Entities;

namespace Application.Features.Rows.Users;

public class UserSearchIndex
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long TotalLikes;

    public DateTime RegistryData;
    public int Role;

    public ImageVariants IconVariants { get; set; } = new();

    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }

    public UserSearchIndex() { }

    public UserSearchIndex(User user, UserMeta userMeta)
    {
        UserId = user.Id;
        Name = user.Name;
        Slug = user.Slug;
        Email = user.Email;
        Description = user.Description;
        TotalLikes = user.TotalLikes;

        IconVariants = new ImageVariants(
            userMeta.IconBase,
            userMeta.Small,
            userMeta.Medium,
            userMeta.Large
        );

        R = userMeta.R;
        G = userMeta.G;
        B = userMeta.B;

        RegistryData = user.RegistryData;
        Role = (int)user.Role;
    }
}