namespace Raggle.Source;

public interface IDataSource
{
    Guid ID { get; set; }
    string Name { get; set; }
    string? Description { get; set; }
}
