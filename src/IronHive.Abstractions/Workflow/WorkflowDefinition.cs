namespace IronHive.Abstractions.Workflow;

/// <summary>
/// 작업 흐름(워크플로우)을 정의하는 클래스입니다.
/// </summary>
public sealed class WorkflowDefinition
{
    // 작업 식별자 (관리용)
    public string? Id { get; set; }

    // 작업 버전 (관리용)
    public Version? Version { get; set; }

    // 노드들 (순차 실행)
    public List<WorkflowNode> Steps { get; set; } = [];

    /// <summary>
    /// 작업 정의서의 유효성을 검사합니다.
    /// </summary>
    public void Validate()
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var n in EnumerateRecursively(Steps))
        {
            if (string.IsNullOrWhiteSpace(n.Id))
                continue;

            if (!ids.Add(n.Id))
                throw new InvalidOperationException($"중복된 node Id 발견: '{n.Id}'");
        }
        // 필요 시 더 깊은 검증 추가(빈 브랜치 등)
    }

    /// <summary>
    /// 노드들을 순회하면서 모든 Node들을 열거합니다.
    /// </summary>
    public static IEnumerable<WorkflowNode> EnumerateRecursively(IEnumerable<WorkflowNode> seq)
    {
        foreach (var n in seq)
        {
            if (n is TaskNode t)
            {
                yield return t;
            }
            else if (n is ConditionNode c)
            {
                foreach (var b in c.Branches.Values)
                    foreach (var node in EnumerateRecursively(b)) yield return node;
            }
            else if (n is ParallelNode p)
            {
                foreach (var b in p.Branches)
                    foreach (var node in EnumerateRecursively(b)) yield return node;
            }
        }
    }
}
