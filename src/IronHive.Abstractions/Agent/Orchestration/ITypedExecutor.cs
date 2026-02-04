namespace IronHive.Abstractions.Agent.Orchestration;

/// <summary>
/// 타입 안전한 입출력을 가진 실행기입니다.
/// </summary>
/// <typeparam name="TInput">입력 타입</typeparam>
/// <typeparam name="TOutput">출력 타입</typeparam>
public interface ITypedExecutor<TInput, TOutput>
{
    /// <summary>
    /// Executor 이름
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 입력을 받아 출력을 생성합니다.
    /// </summary>
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken ct = default);
}
