namespace Backend.API.Data.Models;

public class LikedComment
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid CommentId { get; set; }
    public Comment? Comment { get; set; }

    public DateTime LikedDate { get; set; }
}