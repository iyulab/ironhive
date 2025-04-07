using Microsoft.EntityFrameworkCore;
using WebApiSample.Entities;

namespace WebApiSample.Services;

public class AppDbContext : DbContext
{
    public DbSet<MemoryCollection> Collections { get; set; }
    public DbSet<MemoryFile> Files { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=app.db;Cache=Shared;Pooling=true;Foreign Keys=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MemoryCollection>()
            .HasKey(c => c.Id);
        
        modelBuilder.Entity<MemoryFile>()
            .HasKey(f => f.Id);
    }
}
