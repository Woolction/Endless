using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.API.Data.Models;

public class Comment
{
    public Guid Id { get; set; }
    public Guid CommentatorId { get; set; }
    public User? Commentator { get; set; }

    public Guid ContentId { get; set; }
    public Content? Content { get; set; }

    public string? Text { get; set; }

    public DateTime PublicatedDate { get; set; }

    public long LikeCount { get; set; }
    public long DizLikeCount { get; set; }
    public long ViewsCount { get; set; }
}