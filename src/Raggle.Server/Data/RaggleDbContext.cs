using Microsoft.EntityFrameworkCore;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Assistant;
using Raggle.Server.Entities;

namespace Raggle.Server.Data;

public class RaggleDbContext : DbContext
{
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
        
        // if sqlite
        if (true)
        {
            OnSqliteModelCreating(builder);
        }
    }

    private void OnSqliteModelCreating(ModelBuilder builder)
    {
        // CollectionEntity 설정
        builder.Entity<CollectionEntity>(entity =>
        {
            // 테이블 이름 설정
            entity.ToTable("collections");

            // 기본 키 설정
            entity.HasKey(e => e.Id);

            // 속성 설정
            entity.Property(e => e.EmbedService)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(e => e.EmbedModel)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(e => e.CreatedAt)
                  .IsRequired();

            entity.Property(e => e.HandlerOptions)
                  .HasConversion<JsonValueConverter<IDictionary<string, object>>>()
                  .HasColumnType("TEXT");

            // 참조 속성 설정
            entity.HasMany(e => e.Documents)
                  .WithOne(d => d.Collection)
                  .HasForeignKey(d => d.CollectionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // DocumentEntity 설정
        builder.Entity<DocumentEntity>(entity =>
        {
            // 테이블 이름 설정
            entity.ToTable("documents");

            // 기본 키 설정 (Id로 설정)
            entity.HasKey(d => d.Id);

            // 속성 설정
            entity.Property(d => d.FileName)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(d => d.ContentType)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(d => d.CreatedAt)
                  .IsRequired();

            entity.Property(d => d.Tags)
                  .HasConversion<JsonValueConverter<IEnumerable<string>>>()
                  .HasColumnType("TEXT");
        });

        // AssistantEntity 설정
        builder.Entity<AssistantEntity>(entity =>
        {
            // 테이블 이름 설정
            entity.ToTable("assistants");

            // 기본 키 설정
            entity.HasKey(a => a.Id);

            // 속성 설정

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(e => e.CreatedAt)
                  .IsRequired();

            entity.Property(a => a.Options)
                  .HasConversion<JsonValueConverter<AssistantOptions>>()
                  .HasColumnType("TEXT");

            entity.Property(a => a.Tools)
                  .HasConversion<JsonValueConverter<IEnumerable<string>>>()
                  .HasColumnType("TEXT");

            entity.Property(a => a.ToolOptions)
                  .HasConversion<JsonValueConverter<IDictionary<string, object>>>()
                  .HasColumnType("TEXT");
        });
    }

    private void OnSqlServerModelCreating(ModelBuilder builder)
    {
    }

    private void OnCosmosModelCreating(ModelBuilder builder)
    {
    }

    private void OnPostgreSQLModelCreating(ModelBuilder builder)
    {
    }

    private void OnMySqlModelCreating(ModelBuilder builder)
    {
    }

    private void OnMongoDBModelCreating(ModelBuilder builder)
    {
    }

    private void OnOracleModelCreating(ModelBuilder builder)
    {
    }
}
