using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions;
using Raggle.Abstractions.Embedding;

namespace Raggle.Core.Embedding;

public class EmbeddingService : IEmbeddingService
{
    private readonly IServiceProvider _service;
    private readonly IServiceModelConverter _converter;

    public EmbeddingService(IServiceProvider service)
    {
        _service = service;
        _converter = service.GetRequiredService<IServiceModelConverter>();
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
                Model = _converter.Format(serviceKey, x.Model),
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
        var (serviceKey, model) = _converter.Parse(request.Model);
        var adapter = _service.GetRequiredKeyedService<IEmbeddingAdapter>(serviceKey);
        request.Model = model;

        return await adapter.EmbedBatchAsync(request, cancellationToken);
    }
}
