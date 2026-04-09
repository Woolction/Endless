namespace Application.Channels.CreateMany;

public record class ChannelsCreateCommand(int Count)
{
    public Guid? UserId;
}