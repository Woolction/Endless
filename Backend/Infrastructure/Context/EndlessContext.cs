using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces;
using Domain.Entities;

namespace Infrastructure.Context;

public class EndlessContext : DbContext, IAppDbContext
{
    public EndlessContext(DbContextOptions<EndlessContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<UserFollowing> UserFollowings { get; set; }
    public DbSet<UserInteractionContent> UserInteractionContents { get; set; }

    public DbSet<Channel> Channels { get; set; }
    public DbSet<ChannelOwner> ChannelOwners { get; set; }
    public DbSet<ChannelSubscription> ChannelSubscriptions { get; set; }

    public DbSet<Content> Contents { get; set; }
    public DbSet<SavedContent> SavedContents { get; set; }
    public DbSet<LikedContent> LikedContents { get; set; }
    public DbSet<DisLikedContent> DisLikedContents { get; set; }

    public DbSet<Genre> Genres { get; set; }
    public DbSet<GenreInfo> GenreInfos { get; set; }
    public DbSet<UserGenreVector> UserVectors { get; set; }
    public DbSet<ContentGenreVector> ContentVectors { get; set; }

    public DbSet<VideoMetaData> VideoMetas { get; set; }

    public DbSet<Comment> Comments { get; set; }
    public DbSet<LikedComment> LikedComments { get; set; }
    public DbSet<DisLikedComment> DisLikedComments { get; set; }

    public async Task<int> SaveChangesAsync()
    {
        return await base.SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // User
        EntityTypeBuilder<User> userBuilder = builder.Entity<User>();
        userBuilder
            .HasIndex(u => u.Email)
            .IsUnique();
        userBuilder
            .HasIndex(u => u.Slug)
            .IsUnique();
        userBuilder
            .HasIndex(u => u.Name)
            .IsUnique();
        userBuilder
            .OwnsOne(u => u.RefreshToken);

        EntityTypeBuilder<UserFollowing> userSubBuilder = builder.Entity<UserFollowing>();
        userSubBuilder
            .HasOne(uS => uS.Follower)
            .WithMany(u => u.Followers)
            .HasForeignKey(uS => uS.FollowerId);
        userSubBuilder
            .HasOne(uS => uS.FollowedUser)
            .WithMany(u => u.Following)
            .HasForeignKey(uS => uS.FollowedUserId);
        userSubBuilder
            .HasKey(uS => new { uS.FollowerId, uS.FollowedUserId });
        userSubBuilder
            .HasIndex(uS => uS.FollowedUserId);

        EntityTypeBuilder<UserInteractionContent> userInBuilder = builder.Entity<UserInteractionContent>();
        userInBuilder
            .HasOne(uI => uI.Content)
            .WithMany(c => c.UsersInteration)
            .HasForeignKey(i => i.ContentId);
        userInBuilder
            .HasOne(uI => uI.User)
            .WithMany(u => u.UserInterations)
            .HasForeignKey(i => i.UserId);
        userInBuilder
            .HasKey(uI => new { uI.UserId, uI.ContentId });
        userInBuilder
            .HasIndex(uI => uI.ContentId);

        // Channel
        EntityTypeBuilder<Channel> ChannelBuilder = builder.Entity<Channel>();
        ChannelBuilder
            .HasIndex(d => d.Name)
            .IsUnique();
        ChannelBuilder
            .HasIndex(d => d.Slug)
            .IsUnique();

        EntityTypeBuilder<ChannelOwner> ChannelOwnerBuilder = builder.Entity<ChannelOwner>();
        ChannelOwnerBuilder
            .HasOne(o => o.Owner)
            .WithMany(u => u.OwnedChannels)
            .HasForeignKey(o => o.OwnerId);
        ChannelOwnerBuilder
            .HasOne(o => o.Channel)
            .WithMany(d => d.Owners)
            .HasForeignKey(o => o.ChannelId);
        ChannelOwnerBuilder
            .HasKey(o => new { o.OwnerId, o.ChannelId });
        ChannelOwnerBuilder
            .HasIndex(o => o.ChannelId);

        EntityTypeBuilder<ChannelSubscription> ChannelsubBuilder = builder.Entity<ChannelSubscription>();
        ChannelsubBuilder
            .HasOne(dS => dS.Subscriber)
            .WithMany(d => d.SubscripedChannels)
            .HasForeignKey(uS => uS.SubscriberId);
        ChannelsubBuilder
            .HasOne(dS => dS.Channel)
            .WithMany(d => d.Subscribers)
            .HasForeignKey(uS => uS.ChannelId);
        ChannelsubBuilder
            .HasKey(dS => new { dS.SubscriberId, dS.ChannelId });
        ChannelsubBuilder
            .HasIndex(dS => dS.ChannelId);

        // Content
        EntityTypeBuilder<Content> contentBuilder = builder.Entity<Content>();
        contentBuilder
            .HasOne(c => c.Creator)
            .WithMany(u => u.Contents)
            .HasForeignKey(c => c.CreatorId);
        contentBuilder
            .HasOne(c => c.Channel)
            .WithMany(d => d.Contents)
            .HasForeignKey(c => c.ChannelId)
            .IsRequired(false);
        contentBuilder
            .HasIndex(c => c.Slug)
            .IsUnique();
        contentBuilder
            .HasIndex(c => c.CreatedDate);
        contentBuilder
            .HasIndex(c => c.Title)
            .IsUnique();
        contentBuilder
            .HasIndex(c => c.RandomKey);

        EntityTypeBuilder<SavedContent> savedCBuilder = builder.Entity<SavedContent>();
        savedCBuilder
            .HasOne(sC => sC.User)
            .WithMany(u => u.SavedContents)
            .HasForeignKey(sC => sC.UserId);
        savedCBuilder
            .HasOne(sC => sC.Content)
            .WithMany(c => c.Savers)
            .HasForeignKey(sC => sC.ContentId);
        savedCBuilder
            .HasKey(sC => new { sC.UserId, sC.ContentId });
        savedCBuilder
            .HasIndex(sC => sC.ContentId);

        EntityTypeBuilder<LikedContent> likedCBuilder = builder.Entity<LikedContent>();
        likedCBuilder
            .HasOne(lC => lC.User)
            .WithMany(u => u.LikedContents)
            .HasForeignKey(lC => lC.UserId);
        likedCBuilder
            .HasOne(lC => lC.Content)
            .WithMany(c => c.Likers)
            .HasForeignKey(lC => lC.ContentId);
        likedCBuilder
            .HasKey(lC => new { lC.UserId, lC.ContentId });
        likedCBuilder
            .HasIndex(lC => lC.ContentId);

        EntityTypeBuilder<DisLikedContent> DizLikedCBuilder = builder.Entity<DisLikedContent>();
        DizLikedCBuilder
            .HasOne(lC => lC.User)
            .WithMany(u => u.DisLikedContents)
            .HasForeignKey(lC => lC.UserId);
        DizLikedCBuilder
            .HasOne(lC => lC.Content)
            .WithMany(c => c.DisLikers)
            .HasForeignKey(lC => lC.ContentId);
        DizLikedCBuilder
            .HasKey(lC => new { lC.UserId, lC.ContentId });
        DizLikedCBuilder
            .HasIndex(lC => lC.ContentId);

        // Genre
        EntityTypeBuilder<Genre> genreBuilder = builder.Entity<Genre>();
        genreBuilder
            .HasIndex(g => g.Name)
            .IsUnique();
        genreBuilder.HasData([
            new Genre { Id = Guid.Parse("018f47ac-8b72-7c2f-b8d1-9f3c2e7a6d11"), Name = "Vlog", Order = 0 },
            new Genre { Id = Guid.Parse("018f47ac-8b72-7c30-a2f4-6b1d9c8e2a55"), Name = "Gaming", Order = 1 },
            new Genre { Id = Guid.Parse("018f47ac-8b72-7c31-91aa-3e7f5b2c4d88"), Name = "Tutorial", Order = 2 },
            new Genre { Id = Guid.Parse("018f47ac-8b72-7c32-b7c1-0a9d6e4f2b33"), Name = "Review", Order = 3 },
            new Genre { Id = Guid.Parse("018f47ac-8b72-7c33-8d2e-5c1a9b7f3e66"), Name = "Education", Order = 4 },
            new Genre { Id = Guid.Parse("018f47ac-8b72-7c34-a9f0-2d6c8b1e4a99"), Name = "Tech", Order = 5 }
        ]);

        // Genre Info
        EntityTypeBuilder<GenreInfo> genreInfoBuilder = builder.Entity<GenreInfo>();
        genreInfoBuilder
            .HasData(new GenreInfo() { Id = Guid.Parse("018f47ac-8b72-7abc-8def-1234567890ab"), Count = 6 });

        EntityTypeBuilder<UserGenreVector> genreUsBuilder = builder.Entity<UserGenreVector>();
        genreUsBuilder
            .HasOne(gU => gU.User)
            .WithMany(gU => gU.Vectors)
            .HasForeignKey(gU => gU.UserId);
        genreUsBuilder
            .HasOne(gU => gU.Genre)
            .WithMany()
            .HasForeignKey(gU => gU.GenreId);
        genreUsBuilder
            .HasKey(gU => new { gU.UserId, gU.GenreId });
        genreUsBuilder
            .HasIndex(gU => gU.GenreId);

        EntityTypeBuilder<ContentGenreVector> genreCoBuilder = builder.Entity<ContentGenreVector>();
        genreCoBuilder
            .HasOne(gC => gC.Content)
            .WithMany(c => c.Vectors)
            .HasForeignKey(gC => gC.ContentId);
        genreCoBuilder
            .HasOne(gC => gC.Genre)
            .WithMany()
            .HasForeignKey(gC => gC.GenreId);
        genreCoBuilder
            .HasKey(gC => new { gC.ContentId, gC.GenreId });
        genreCoBuilder
            .HasIndex(gC => gC.GenreId);

        // Meta
        EntityTypeBuilder<VideoMetaData> videoMetaBuilder = builder.Entity<VideoMetaData>();
        videoMetaBuilder
            .HasOne(v => v.Content)
            .WithOne(c => c.VideoMeta)
            .HasForeignKey<VideoMetaData>(v => v.ContentId);
        videoMetaBuilder
            .HasKey(v => v.ContentId);

        // Comment
        EntityTypeBuilder<Comment> commentBuilder = builder.Entity<Comment>();
        commentBuilder
            .HasOne(co => co.Commentator)
            .WithMany(u => u.Comments)
            .HasForeignKey(co => co.CommentatorId);
        commentBuilder.
            HasIndex(co => co.CommentatorId);
        commentBuilder
            .HasOne(co => co.Content)
            .WithMany(c => c.Comments)
            .HasForeignKey(co => co.ContentId);
        commentBuilder
            .HasIndex(co => co.ContentId);
        commentBuilder
            .HasOne(co => co.Parent)
            .WithMany(co => co.Comments)
            .HasForeignKey(co => co.ParentId)
            .IsRequired(false);
        commentBuilder
            .HasIndex(co => co.PublicatedDate);

        EntityTypeBuilder<LikedComment> commentLikedBuilder = builder.Entity<LikedComment>();
        commentLikedBuilder
            .HasOne(cL => cL.User)
            .WithMany(u => u.LikedComments)
            .HasForeignKey(cL => cL.UserId);
        commentLikedBuilder
            .HasOne(cL => cL.Comment)
            .WithMany(co => co.Likers)
            .HasForeignKey(cL => cL.CommentId);
        commentLikedBuilder
            .HasKey(cl => new { cl.UserId, cl.CommentId });
        commentLikedBuilder
            .HasIndex(cl => cl.CommentId);

        EntityTypeBuilder<DisLikedComment> commentDisLikedBuilder = builder.Entity<DisLikedComment>();
        commentDisLikedBuilder
            .HasOne(cL => cL.User)
            .WithMany(u => u.DisLikedComments)
            .HasForeignKey(cL => cL.UserId);
        commentDisLikedBuilder
            .HasOne(cL => cL.Comment)
            .WithMany(co => co.DisLikers)
            .HasForeignKey(cL => cL.CommentId);
        commentDisLikedBuilder
            .HasKey(cl => new { cl.UserId, cl.CommentId });
        commentDisLikedBuilder
            .HasIndex(cl => cl.CommentId);
    }
}