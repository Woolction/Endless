using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Models;
using Microsoft.AspNetCore.SignalR;

namespace Backend.API.Data.Context;

public class EndlessContext : DbContext
{
    public EndlessContext(DbContextOptions<EndlessContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Content> Contents { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        //builder.Entity<User>().Property(p => p.Id).ValueGeneratedNever();
        builder.Entity<User>().OwnsOne(c => c.RefreshToken);
    }
}