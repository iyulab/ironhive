using IronHive.Abstractions;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.ChatCompletion.Tools;

namespace IronHive.Core.ChatCompletion;

public class ChatCompletionServiceBuilder : IChatCompletionServiceBuilder
{
    private readonly Dictionary<string, IChatCompletionConnector> _connectors = new();
    private readonly Dictionary<string, FunctionToolCollection> _tools = new();
    private IServiceModelParser _parser = new ServiceModelParser();

    public IChatCompletionServiceBuilder AddConnector(string key, IChatCompletionConnector connector)
    {
        _connectors.Add(key, connector);
        return this;
    }

    public IChatCompletionServiceBuilder AddTool<TService>(string key, TService? implementation = null)
        where TService : class
    {
        var collection = implementation == null
                ? FunctionToolFactory.CreateFromObject<TService>()
                : FunctionToolFactory.CreateFromObject(implementation);

        _tools.Add(key, new FunctionToolCollection(collection));
        return this;
    }

    public IChatCompletionServiceBuilder WithParser(IServiceModelParser parser)
    {
        _parser = parser;
        return this;
    }

    public IChatCompletionService Build()
    {
        return new ChatCompletionService(
            connectors: _connectors, 
            tools: _tools,
            parser: _parser);
    }
}
