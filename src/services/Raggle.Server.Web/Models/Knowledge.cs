namespace Raggle.Server.Web.Models;

public class Knowledge : UserEntity
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public ICollection<string> Files { get; set; } = [];
}
