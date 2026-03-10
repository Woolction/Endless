using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.API.Data.Models;

public class UserSubscribtion
{
    public Guid FollowerId { get; set; }
    public User? Follower { get; set; }

    public Guid FollowedUserId { get; set; }
    public User? FollowedUser { get; set; }

    public DateTime FollowedDate { get; set; }
    public bool Notification { get; set; }
}