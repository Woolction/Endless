namespace Application.Channels.Update;

public record class ChannelUpdateCommand(
    string Name, string Description);