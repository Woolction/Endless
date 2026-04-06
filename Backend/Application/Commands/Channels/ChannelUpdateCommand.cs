namespace Application.Commands.Channels;

public record class ChannelUpdateCommand(
    string Name, string Description);