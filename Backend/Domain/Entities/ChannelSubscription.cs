namespace Domain.Entities;

public class ChannelSubscription
{
    public Guid SubscriberId { get; set; }
    public User? Subscriber { get; set; }

    public Guid ChannelId { get; set; }
    public Channel? Channel { get; set; }

    public DateTime SubscribedDate { get; set; }
    public bool Notification { get; set; }
}