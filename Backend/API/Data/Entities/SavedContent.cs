namespace Backend.API.Data.Models;

public class SavedContent
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid ContentId { get; set; }
    public Content? Content { get; set; }
 
    public DateTime SavedDate { get; set; }
}