using System.Text.Json;

namespace Raggle.Server.API.Models;

public class DataSource
{
    public required Guid ID { get; set; }
    public required Guid UserID { get; set; }
    
    public required string Name { get; set; }
    public string? Description { get; set; }

    public required string Type { get; set; }
    public JsonElement? Details { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class FileDetails
{
    public IEnumerable<FileMeta> Files { get; set; } = [];
}

public class DatabaseDetails
{
    public string ConnectionString { get; set; } = string.Empty;
}

public class OpenApiDetails
{
    public string Schema { get; set; } = string.Empty;
}

public class FileMeta
{
    public required string Type { get; set; }
    public required string Name { get; set; }
    public long Size { get; set; }
}
