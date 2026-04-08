using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Domain.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; set; }
    DbSet<UserFollowing> UserFollowings { get; set; }
    DbSet<UserInteractionContent> UserInteractionContents { get; set; }

    DbSet<Channel> Channels { get; set; }
    DbSet<ChannelOwner> ChannelOwners { get; set; }
    DbSet<ChannelSubscription> ChannelSubscriptions { get; set; }

    DbSet<Content> Contents { get; set; }
    DbSet<SavedContent> SavedContents { get; set; }
    DbSet<LikedContent> LikedContents { get; set; }
    DbSet<DisLikedContent> DisLikedContents { get; set; }

    DbSet<Genre> Genres { get; set; }
    DbSet<GenreInfo> GenreInfos { get; set; }
    DbSet<UserGenreVector> UserVectors { get; set; }
    DbSet<ContentGenreVector> ContentVectors { get; set; }

    DbSet<VideoMetaData> VideoMetas { get; set; }

    DbSet<Comment> Comments { get; set; }
    DbSet<LikedComment> LikedComments { get; set; }
    DbSet<DisLikedComment> DisLikedComments { get; set; }

    Task<int> SaveChangesAsync();
}