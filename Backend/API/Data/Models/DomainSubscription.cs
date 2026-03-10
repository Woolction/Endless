using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.API.Data.Models;

public class DomainSubscription
{
    public Guid SubscriberId { get; set; }
    public User? SubscribedUser { get; set; }

    public Guid DomainId { get; set; }
    public Domain? Domain { get; set; }

    public DateTime SubscribedDate { get; set; }
    public bool Notification { get; set; }
}