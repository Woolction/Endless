namespace Contracts.Dtos.Channels;

public record class ChannelOwnerDto(
    Guid OwnerId, Guid ChannelId, DateTime OwnedDate, string OwnerRole);