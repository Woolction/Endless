using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.API.Data.Models;

public class SavedContent
{
    public Guid OwnerId { get; set; }
    [ForeignKey(nameof(OwnerId))] public User? Owner { get; set; }

    public Guid ContentId { get; set; }
    [ForeignKey(nameof(ContentId))] public Content? Content { get; set; }
 
    public DateTime RegistryDate { get; set; }
}