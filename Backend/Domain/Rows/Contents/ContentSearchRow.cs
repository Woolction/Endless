namespace Domain.Rows.Contents;

public class ContentSearchRow
{
    public Guid ContentId { get; set; } 
    public Guid? ChannelId { get; set; } 
    public Guid CreatorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid Slug { get; set; } 
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public int? DurationSeconds { get; set; } 
    public string? ContentUrl { get; set; } 
    public string? PrewievPhotoUrl { get; set; } 
    public long SavesCount { get; set; }
    public long LikesCount { get; set; } 
    public long CommentsCount { get; set; } 
    public long DisLikersCount { get; set; } 
    public long ViewsCount { get; set; }

    public double Score { get; set; }
}