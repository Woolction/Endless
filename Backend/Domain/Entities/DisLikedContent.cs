namespace Domain.Entities;

public class DisLikedContent
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid ContentId { get; set; }
    public Content? Content { get; set; }

    public DateTime DisLikedDate { get; set; }
}