namespace IronHive.Abstractions.Memory;

/// <summary>
/// Represents a worker that processes data pipelines.
/// </summary>
public interface IPipelineWorker
{
    /// <summary>
    /// 큐에서 지속적으로 데이터 파이프라인을 가져와 처리합니다.
    /// </summary>
    /// <param name="cancellationToken">
    /// 작업을 취소할 수 있는 토큰입니다.
    /// </param>
    Task StartAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 데이터 파이프라인을 실행합니다.
    /// </summary>
    /// <param name="pipeline">
    /// 데이터를 처리하는 파이프라인입니다.
    /// </param>
    Task RunPipelineAsync(
        PipelineContext pipeline,
        CancellationToken cancellationToken = default);
}
