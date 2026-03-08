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
    
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        EntityTypeBuilder<User> userBuilder = builder.Entity<User>();
        userBuilder.HasMany(u => u.Subscribes).WithMany(u => u.Subscriptions).UsingEntity(j => j.ToTable("UserSubscription"));
        userBuilder.OwnsOne(u => u.RefreshToken);

        EntityTypeBuilder<Domain> domainBuilder = builder.Entity<Domain>();
        domainBuilder.HasMany(d => d.SignedUsers).WithMany(u => u.SignedDomains).UsingEntity(j => j.ToTable("DomainSigned"));

        EntityTypeBuilder<DomainOwner> domainOwnerBuilder = builder.Entity<DomainOwner>();
        domainOwnerBuilder.HasOne(o => o.User).WithMany(u => u.OwnedDomains).HasForeignKey(o => o.UserId);
        domainOwnerBuilder.HasOne(o => o.Domain).WithMany(d => d.Owners).HasForeignKey(o => o.DomainId);
        domainOwnerBuilder.HasKey(o => new { o.UserId, o.DomainId });

        EntityTypeBuilder<Content> contentBuilder = builder.Entity<Content>();
        contentBuilder.HasOne(c => c.Creator).WithMany(u => u.Contents).HasForeignKey(c => c.CreatorId);
        contentBuilder.HasOne(c => c.Domain).WithMany(d => d.Contents).HasForeignKey(c => c.DomainId);

        EntityTypeBuilder<SavedContent> savedCBuilder = builder.Entity<SavedContent>();
        savedCBuilder.HasOne(sC => sC.Owner).WithMany(u => u.SavedContents).HasForeignKey(sC => sC.OwnerId);
        savedCBuilder.HasOne(sC => sC.Content).WithMany(c => c.ContentSaveds).HasForeignKey(sC => sC.ContentId);
        savedCBuilder.HasKey(sC => new { sC.OwnerId, sC.ContentId });

        EntityTypeBuilder<LikedContent> likedCBuilder = builder.Entity<LikedContent>();
        likedCBuilder.HasOne(lC => lC.Owner).WithMany(u => u.LikedContents).HasForeignKey(lC => lC.OwnerId);
        likedCBuilder.HasOne(lC => lC.Content).WithMany(c => c.ContentLikeds).HasForeignKey(lC => lC.ContentId);
        likedCBuilder.HasKey(lC => new { lC.OwnerId, lC.ContentId });

        EntityTypeBuilder<Comment> commentBuilder = builder.Entity<Comment>();
        commentBuilder.HasOne(co => co.Commentator).WithMany(u => u.Comments).HasForeignKey(co => co.CommentatorId);
        commentBuilder.HasOne(co => co.Content).WithMany(c => c.Comments).HasForeignKey(co => co.ContentId);
    }
}