using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.API.Data.Models;

public class Comment
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;

    public DateTime PublicationDate { get; set; }

    public long LikeCount { get; set; }
    public long DizLikeCount { get; set; }
    public long ViewsCount { get; set; }

    public Guid ContentId { get; set; }

    [ForeignKey(nameof(ContentId))]
    public Content? Content { get; set; }

    public Guid CommentatorId { get; set; }
    
    [ForeignKey(nameof(CommentatorId))]
    public User? Commentator { get; set; }
}