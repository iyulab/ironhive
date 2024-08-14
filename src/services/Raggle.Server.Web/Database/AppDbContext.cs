using Raggle.Server.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Raggle.Server.Web.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Assistant> Assistants { get; set; }
    public DbSet<Knowledge> Knowledges { get; set; }
    public DbSet<Connection> Connections { get; set; }
    public DbSet<OpenAPI> OpenAPIs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primary Keys 설정
        modelBuilder.Entity<User>()
            .HasKey(u => u.ID);

        modelBuilder.Entity<Assistant>()
            .HasKey(a => a.ID);

        modelBuilder.Entity<Knowledge>()
            .HasKey(k => k.ID);

        modelBuilder.Entity<Connection>()
            .HasKey(c => c.ID);

        modelBuilder.Entity<OpenAPI>()
            .HasKey(o => o.ID);

        // 관계 설정
        modelBuilder.Entity<Assistant>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserID);

        modelBuilder.Entity<Knowledge>()
            .HasOne(k => k.User)
            .WithMany()
            .HasForeignKey(k => k.UserID);

        modelBuilder.Entity<Connection>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserID);

        modelBuilder.Entity<OpenAPI>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserID);
    }
}
