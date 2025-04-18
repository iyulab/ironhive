namespace IronHive.Abstractions.Models;

/// <summary>
/// 모델 정보를 가져오는 인터페이스입니다.
/// </summary>
public interface IModelConnector
{
    /// <summary>
    /// 지원하는 모델의 목록을 가져옵니다.
    /// </summary>
    Task<IEnumerable<ModelDescriptor>> ListModelsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정 식별자에 해당하는 모델의 정보를 가져옵니다.
    /// </summary>
    /// <param name="modelId">The ID of the model.</param>
    Task<ModelDescriptor?> GetModelAsync(
        string modelId, 
        CancellationToken cancellationToken = default);
}
