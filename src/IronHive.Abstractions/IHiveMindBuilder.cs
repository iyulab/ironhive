using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;

namespace IronHive.Abstractions;

public interface IHiveMindBuilder
{
    /// <summary>
    /// Register a keyed service to IHiveServiceRegistry.
    /// </summary>
    IHiveMindBuilder AddKeyedService<TService>(string serviceKey, TService instance)
        where TService : class;

    IHiveMindBuilder AddChatCompletionConnector(string serviceKey, IChatCompletionConnector connector);

    IHiveMindBuilder AddEmbeddingConnector(string serviceKey, IEmbeddingConnector connector);

    IHiveMindBuilder AddFileStorage(string serviceKey, IFileStorage storage);

    /// <summary>
    /// Build the IHiveMind.
    /// </summary>
    //IHiveMind Build();
}
