using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Workflow;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TaskNode), "task")]
[JsonDerivedType(typeof(ConditionNode), "condition")]
[JsonDerivedType(typeof(ParallelNode), "parallel")]
[JsonDerivedType(typeof(EndNode), "end")]
public abstract class WorkflowNode
{
    // 그래프 노드의 식별자
    public string? Id { get; init; }
}

public sealed class TaskNode : WorkflowNode
{
    // 실행할 step 이름 (레지스트리에 등록된 이름)
    public required string Step { get; init; }

    // 스텝에서 사용할 옵션 (정의 시점에 자유형으로 넣고 런타임에 TOptions로 역직렬화)
    public object? With { get; init; }
}

public sealed class ConditionNode : WorkflowNode
{
    // 분기 판단을 수행할 step 이름 (IWorkflowCondition)
    public required string Step { get; init; }

    // 분기키(string) → 다음 실행 노드들(순차 실행)
    public required Dictionary<string, IEnumerable<WorkflowNode>> Branches { get; init; } = new();
}

public enum JoinMethod
{
    // 모든 브랜치가 성공해야 다음으로 진행 (예외 발생 시 중단)
    WaitAll,

    // 하나라도 성공하면 다음으로 진행(남은 작업 계속 수행)
    WaitAny,
}

public enum ContextMethod
{
    // 각 브랜치가 복사된 컨텍스트에서 실행됨 (기본값)
    Copyed,

    // 모든 브랜치가 동일한 컨텍스트를 공유함 (주의: 동시성 문제 발생 가능)
    Shared,
}

public sealed class ParallelNode : WorkflowNode
{
    // 병합 방식
    public JoinMethod JoinMode { get; init; } = JoinMethod.WaitAll;

    // 컨텍스트 모드
    public ContextMethod ContextMode { get; init; } = ContextMethod.Copyed;

    // 병렬 실행할 각 브랜치의 노드들(순차 실행)
    public required IEnumerable<IEnumerable<WorkflowNode>> Branches { get; init; } = [];
}

public sealed class EndNode : WorkflowNode
{ }
