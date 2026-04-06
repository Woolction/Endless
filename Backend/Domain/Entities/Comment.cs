namespace Domain.Entities;

public class Comment
{
    public Guid Id { get; set; }
    public Guid CommentatorId { get; set; }
    public User? Commentator { get; set; }

    public Guid ContentId { get; set; }
    public Content? Content { get; set; }

    public string? Text { get; set; }

    public DateTime PublicatedDate { get; set; }

    public long ViewsCount { get; set; }

    public List<LikedComment> Likers { get; set; } = new List<LikedComment>();
    public List<DisLikedComment> DisLikers { get; set; } = new List<DisLikedComment>();
}