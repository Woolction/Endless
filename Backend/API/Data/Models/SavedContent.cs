using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.API.Data.Models;

public class SavedContent
{
    public Guid OwnerId { get; set; }
    public User? Owner { get; set; }

    public Guid ContentId { get; set; }
    public Content? Content { get; set; }
 
    public DateTime RegistryDate { get; set; }
}