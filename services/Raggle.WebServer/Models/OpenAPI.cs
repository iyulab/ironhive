namespace Raggle.Server.Web.Models;

public class OpenAPI : UserEntity
{
    public required string SchemaType { get; set; } = string.Empty;
    public required string Schema { get; set; } = string.Empty;
}
