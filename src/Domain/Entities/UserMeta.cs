namespace Domain.Entities;

public class UserMeta
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid ImageId { get; set; }
    public Image Image { get; set; } = new() { BaseUrl = "/storage/images/user" };
}