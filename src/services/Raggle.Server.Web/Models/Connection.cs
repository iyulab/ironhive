namespace Raggle.Server.Web.Models;

public class Connection : UserEntity
{
    public required string Type { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public required string ConnectionString { get; set; }
}
