namespace Raggle.Abstractions.Memory;

public interface IMemoryServiceBuilder
{
    void AddStorage();

    IMemoryService Build();
}
