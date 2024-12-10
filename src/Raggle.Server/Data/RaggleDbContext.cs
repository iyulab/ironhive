using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Raggle.Server.Entities;
using System.Text.Json;

namespace Raggle.Server.Data;

public class RaggleDbContext : DbContext
{
    private static readonly ValueConverter<IDictionary<string, object>?, string?> DictionaryConverter =
        new(v => v == null ? null : JsonSerializer.Serialize(v, new JsonSerializerOptions()),
            v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions()));

    private static readonly ValueConverter<IEnumerable<string>?, string?> EnumerableConverter =
        new(v => v == null ? null : JsonSerializer.Serialize(v, new JsonSerializerOptions()),
            v => v == null ? null : JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions()));

    public DbSet<CollectionEntity> Collections { get; set; }
    public DbSet<DocumentEntity> Documents { get; set; }
    public DbSet<AssistantEntity> Assistants { get; set; }

    public RaggleDbContext(DbContextOptions<RaggleDbContext> options) : base(options)
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
        builder.Entity<CollectionEntity>()
            .HasKey(c => c.CollectionId);

        builder.Entity<DocumentEntity>()
            .HasKey(d => d.DocumentId);

        builder.Entity<AssistantEntity>()
            .HasKey(a => a.AssistantId);

        // 타입 설정
        builder.Entity<CollectionEntity>()
            .Property(c => c.HandlerOptions)
            .HasConversion(DictionaryConverter)
            .HasColumnType("TEXT");

        builder.Entity<DocumentEntity>()
            .Property(d => d.Tags)
            .HasConversion(EnumerableConverter)
            .HasColumnType("TEXT");

        builder.Entity<AssistantEntity>()
            .Property(a => a.ToolOptions)
            .HasConversion(DictionaryConverter)
            .HasColumnType("TEXT");

        builder.Entity<AssistantEntity>()
            .Property(a => a.Memories)
            .HasConversion(EnumerableConverter)
            .HasColumnType("TEXT");

        builder.Entity<AssistantEntity>()
            .Property(a => a.Tools)
            .HasConversion(EnumerableConverter)
            .HasColumnType("TEXT");

        // 관계 설정: 외래 키
        builder.Entity<DocumentEntity>()
            .HasOne(d => d.Collection)
            .WithMany(c => c.Documents)
            .HasForeignKey(d => d.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
