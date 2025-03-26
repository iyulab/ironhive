namespace IronHive.Abstractions.Tools;

public interface IToolManager
{
    IToolHandler GetToolService(string key);

    ICollection<ITool> CreateFromObject<T>(params object[] parameters) where T : class;

    ICollection<ITool> CreateFromObject(object instance);

    ITool CreateFromFunction(string name, string? description, Delegate function);
}
