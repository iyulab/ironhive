namespace IronHive.Abstractions.Memory;

public interface IMemoryServiceBuilder
{
    void AddStorage();

    IMemoryService Build();
}
