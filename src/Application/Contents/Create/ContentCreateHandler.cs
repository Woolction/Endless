using Domain.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Domain.Common.Interfaces.Services;
using Application.Contents.Dtos;
using Domain.Common.Interfaces.Db;
using Domain.Rows.Contents;
using Domain.Entities;
using Domain.Common.Enums;
using MediatR;
using Application.Dtos;

namespace Application.Contents.Create;

public class ContentCreateHandler : IRequestHandler<ContentCreateCommand, Result<ContentDto>>
{
    private readonly IAppDbContext context;
    private readonly ILogger<ContentCreateHandler> logger;
    private readonly ContentUploadPublisher publisher;
    private readonly IRandomService randomService;
    private readonly IR2Service r2Service;

    public ContentCreateHandler(IAppDbContext context, ContentUploadPublisher publisher, IRandomService randomService, IR2Service r2Service, ILogger<ContentCreateHandler> logger)
    {
        this.context = context;

        this.randomService = randomService;
        this.publisher = publisher;
        this.r2Service = r2Service;
        this.logger = logger;
    }

    public async Task<Result<ContentDto>> Handle(ContentCreateCommand cmd, CancellationToken cancellationToken)
    {
        if (cmd.ChannelId != null)
        {
            ChannelOwner? channelOwner = await context.ChannelOwners
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == cmd.UserId &&
                owner.ChannelId == cmd.ChannelId,
                cancellationToken: cancellationToken);

            if (channelOwner == null)
            {
                logger.LogWarning("User {UserId} tried to create content without permission",
                   cmd.UserId);
                return Result<ContentDto>.Failure(404, "User not found");
            }

            if (channelOwner.OwnerRole <= ChannelOwnerRole.ContentEditor)
            {
                logger.LogWarning("User {UserId} tried to create content without permission",
                   cmd.UserId);
                return Result<ContentDto>.Failure(403, "You do not have sufficient rights");
            }
        }
        else
        {
            User? user = await context.Users.FindAsync(
                cmd.UserId, cancellationToken);

            if (user == null)
                return Result<ContentDto>.Failure(404, "User not found");
        }

        string? videoPath = null;
        string? photoPath = null;

        if (cmd.ContentFile != null && cmd.ContentFile.Length != 0)
        {
            videoPath = await r2Service.SaveFormFileAsync(
                cmd.ContentFile, "Video", token: cancellationToken);
        }

        if (cmd.PrewievPhoto != null && cmd.PrewievPhoto.Length != 0)
        {
            photoPath = await r2Service.SaveFormFileAsync(
                cmd.PrewievPhoto, "Images", ".jpeg", token: cancellationToken);
        }

        Content content = new()
        {
            CreatorId = cmd.UserId,
            ChannelId = cmd.ChannelId,
            Title = cmd.Title,
            Slug = randomService.GenerateToken(16),
            CreatedDate = DateTime.UtcNow,
            RandomKey = System.Random.Shared.NextDouble(),
            ContentType = cmd.ContentType
        };

        context.ContentVectors.AddRange(await context.Genres
            .Select(genre => new ContentGenreVector()
            {
                Content = content,
                GenreId = genre.Id
            })
            .AsNoTracking()
            .ToArrayAsync(cancellationToken));

        context.Contents.Add(content);

        await context.SaveChangesAsync();

        var message = new VideoUploadMessage(
            content.Id, content.Slug, videoPath, photoPath);

        await publisher.PublishAsync(message, cancellationToken);

        logger.LogInformation("Content {ContentId} created for user {UserId}",
            content.Id, cmd.UserId);

        return Result<ContentDto>.Success(201, new ContentDto(
            content.Id, content.ChannelId, content.CreatorId,
            content.Title, content.Slug, content.Description,
            content.CreatedDate, content.ContentType.ToString(), 0,
            content.VideoMeta.VideoUrl, new PreviewPhotoDto(
                content.VideoMeta.PhotoUrl, content.VideoMeta.R, content.VideoMeta.G, content.VideoMeta.B),
            0, 0, 0, 0, 0));
    }
}