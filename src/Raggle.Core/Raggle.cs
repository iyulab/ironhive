using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Tools;
using Raggle.Core.Memory;

namespace Raggle.Core;

public class Raggle : IRaggle
{
    public required IServiceProvider Services { get; init; }

    public T GetRaggleService<T>(object key)
    {
        var service = Services.GetKeyedService<T>(key)
            ?? throw new InvalidOperationException($"Service of type {typeof(T).Name} with key {key} not found.");
        return service;
    }

    public IEnumerable<T> GetRaggleServices<T>()
    {
        var services = Services.GetKeyedServices<T>(KeyedService.AnyKey);
        return services;
    }

    public IRaggleMemory CreateMemory(RaggleMemoryConfig config)
    {
        throw new NotImplementedException();
    }

    #region 버리기

    //public IChatCompletionService GetChatCompletionService(object key)
    //    => Services.GetRequiredKeyedService<IChatCompletionService>(key);

    //public IEnumerable<IChatCompletionService> GetChatCompletionServices()
    //    => Services.GetKeyedServices<IChatCompletionService>(KeyedService.AnyKey);

    //public IEmbeddingService GetEmbeddingService(object key)
    //    => Services.GetRequiredKeyedService<IEmbeddingService>(key);

    //public IEnumerable<IEmbeddingService> GetEmbeddingServices()
    //    => Services.GetKeyedServices<IEmbeddingService>(KeyedService.AnyKey);

    //public IDocumentStorage GetDocumentStorage(object key)
    //    => Services.GetRequiredKeyedService<IDocumentStorage>(key);

    //public IEnumerable<IDocumentStorage> GetDocumentStorages()
    //    => Services.GetKeyedServices<IDocumentStorage>(KeyedService.AnyKey);

    //public IVectorStorage GetVectorStorage(object key)
    //    => Services.GetRequiredKeyedService<IVectorStorage>(key);

    #endregion
}
