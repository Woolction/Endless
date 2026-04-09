namespace Application.Channels.Dtos;

public record class ChannelOwnerDto(
    Guid OwnerId, Guid ChannelId, DateTime OwnedDate, string OwnerRole);