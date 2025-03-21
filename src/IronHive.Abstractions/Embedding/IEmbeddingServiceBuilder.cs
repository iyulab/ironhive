namespace IronHive.Abstractions.Embedding;

public interface IEmbeddingServiceBuilder
{
    IEmbeddingServiceBuilder AddConnector(string key, IEmbeddingConnector connector);

    IEmbeddingServiceBuilder WithParser(IServiceModelParser parser);

    IEmbeddingService Build();
}
