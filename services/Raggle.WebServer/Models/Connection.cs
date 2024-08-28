using System.Text.Json.Serialization;

namespace Raggle.Server.Web.Models;

public class Connection : UserEntity
{
    public required ConnectionType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public required string ConnectionString { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConnectionType
{
    SqlServer,
    MongoDB,
}