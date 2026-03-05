using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Model;
using Microsoft.AspNetCore.SignalR;

namespace Backend.API.Data.Context;

public class EndlessContext : DbContext
{
    public EndlessContext(DbContextOptions options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Content> Contents { get; set; }
}