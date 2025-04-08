using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebApiSample.Settings;

namespace WebApiSample.Data;

public class AppDbContext : DbContext
{
    public DbSet<FileEntity> Files { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            options.UseSqlite(new SqliteConnectionStringBuilder
            {
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared,
                DataSource = AppConstants.FileDatabasePath,
            }.ConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        // FileEntity에 대한 테이블 설정
        model.Entity<FileEntity>()
            .ToTable("Files");

        // FileEntity의 Id를 기본 키로 설정
        model.Entity<FileEntity>()
             .HasKey(e => e.Id);

        // EmbeddingModel에 인덱스 설정
        model.Entity<FileEntity>()
            .HasIndex(e => e.EmbeddingModel);

        // Status에 인덱스 설정
        model.Entity<FileEntity>()
            .HasIndex(e => e.Status);

        // FilePath에 인덱스 설정
        model.Entity<FileEntity>()
            .HasIndex(e => e.FilePath);

        // Enum 값을 String으로 저장하도록 설정
        model.Entity<FileEntity>()
            .Property(e => e.Status)
            .HasConversion<string>();
    }
}
