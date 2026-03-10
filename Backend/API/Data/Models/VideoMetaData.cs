namespace Backend.API.Data.Models;

public class VideoMetaData
{
    public Guid ContentId { get; set; }
    public Content? Content { get; set; }
}