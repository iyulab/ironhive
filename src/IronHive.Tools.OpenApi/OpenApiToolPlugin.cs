using IronHive.Abstractions.Tools;

namespace IronHive.Tools.OpenApi;

public class OpenApiToolPlugin : IToolPlugin
{
    public required string PluginName { get; init; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public Task<IEnumerable<ToolDescriptor>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ToolOutput> InvokeAsync(
        string name, 
        ToolInput input, 
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
