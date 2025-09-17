using IronHive.Abstractions.Workflow;

namespace IronHive.Abstractions.Memory;

/// <summary>
/// 파이프라인의 최소 단위 작업을 정의합니다.
/// </summary>
/// <typeparam name="TOptions">파이프라인 작업에 필요한 옵션/파라미터 DTO 타입.</typeparam>
public interface IMemoryPipeline<TOptions> : IWorkflowTask<MemoryContext, TOptions>
    where TOptions : notnull
{ }

/// <summary>
/// 옵션이 필요 없는 파이프라인의 최소 단위 작업을 정의합니다.
/// </summary>
public interface IMemoryPipeline : IWorkflowTask<MemoryContext>
{ }
