
using System.ComponentModel.DataAnnotations.Schema;
using Backend.API.Data.Components;

namespace Backend.API.Data.Models;

public class Content
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime PublicationDate { get; set; }
    public ContentType ContentType { get; set; } = ContentType.Video;

    public long LikeCount { get; set; }
    public long DizLikeCount { get; set; }
    public long ViewsCount { get; set; }

    public List<Comment> Comments { get; set; } = new List<Comment>();

    public Guid DomainId { get; set; }

    [ForeignKey(nameof(DomainId))]
    public Domain? Domain { get; set; }

    public Guid CreatorId { get; set; }

    [ForeignKey(nameof(CreatorId))]
    public User? Creator { get; set; }
}