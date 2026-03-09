using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Models;

namespace Backend.API.Data.Context;

public class EndlessContext : DbContext
{
    public EndlessContext(DbContextOptions<EndlessContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    
    public DbSet<Domain> Domains { get; set; }
    public DbSet<DomainOwner> DomainOwners { get; set; }

    public DbSet<Content> Contents { get; set; }
    public DbSet<SavedContent> SavedContents { get; set; }
    public DbSet<LikedContent> LikedContents { get; set; }

    public DbSet<VideoMetaData> VideoMetas { get; set; }
    
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        EntityTypeBuilder<User> userBuilder = builder.Entity<User>();
        userBuilder.HasIndex(u => u.Email).IsUnique();
        userBuilder.HasIndex(u => u.Slug).IsUnique();
        userBuilder.HasIndex(u => u.Name).IsUnique();
        userBuilder.OwnsOne(u => u.RefreshToken);

        EntityTypeBuilder<Domain> domainBuilder = builder.Entity<Domain>();
        domainBuilder.HasIndex(d => d.Name).IsUnique();
        domainBuilder.HasIndex(d => d.Slug).IsUnique();

        EntityTypeBuilder<DomainOwner> domainOwnerBuilder = builder.Entity<DomainOwner>();
        domainOwnerBuilder.HasOne(o => o.Owner).WithMany(u => u.OwnedDomains).HasForeignKey(o => o.OwnerId);
        domainOwnerBuilder.HasOne(o => o.Domain).WithMany(d => d.Owners).HasForeignKey(o => o.DomainId);
        domainOwnerBuilder.HasKey(o => new { o.OwnerId, o.DomainId });

        EntityTypeBuilder<Content> contentBuilder = builder.Entity<Content>();
        contentBuilder.HasOne(c => c.Creator).WithMany(u => u.Contents).HasForeignKey(c => c.CreatorId);
        contentBuilder.HasOne(c => c.Domain).WithMany(d => d.Contents).HasForeignKey(c => c.DomainId);
        contentBuilder.HasIndex(c => c.Slug).IsUnique();
        contentBuilder.HasIndex(c => c.CreatedDate);
        contentBuilder.HasIndex(c => c.Title);

        EntityTypeBuilder<SavedContent> savedCBuilder = builder.Entity<SavedContent>();
        savedCBuilder.HasOne(sC => sC.Owner).WithMany(u => u.SavedContents).HasForeignKey(sC => sC.OwnerId);
        savedCBuilder.HasOne(sC => sC.Content).WithMany(c => c.ContentSaveds).HasForeignKey(sC => sC.ContentId);
        savedCBuilder.HasKey(sC => new { sC.OwnerId, sC.ContentId });

        EntityTypeBuilder<LikedContent> likedCBuilder = builder.Entity<LikedContent>();
        likedCBuilder.HasOne(lC => lC.Owner).WithMany(u => u.LikedContents).HasForeignKey(lC => lC.OwnerId);
        likedCBuilder.HasOne(lC => lC.Content).WithMany(c => c.ContentLikeds).HasForeignKey(lC => lC.ContentId);
        likedCBuilder.HasKey(lC => new { lC.OwnerId, lC.ContentId });

        EntityTypeBuilder<VideoMetaData> videoMetaBuilder = builder.Entity<VideoMetaData>();
        videoMetaBuilder.HasOne(v => v.Content).WithOne(c => c.VideoMeta).HasForeignKey<VideoMetaData>(v => v.ContentId);
        videoMetaBuilder.HasKey(v => v.ContentId);

        EntityTypeBuilder<Comment> commentBuilder = builder.Entity<Comment>();
        commentBuilder.HasOne(co => co.Commentator).WithMany(u => u.Comments).HasForeignKey(co => co.CommentatorId);
        commentBuilder.HasOne(co => co.Content).WithMany(c => c.Comments).HasForeignKey(co => co.ContentId);
        commentBuilder.HasIndex(co => co.PublicatedDate);

        EntityTypeBuilder<DomainSubscription> domainSubBuilder = builder.Entity<DomainSubscription>();
        domainSubBuilder.HasOne(dS => dS.SubscribedUser).WithMany(d => d.DomainSubscriptions).HasForeignKey(uS => uS.SubscriberId);
        domainSubBuilder.HasOne(dS => dS.Domain).WithMany(d => d.Subsrcibers).HasForeignKey(uS => uS.DomainId);
        domainSubBuilder.HasKey(dS => new { dS.SubscriberId, dS.DomainId });

        EntityTypeBuilder<UserSubscribtion> userSubBuilder = builder.Entity<UserSubscribtion>();
        userSubBuilder.HasOne(uS => uS.FollowedUser).WithMany(u => u.Followers).HasForeignKey(uS => uS.FollowerId);
        userSubBuilder.HasOne(uS => uS.User).WithMany(u => u.Following).HasForeignKey(uS => uS.UserId);
        userSubBuilder.HasKey(uS => new { uS.FollowerId, uS.UserId });
    }
}