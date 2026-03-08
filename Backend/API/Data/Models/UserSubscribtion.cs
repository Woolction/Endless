using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.API.Data.Models;

public class UserSubscribtion
{
    public Guid FollowerId { get; set; }
    [ForeignKey(nameof(FollowerId))] public User? FollowedUser { get; set; }

    public Guid UserId { get; set; }
    [ForeignKey(nameof(UserId))] public User? User { get; set; }

    public DateTime FollowedDate { get; set; }
    public bool Notification { get; set; }
}