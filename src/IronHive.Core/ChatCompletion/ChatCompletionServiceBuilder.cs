using IronHive.Abstractions;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.ChatCompletion.Tools;

namespace IronHive.Core.ChatCompletion;

public class ChatCompletionServiceBuilder : IChatCompletionServiceBuilder
{
    private readonly Dictionary<string, IChatCompletionConnector> _connectors = new();
    private readonly Dictionary<string, IToolService> _tools = new();
    private readonly Dictionary<string, Action<ChatCompletionRequest, object>> _toolOptions = new();

    public IChatCompletionServiceBuilder AddConnector(string key, IChatCompletionConnector connector)
    {
        _connectors.Add(key, connector);
        return this;
    }

    public IChatCompletionServiceBuilder AddTool(string key, IToolService service)
    {
        _tools.Add(key, service);
        return this;
    }

    public IChatCompletionService Build()
    {
        return new ChatCompletionService(
            connectors: _connectors,
            tools: _tools);
    }
}
