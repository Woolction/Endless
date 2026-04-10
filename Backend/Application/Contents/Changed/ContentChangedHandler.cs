using Application.Channels.Dtos;
using Application.Contents.Dtos;
using Application.Users.Dtos;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Contents.Changed;

public class ContentChangedHandler : IRequestHandler<ContentChangedQuery, Result<ChangedContentDto>>
{
    private readonly ILogger<ContentChangedHandler> logger;
    private readonly IAppDbContext context;

    public ContentChangedHandler(IAppDbContext context, ILogger<ContentChangedHandler> logger)
    {
        this.context = context;
        this.logger = logger;
    }
    
    public async Task<Result<ChangedContentDto>> Handle(ContentChangedQuery query, CancellationToken cancellationToken)
    {
        var changedContent = await context.Contents
            .Select(content => new
            {
                c = content,
                cResponse = new ContentDto(content.Id, content.ChannelId, content.CreatorId,
                    content.Title, content.Slug, content.Description,
                    content.CreatedDate, content.ContentType.ToString(),
                    content.VideoMeta != null ? content.VideoMeta.DurationSeconds : 0,
                    content.ContentUrl, content.PrewievPhotoUrl, content.Savers.Count, content.Likers.Count,
                    content.Comments.Count, content.DisLikers.Count, content.ViewsCount)
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(content => content.c.Id == query.ContentId, cancellationToken);

        if (changedContent is null || changedContent.c is null)
            return Result<ChangedContentDto>.Failure(404, "Content not found");

        ChannelDto? ChannelResponse = await context.Channels //segregate
            .Select(Channel => new ChannelDto(
                Channel.Id, Channel.Name,
                "@" + Channel.Slug, Channel.Description ?? "",
                Channel.CreatedDate, Channel.AvatarPhotoUrl,
                Channel.Subscribers.Count, Channel.Contents.Count,
                Channel.Owners.Count, Channel.TotalLikes, Channel.TotalViews))
            .AsNoTracking()
            .FirstOrDefaultAsync(Channel => Channel.Id == changedContent.c.ChannelId, cancellationToken);

        UserDto? userResponse = await context.Users //segregate
            .Select(user => new UserDto(
                user.Id, user.Name, "@" + user.Slug,
                user.Description ?? "", user.RegistryData, user.Email,
                user.Role.ToString(), user.AvatarPhotoUrl, user.TotalLikes,
                user.Comments.Count, user.Contents.Count, user.Followers.Count,
                user.Following.Count, user.OwnedChannels.Count, user.SubscripedChannels.Count))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        logger.LogInformation("Returned content {ContentId}",
            query.ContentId);

        return Result<ChangedContentDto>.Success(200, new ChangedContentDto(
            ChannelResponse, changedContent.cResponse, userResponse));
    }
}