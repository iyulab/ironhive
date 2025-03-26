namespace IronHive.Abstractions.Tools;

public interface IToolManager
{
    Task<string> HandleSetInstructionsAsync(string serviceKey, object? options);

    Task HandleInitializedAsync(string serviceKey, object? options);

    ICollection<ITool> CreateToolCollection(string serviceKey);
}
