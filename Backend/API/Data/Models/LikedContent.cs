using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.API.Data.Models;

public class LikedContent
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid ContentId { get; set; }
    public Content? Content { get; set; }
    
    public DateTime LikedDate { get; set; }
}