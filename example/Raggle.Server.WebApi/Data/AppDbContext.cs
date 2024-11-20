using Microsoft.EntityFrameworkCore;
using Raggle.Server.WebApi.Models;

namespace Raggle.Server.WebApi.Data;

public class AppDbContext : DbContext
{
    public DbSet<CollectionModel> Collections { get; set; }
    public DbSet<DocumentModel> Documents { get; set; }
    public DbSet<AssistantModel> Assistants { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<CollectionModel>().HasIndex(c => c.Id).IsUnique();
        builder.Entity<DocumentModel>().HasIndex(c => c.Id).IsUnique();
        builder.Entity<AssistantModel>().HasIndex(c => c.Id).IsUnique();
    }

}
