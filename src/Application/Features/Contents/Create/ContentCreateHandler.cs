using Application.Features.Contents.Video.Upload;
using Application.Features.Rows.Contents;
using Application.Features.Contents.Dtos;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Features.Images;
using Application.Features.Images;
using Application.Interfaces.Db;
using Domain.Common.Enums;
using Domain.Entities;
using MediatR;

namespace Application.Features.Contents.Create;

public class ContentCreateHandler : IRequestHandler<ContentCreateCommand, Result<ContentDto>>
{
    private readonly ILogger<ContentCreateHandler> logger;
    private readonly VideoUploadPublisher publisher;
    private readonly IRandomService randomService;
    private readonly IAppDbContext context;
    private readonly IStorage Storage;

    public ContentCreateHandler(IAppDbContext context, VideoUploadPublisher publisher, IRandomService randomService, IStorage Storage, ILogger<ContentCreateHandler> logger)
    {
        this.context = context;

        this.randomService = randomService;
        this.publisher = publisher;
        this.Storage = Storage;
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

        // publishing to rabbit queue

        string? videoPath = null;
        string? photoPath = null;

        if (cmd.ContentFile != null && cmd.ContentFile.Length != 0)
        {
            videoPath = await Storage.SaveFormFileAsync(
                cmd.ContentFile, "Video", token: cancellationToken);
        }

        if (cmd.PrewievPhoto != null && cmd.PrewievPhoto.Length != 0)
        {
            photoPath = await Storage.SaveFormFileAsync(
                cmd.PrewievPhoto, "Images", token: cancellationToken);
        }

        await publisher.PublishAsync(new VideoUploadMessage(
            content.Id, content.Slug, videoPath, photoPath), cancellationToken);

        logger.LogInformation("Content {ContentId} created for user {UserId}",
            content.Id, cmd.UserId);

        return Result<ContentDto>.Success(201, new ContentDto(
            content.Id, content.ChannelId, content.CreatorId,
            content.Title, content.Slug, content.Description,
            content.CreatedDate, content.ContentType.ToString(), 0,
            content.VideoMeta.VideoUrl,
            new ImageDto(
                new ImageVariants(
                    content.VideoMeta.PhotoBase,
                    content.VideoMeta.Small,
                    content.VideoMeta.Medium,
                    content.VideoMeta.Large),
                content.VideoMeta.R,
                content.VideoMeta.G,
                content.VideoMeta.B),
            0, 0, 0, 0, 0));
    }
}