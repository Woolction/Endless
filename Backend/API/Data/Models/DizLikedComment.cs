namespace Backend.API.Data.Models;

public class DizLikedComment
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid CommentId { get; set; }
    public Comment? Comment { get; set; }

    public DateTime DizLikedDate { get; set; }
}