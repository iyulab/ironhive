using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions;
using Raggle.Abstractions.Embedding;

namespace Raggle.Core.Embedding;

public class EmbeddingService : IEmbeddingService
{
    private readonly IServiceProvider _service;

    public EmbeddingService(IKeyedServiceProvider service)
    {
        _service = service;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var services = _service.GetKeyedServices<IEmbeddingAdapter>(KeyedService.AnyKey);
        var models = new List<EmbeddingModel>();

        foreach (var service in services)
        {
            var serviceKey = ServiceKeyRegistry.Get(service.GetType());
            var serviceModels = await service.GetModelsAsync(cancellationToken);

            models.AddRange(serviceModels.Select(x => new EmbeddingModel
            {
                Model = $"{serviceKey}/{x.Model}",
            }));
        }

        return models;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<float>> EmbedAsync(
        string model,
        string input,
        CancellationToken cancellationToken = default)
    {
        var res = await EmbedBatchAsync(new EmbeddingsRequest
        {
            Model = model,
            Input = [input]
        }, cancellationToken);

        return res.Embeddings?.First().Embedding 
            ?? throw new InvalidOperationException("Embedding is null");
    }

    /// <inheritdoc />
    public async Task<EmbeddingsResponse> EmbedBatchAsync(
        EmbeddingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var serviceKey = request.Model.Split('/', 2)[0];
        var model = request.Model.Split('/', 2)[1];
        var adapter = _service.GetRequiredKeyedService<IEmbeddingAdapter>(serviceKey);
        request.Model = model;

        return await adapter.EmbedBatchAsync(request, cancellationToken);
    }
}
