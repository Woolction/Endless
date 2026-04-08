namespace Domain.Entities;

public class UserInteractionContent
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid ContentId { get; set; }
    public Content? Content { get; set; }

    public int WatchTimeSeconds { get; set; }

    public bool Liked { get; set; }

    public bool Saved { get; set; }
}