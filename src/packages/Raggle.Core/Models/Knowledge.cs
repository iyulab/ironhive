namespace Raggle.Server.Models;

public class Knowledge
{
	public required Guid ID { get; set; } = Guid.NewGuid();
	public required string Name { get; set; }
	public string? Description { get; set; }
	public required IEnumerable<Guid> Documents { get; set; } = [];
}
