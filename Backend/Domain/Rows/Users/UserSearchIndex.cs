using Domain.Common;
using Domain.Entities;

namespace Domain.Rows.Users;

public class UserSearchIndex
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarPhotoUrl { get; set; }
    public string? Description { get; set; }
    public long TotalLikes;

    public DateTime RegistryData;
    public int Role;

    public UserSearchIndex() {}

    public UserSearchIndex(User user)
    {
        UserId = user.Id;
        Name = user.Name;
        Slug = user.Slug;
        Email = user.Email;
        AvatarPhotoUrl = user.AvatarPhotoUrl;
        Description = user.Description;
        TotalLikes = user.TotalLikes;

        RegistryData = user.RegistryData;
        Role = (int)user.Role;
    }
}