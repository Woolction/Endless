using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Models;

namespace Backend.API.Data.Context;

public class EndlessContext : DbContext
{
    public EndlessContext(DbContextOptions<EndlessContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<UserFollowing> UserFollowings { get; set; }
    public DbSet<UserInterationContent> UserInterationContents { get; set; }
    
    public DbSet<Domain> Domains { get; set; }
    public DbSet<DomainOwner> DomainOwners { get; set; }
    public DbSet<DomainSubscription> DomainSubscriptions { get; set; }

    public DbSet<Content> Contents { get; set; }
    public DbSet<SavedContent> SavedContents { get; set; }
    public DbSet<LikedContent> LikedContents { get; set; }
    public DbSet<DizLikedContent> DizLikedContents { get; set; } 

    public DbSet<Genre> Genres { get; set; }
    public DbSet<GenreInfo> GenreInfos { get; set; }
    public DbSet<UserGenreVector> UserVectors { get; set; }
    public DbSet<ContentGenreVector> ContentVectors { get; set; }

    public DbSet<VideoMetaData> VideoMetas { get; set; }
    
    public DbSet<Comment> Comments { get; set; }
    public DbSet<LikedComment> LikedComments { get; set; }
    public DbSet<DizLikedComment> DizLikedComments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Extension
        builder.HasPostgresExtension("pg_trgm");
        builder.HasPostgresExtension("fuzzystrmatch");

        // User
        EntityTypeBuilder<User> userBuilder = builder.Entity<User>();
        userBuilder.HasIndex(u => u.Email).IsUnique();
        userBuilder.HasIndex(u => u.Slug).IsUnique();
        userBuilder.HasIndex(u => u.Name)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
        userBuilder.OwnsOne(u => u.RefreshToken);

        EntityTypeBuilder<UserFollowing> userSubBuilder = builder.Entity<UserFollowing>();
        userSubBuilder.HasOne(uS => uS.Follower).WithMany(u => u.Followers).HasForeignKey(uS => uS.FollowerId);
        userSubBuilder.HasOne(uS => uS.FollowedUser).WithMany(u => u.Following).HasForeignKey(uS => uS.FollowedUserId);
        userSubBuilder.HasKey(uS => new { uS.FollowerId, uS.FollowedUserId });
        userSubBuilder.HasIndex(uS => uS.FollowedUserId);

        EntityTypeBuilder<UserInterationContent> userInBuilder = builder.Entity<UserInterationContent>();
        userInBuilder.HasOne(uI => uI.Content).WithMany(c => c.UsersInteration).HasForeignKey(i => i.ContentId);
        userInBuilder.HasOne(uI => uI.User).WithMany(u => u.UserInterations).HasForeignKey(i => i.UserId);
        userInBuilder.HasKey(uI => new { uI.UserId, uI.ContentId });
        userInBuilder.HasIndex(uI => uI.ContentId);

        // Domain
        EntityTypeBuilder<Domain> domainBuilder = builder.Entity<Domain>();
        domainBuilder.HasIndex(d => d.Name)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
        domainBuilder.HasIndex(d => d.Slug)
            .IsUnique();

        EntityTypeBuilder<DomainOwner> domainOwnerBuilder = builder.Entity<DomainOwner>();
        domainOwnerBuilder.HasOne(o => o.Owner).WithMany(u => u.OwnedDomains).HasForeignKey(o => o.OwnerId);
        domainOwnerBuilder.HasOne(o => o.Domain).WithMany(d => d.Owners).HasForeignKey(o => o.DomainId);
        domainOwnerBuilder.HasKey(o => new { o.OwnerId, o.DomainId });
        domainOwnerBuilder.HasIndex(o => o.DomainId);

        EntityTypeBuilder<DomainSubscription> domainSubBuilder = builder.Entity<DomainSubscription>();
        domainSubBuilder.HasOne(dS => dS.Subscriber).WithMany(d => d.SubscripedDomains).HasForeignKey(uS => uS.SubscriberId);
        domainSubBuilder.HasOne(dS => dS.Domain).WithMany(d => d.Subscribers).HasForeignKey(uS => uS.DomainId);
        domainSubBuilder.HasKey(dS => new { dS.SubscriberId, dS.DomainId });
        domainSubBuilder.HasIndex(dS => dS.DomainId);

        // Content
        EntityTypeBuilder<Content> contentBuilder = builder.Entity<Content>();
        contentBuilder.HasOne(c => c.Creator).WithMany(u => u.Contents).HasForeignKey(c => c.CreatorId);
        contentBuilder.HasOne(c => c.Domain).WithMany(d => d.Contents).HasForeignKey(c => c.DomainId);
        contentBuilder.HasIndex(c => c.Slug).IsUnique();
        contentBuilder.HasIndex(c => c.CreatedDate);
        contentBuilder.HasIndex(c => c.Title)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
        contentBuilder.HasIndex(c => c.RandomKey);

        EntityTypeBuilder<SavedContent> savedCBuilder = builder.Entity<SavedContent>();
        savedCBuilder.HasOne(sC => sC.User).WithMany(u => u.SavedContents).HasForeignKey(sC => sC.UserId);
        savedCBuilder.HasOne(sC => sC.Content).WithMany(c => c.Savers).HasForeignKey(sC => sC.ContentId);
        savedCBuilder.HasKey(sC => new { sC.UserId, sC.ContentId });
        savedCBuilder.HasIndex(sC => sC.ContentId);

        EntityTypeBuilder<LikedContent> likedCBuilder = builder.Entity<LikedContent>();
        likedCBuilder.HasOne(lC => lC.User).WithMany(u => u.LikedContents).HasForeignKey(lC => lC.UserId);
        likedCBuilder.HasOne(lC => lC.Content).WithMany(c => c.Likers).HasForeignKey(lC => lC.ContentId);
        likedCBuilder.HasKey(lC => new { lC.UserId, lC.ContentId });
        likedCBuilder.HasIndex(lC => lC.ContentId);

        EntityTypeBuilder<DizLikedContent> DizLikedCBuilder = builder.Entity<DizLikedContent>();
        DizLikedCBuilder.HasOne(lC => lC.User).WithMany(u => u.DizLikedContents).HasForeignKey(lC => lC.UserId);
        DizLikedCBuilder.HasOne(lC => lC.Content).WithMany(c => c.DizLikers).HasForeignKey(lC => lC.ContentId);
        DizLikedCBuilder.HasKey(lC => new { lC.UserId, lC.ContentId });
        DizLikedCBuilder.HasIndex(lC => lC.ContentId);

        // Genre
        EntityTypeBuilder<Genre> genreBuilder = builder.Entity<Genre>();
        genreBuilder.HasIndex(g => g.Name)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        EntityTypeBuilder<UserGenreVector> genreUsBuilder = builder.Entity<UserGenreVector>();
        genreUsBuilder.HasOne(gU => gU.User).WithMany(gU => gU.Vectors).HasForeignKey(gU => gU.UserId);
        genreUsBuilder.HasOne(gU => gU.Genre).WithMany().HasForeignKey(gU => gU.GenreId);
        genreUsBuilder.HasKey(gU => new { gU.UserId, gU.GenreId });
        genreUsBuilder.HasIndex(gU => gU.GenreId);

        EntityTypeBuilder<ContentGenreVector> genreCoBuilder = builder.Entity<ContentGenreVector>();
        genreCoBuilder.HasOne(gC => gC.Content).WithMany(c => c.Vectors).HasForeignKey(gC => gC.ContentId);
        genreCoBuilder.HasOne(gC => gC.Genre).WithMany().HasForeignKey(gC => gC.GenreId);
        genreCoBuilder.HasKey(gC => new { gC.ContentId, gC.GenreId });
        genreCoBuilder.HasIndex(gC => gC.GenreId);

        // Meta
        EntityTypeBuilder<VideoMetaData> videoMetaBuilder = builder.Entity<VideoMetaData>();
        videoMetaBuilder.HasOne(v => v.Content).WithOne(c => c.VideoMeta).HasForeignKey<VideoMetaData>(v => v.ContentId);
        videoMetaBuilder.HasKey(v => v.ContentId);

        // Comment
        EntityTypeBuilder<Comment> commentBuilder = builder.Entity<Comment>();
        commentBuilder.HasOne(co => co.Commentator).WithMany(u => u.Comments).HasForeignKey(co => co.CommentatorId);
        commentBuilder.HasOne(co => co.Content).WithMany(c => c.Comments).HasForeignKey(co => co.ContentId);
        commentBuilder.HasIndex(co => co.PublicatedDate);

        EntityTypeBuilder<LikedComment> commentLikedBuilder = builder.Entity<LikedComment>();
        commentLikedBuilder.HasOne(cL => cL.User).WithMany(u => u.LikedComments).HasForeignKey(cL => cL.UserId);
        commentLikedBuilder.HasOne(cL => cL.Comment).WithMany(co => co.Likers).HasForeignKey(cL => cL.CommentId);
        commentLikedBuilder.HasKey(cl => new { cl.UserId, cl.CommentId });

        EntityTypeBuilder<DizLikedComment> commentDizLikedBuilder = builder.Entity<DizLikedComment>();
        commentDizLikedBuilder.HasOne(cL => cL.User).WithMany(u => u.DizLikedComments).HasForeignKey(cL => cL.UserId);
        commentDizLikedBuilder.HasOne(cL => cL.Comment).WithMany(co => co.DizLikers).HasForeignKey(cL => cL.CommentId);
        commentDizLikedBuilder.HasKey(cl => new { cl.UserId, cl.CommentId });
    }
}