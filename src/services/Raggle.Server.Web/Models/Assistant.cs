using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Raggle.Server.Web.Models;

public class Assistant : UserEntity
{
    public string Name { get; set; } = Constants.DefaultAssistantName;
    public string Instruction { get; set; } = string.Empty;
    public ICollection<Guid> Knowledges { get; set; } = [];
    public ICollection<Guid> Connections { get; set; } = [];
    public ICollection<Guid> OpenAPIs { get; set; } = [];
    public string ChatHistoryJson { get; set; } = string.Empty;

    // 이 속성은 DB에 직접 매핑되지 않음
    [NotMapped]
    public ChatHistory? ChatHistory
    {
        get => string.IsNullOrEmpty(ChatHistoryJson)
            ? new ChatHistory()
            : JsonSerializer.Deserialize<ChatHistory>(ChatHistoryJson);
        set => ChatHistoryJson = JsonSerializer.Serialize(value);
    }
}
