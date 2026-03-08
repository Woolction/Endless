using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.API.Data.Models;

public class VideoMetaData
{
    [Key] public Guid ContentId { get; set; }
    [ForeignKey(nameof(ContentId))] public Content? Content { get; set; }

    public TimeSpan Duration { get; set; }
}