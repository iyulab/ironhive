using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;

namespace Raggle.Core;

public class MemoryServiceBuilder
{
    private readonly IServiceProvider? _services;

    public IDocumentStorage? DocumentStorage { get; private set; }
    public IVectorStorage? VectorStorage { get; private set; }
    public IPipelineOrchestrator? PipelineOrchestrator { get; private set; }

    public IEmbeddingService? EmbeddingService { get; private set; }
    public EmbeddingRequest? EmbeddingOptions { get; private set; }

    public IChatCompletionService? ChatCompletionService { get; private set; }
    public ChatCompletionRequest? ChatCompletionOptions { get; private set; }

    public MemoryServiceBuilder(IServiceProvider? services = null)
    {
        _services = services;
    }

    public MemoryServiceBuilder SetDocumentStorage(IDocumentStorage documentStorage)
    {
        DocumentStorage = documentStorage;
        return this;
    }

    public MemoryServiceBuilder SetVectorStorage(IVectorStorage vectorStorage)
    {
        VectorStorage = vectorStorage;
        return this;
    }

    public MemoryServiceBuilder SetEmbeddingService(
        IEmbeddingService embeddingService,
        EmbeddingRequest embeddingOptions)
    {
        throw new NotImplementedException();
    }

    public MemoryServiceBuilder SetChatCompletionService(
        IChatCompletionService chatCompletionService,
        ChatCompletionRequest chatCompletionOptions)
    {
        throw new NotImplementedException();
    }

    public MemoryServiceBuilder SetDefaultPipelineHandler()
    {
        var orchestra = new PipelineOrchestrator(DocumentStorage);
        PipelineOrchestrator = orchestra;
        return this;
    }

    public IMemoryService Build()
    {
        throw new NotImplementedException();
    }
}
