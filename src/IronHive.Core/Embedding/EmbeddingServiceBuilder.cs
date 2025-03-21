using IronHive.Abstractions;
using IronHive.Abstractions.Embedding;

namespace IronHive.Core.Embedding;

public class EmbeddingServiceBuilder : IEmbeddingServiceBuilder
{
    private readonly Dictionary<string, IEmbeddingConnector> _connectors = new();
    private IServiceModelParser _parser = new ServiceModelParser();

    public IEmbeddingServiceBuilder AddConnector(string key, IEmbeddingConnector connector)
    {
        _connectors.Add(key, connector);
        return this;
    }

    public IEmbeddingServiceBuilder WithParser(IServiceModelParser parser)
    {
        _parser = parser;
        return this;
    }

    public IEmbeddingService Build()
    {
        return new EmbeddingService(
            connectors: _connectors,
            parser: _parser);
    }
}
