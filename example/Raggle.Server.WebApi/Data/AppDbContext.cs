using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Raggle.Server.WebApi.Models;
using System.Text.Json;

namespace Raggle.Server.WebApi.Data;

public class AppDbContext : DbContext
{
    private static readonly ValueConverter<IDictionary<string, object>?, string?> DictionaryConverter =
        new(v => v == null ? null : JsonSerializer.Serialize(v, new JsonSerializerOptions()),
            v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions()));

    private static readonly ValueConverter<IEnumerable<string>?, string?> EnumerableConverter =
        new(v => v == null ? null : JsonSerializer.Serialize(v, new JsonSerializerOptions()),
            v => v == null ? null : JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions()));

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

        // 키 설정
        builder.Entity<CollectionModel>()
            .HasKey(c => c.CollectionId);

        builder.Entity<DocumentModel>()
            .HasKey(d => d.DocumentId);

        builder.Entity<AssistantModel>()
            .HasKey(a => a.AssistantId);

        // 타입 설정
        builder.Entity<CollectionModel>()
            .Property(c => c.HandlerOptions)
            .HasConversion(DictionaryConverter)
            .HasColumnType("TEXT");

        builder.Entity<DocumentModel>()
            .Property(d => d.Tags)
            .HasConversion(EnumerableConverter)
            .HasColumnType("TEXT");

        builder.Entity<AssistantModel>()
            .Property(a => a.ToolkitOptions)
            .HasConversion(DictionaryConverter)
            .HasColumnType("TEXT");

        builder.Entity<AssistantModel>()
            .Property(a => a.Memories)
            .HasConversion(EnumerableConverter)
            .HasColumnType("TEXT");

        builder.Entity<AssistantModel>()
            .Property(a => a.ToolKits)
            .HasConversion(EnumerableConverter)
            .HasColumnType("TEXT");

        // 관계 설정: 외래 키
        builder.Entity<DocumentModel>()
            .HasOne(d => d.Collection)
            .WithMany(c => c.Documents)
            .HasForeignKey(d => d.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
