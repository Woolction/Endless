using Application.Features.Images;
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

    public ImageVariantsDto Avatar { get; set; } = new ();

    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }

    public UserSearchIndex() { }

    public UserSearchIndex(User user, Image image, List<ImageVariant> variants)
    {
        UserId = user.Id;
        Name = user.Name;
        Slug = user.Slug;
        Email = user.Email;
        Description = user.Description;
        TotalLikes = user.TotalLikes;

        for (int i = 0; i < variants.Count; i++)
        {
            var variant = variants[i];

            Avatar.Variants.Add(new ImageVariantDto(
                    variant.Url, variant.Width, variant.Height));
        }

        R = image.R;
        G = image.G;
        B = image.B;

        RegistryData = user.RegistryData;
        Role = (int)user.Role;
    }
}