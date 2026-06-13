namespace Application.Features.Channels.Update;

public record class ChannelUpdateDto(
    Guid Id, string Name, string Slug,
    string? Description, DateTime CreatedDate, string Process);