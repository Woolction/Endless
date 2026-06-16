namespace Domain.Entities;

public class UserMeta
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid ImageId { get; set; }
    public Image Image { get; set; } = new() { BaseUrl = "/storage/images/user" };

    public string IconBase { get; set; } = "/storage/images/user-icons";
    public string Small { get; set; } = string.Empty;
    public string? Medium { get; set; }
    public string? Large { get; set; }

    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }
}