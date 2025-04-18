namespace IronHive.Abstractions.Models;

/// <summary>
/// 지원하는 모델의 정보를 가져오는 서비스 인터페이스입니다.
/// </summary>
public interface IModelService
{
    /// <summary>
    /// Retrieves a list of all available models.
    /// </summary>
    Task<IEnumerable<ModelDescriptor>> ListModelsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a model descriptor by its ID.
    /// </summary>
    /// <param name="provider">The service key of the model provider</param>
    /// <param name="modelId">The ID of the model.</param>
    Task<ModelDescriptor?> GetModelAsync(
        string provider,
        string modelId, 
        CancellationToken cancellationToken = default);
}
