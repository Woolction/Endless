using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.API.Data.Models;

public class DomainSubscription
{
    public Guid SubscriberId { get; set; }
    [ForeignKey(nameof(SubscriberId))] public User? SubscribedUser { get; set; }

    public Guid DomainId { get; set; }
    [ForeignKey(nameof(DomainId))] public Domain? Domain { get; set; }

    public DateTime SubscribedDate { get; set; }
    public bool Notification { get; set; }
}