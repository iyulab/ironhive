using Raggle.Abstractions.ChatCompletion.Messages;

namespace Raggle.Server.Entities;

public class SessionEntity
{
    public string Id { get; set; } = $"{Guid.NewGuid():N}";

    public string Title { get; set; } = string.Empty;

    public MessageCollection Messages { get; set; } = new();

    public string? ServiceId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }
}
