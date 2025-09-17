using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Workflow;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TaskNode), "task")]
[JsonDerivedType(typeof(ConditionNode), "condition")]
[JsonDerivedType(typeof(ParallelNode), "parallel")]
public abstract record WorkflowNode
{
    /// <summary>노드 식별자</summary>
    public string? Id { get; init; }
}

/// <summary>작업(step)을 실행하는 노드</summary>
public sealed record TaskNode : WorkflowNode
{
    /// <summary>실행할 step 이름</summary>
    public required string Step { get; init; }

    /// <summary>스텝 옵션 (런타임에 역직렬화)</summary>
    public object? With { get; init; }
}

/// <summary>조건에 따라 다음 노드를 고르는 분기 노드</summary>
public sealed record ConditionNode : WorkflowNode
{
    /// <summary>분기 판단 step 이름 (IWorkflowCondition)</summary>
    public required string Step { get; init; }

    /// <summary>분기 키 → 실행 노드들(순차)</summary>
    public required Dictionary<string, IEnumerable<WorkflowNode>> Branches { get; init; } = new();

    /// <summary>해당 키가 없을 때 실행할 기본 브랜치</summary>
    public IEnumerable<WorkflowNode>? DefaultBranch { get; init; }
}

/// <summary>병렬 병합 방식</summary>
public enum JoinMode
{
    /// <summary>모두 성공 시 진행 (실패 시 중단)</summary>
    WaitAll,

    /// <summary>하나라도 성공하면 진행 (나머지는 계속)</summary>
    WaitAny,
}

/// <summary>병렬 시 컨텍스트 처리 방식</summary>
public enum ContextMode
{
    /// <summary>브랜치마다 복사된 컨텍스트</summary>
    Copied,

    /// <summary>모든 브랜치가 동일 컨텍스트 공유</summary>
    Shared,
}

/// <summary>병렬 실행 노드</summary>
public sealed record ParallelNode : WorkflowNode
{
    /// <summary>병합 방식</summary>
    public required JoinMode Join { get; init; }

    /// <summary>컨텍스트 처리 방식</summary>
    public required ContextMode Context { get; init; }

    /// <summary>각 브랜치의 실행 노드들(순차)</summary>
    public required IEnumerable<IEnumerable<WorkflowNode>> Branches { get; init; } = [];
}
