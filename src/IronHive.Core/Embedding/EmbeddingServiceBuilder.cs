using IronHive.Abstractions.Embedding;

namespace IronHive.Core.Embedding;

public class EmbeddingServiceBuilder : IEmbeddingServiceBuilder
{
    private readonly Dictionary<string, IEmbeddingConnector> _connectors = new();

    public IEmbeddingServiceBuilder AddConnector(string key, IEmbeddingConnector connector)
    {
        _connectors.Add(key, connector);
        return this;
    }

    public IEmbeddingService Build()
    {
        return new EmbeddingService(connectors: _connectors);
    }
}
