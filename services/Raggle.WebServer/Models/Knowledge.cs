using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Raggle.Server.Web.Models;

public class Knowledge : UserEntity
{
    public string Name { get; set; } = Constants.DefaultKnowledgeName;
    public string Description { get; set; } = string.Empty;
    public string? FilesJson { get; set; } = string.Empty;

    // 이 속성은 DB에 직접 매핑되지 않음
    [NotMapped]
    public ICollection<KnowledgeFile> Files
    {
        get => string.IsNullOrEmpty(FilesJson)
            ? new List<KnowledgeFile>()
            : JsonSerializer.Deserialize<List<KnowledgeFile>>(FilesJson) ?? [];
        set => FilesJson = JsonSerializer.Serialize(value);
    }
}

public class KnowledgeFile
{
    public required string Type { get; set; }
    public required string Name { get; set; }
    public required long Size { get; set; }
}
