namespace Application.Commands.Channels;

public record class ChannelsCreateCommand(int Count)
{
    public Guid? UserId;
}