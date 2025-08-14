using IronHive.Abstractions;
using IronHive.Abstractions.Tools;

namespace IronHive.Core.Tools;

/// <inheritdoc />
public class ToolPluginManager : IToolPluginManager
{
    private const char Separator = '_';

    public ToolPluginManager()
        : this(Enumerable.Empty<IToolPlugin>())
    { }

    public ToolPluginManager(IEnumerable<IToolPlugin> plugins)
    {
        Plugins = new KeyedCollection<IToolPlugin>(
            plugins,
            plugin => plugin.PluginName);
    }

    /// <inheritdoc />
    public IKeyedCollection<IToolPlugin> Plugins { get; }

    /// <inheritdoc />
    public async Task<IEnumerable<ToolDescriptor>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var tasks = Plugins.GetAll().Select(async kvp =>
        {
            var (pluginName, plugin) = (kvp.Key, kvp.Value);
            var tools = await plugin.ListAsync(cancellationToken);

            // 툴 이름에 플러그인 이름을 접두사로 추가합니다.
            return tools.Select(t => new ToolDescriptor
            {
                Name = t.Name.StartsWith($"{pluginName}{Separator}") 
                    ? t.Name
                    : $"{pluginName}{Separator}{t.Name}",
                Description = t.Description,
                Parameters = t.Parameters,
                RequiresApproval = t.RequiresApproval,
            });
        });

        var result = await Task.WhenAll(tasks);
        return result.SelectMany(t => t);
    }

    /// <inheritdoc />
    public async Task<ToolOutput> InvokeAsync(
        string name,
        ToolInput input,
        CancellationToken cancellationToken = default)
    {
        var parts = name.Split(Separator, 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            throw new ArgumentException($"Invalid tool name '{name}'.");

        if (!Plugins.TryGet(parts[0], out var plugin))
            throw new KeyNotFoundException($"Tool Plugin '{parts[0]}' not found.");

        return await plugin.InvokeAsync(parts[1], input, cancellationToken);
    }
}
