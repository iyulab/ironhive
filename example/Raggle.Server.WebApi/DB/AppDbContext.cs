using Microsoft.EntityFrameworkCore;
using Raggle.Server.WebApi.Models;

namespace Raggle.Server.WebApi.DB;

public class AppDbContext : DbContext
{
    public DbSet<Collection> Collections { get; set; } = null!;
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<Assistant> Memories { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Collection>().HasIndex(c => c.Id).IsUnique();
        builder.Entity<Document>().HasIndex(c => c.Id).IsUnique();
        builder.Entity<Assistant>().HasIndex(c => c.Id).IsUnique();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
}
