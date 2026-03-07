using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.API.Data.Context;

public class EndlessContext : DbContext
{
    public EndlessContext(DbContextOptions<EndlessContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Domain> Domains { get; set; }
    public DbSet<Content> Contents { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        EntityTypeBuilder<User> userBuilder = builder.Entity<User>();
        userBuilder.HasMany(u => u.Subscribes).WithMany(u => u.Subscriptions).UsingEntity(j => j.ToTable("UserSubscription"));
        userBuilder.OwnsOne(u => u.RefreshToken);

        EntityTypeBuilder<Domain> domainBuilder = builder.Entity<Domain>();
        domainBuilder.HasMany(d => d.SignedUsers).WithMany(u => u.SignedDomains).UsingEntity(j => j.ToTable("DomainSigned"));
        domainBuilder.HasMany(d => d.Owners).WithMany(u => u.Domains).UsingEntity(j => j.ToTable("DomainsOwners"));

        EntityTypeBuilder<Content> contentBuilder = builder.Entity<Content>();
        contentBuilder.HasOne(c => c.Creator).WithMany(u => u.Contents).HasForeignKey(c => c.CreatorId);
        contentBuilder.HasOne(c => c.Domain).WithMany(d => d.Contents).HasForeignKey(c => c.DomainId);

        EntityTypeBuilder<SavedContent> savedCBuilder = builder.Entity<SavedContent>();
        savedCBuilder.HasOne(sC => sC.Owner).WithMany(u => u.SavedContents).HasForeignKey(sC => sC.OwnerId);
        savedCBuilder.HasOne(sC => sC.Content).WithMany().HasForeignKey(sC => sC.ContentId);

        EntityTypeBuilder<LikedContent> likedCBuilder = builder.Entity<LikedContent>();
        likedCBuilder.HasOne(sC => sC.Owner).WithMany(u => u.LikedContents).HasForeignKey(sC => sC.OwnerId);
        likedCBuilder.HasOne(sC => sC.Content).WithMany().HasForeignKey(sC => sC.ContentId);

        EntityTypeBuilder<Comment> commentBuilder = builder.Entity<Comment>();
        commentBuilder.HasOne(co => co.Commentator).WithMany(u => u.Comments).HasForeignKey(co => co.CommentatorId);
        commentBuilder.HasOne(co => co.Content).WithMany(c => c.Comments).HasForeignKey(co => co.ContentId);
    }
}