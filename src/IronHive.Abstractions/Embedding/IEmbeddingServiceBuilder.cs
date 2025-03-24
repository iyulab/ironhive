namespace IronHive.Abstractions.Embedding;

public interface IEmbeddingServiceBuilder
{
    IEmbeddingServiceBuilder AddConnector(string key, IEmbeddingConnector connector);

    IEmbeddingService Build();
}
