using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.API.Data.Models;

public class UserInterationContent
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid ContentId { get; set; }
    public Content? Content { get; set; }

    public float WatchTime { get; set; }

    public bool Liked { get; set; }

    public bool Saved { get; set; }
}