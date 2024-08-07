using System.Text.Json.Serialization;

namespace Raggle.Server.API.Models;

[JsonDerivedType(typeof(FileSource), "file")]
[JsonDerivedType(typeof(DatabaseSource), "sqlserver")]
[JsonDerivedType(typeof(OpenApiSource), "openapi")]
public interface IDataSource
{
    public Guid ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public abstract class DataSource : IDataSource
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; }
}

public class FileSource : DataSource
{
    public IEnumerable<FileMeta> Files { get; set; }
}

public class DatabaseSource : DataSource
{
    public string ConnectionString { get; set; }
}

public class OpenApiSource : DataSource
{
    public string Schema { get; set; }
}

public class FileMeta
{
    public string Type { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public string Extension { get; set; }
    public long Size { get; set; }
}