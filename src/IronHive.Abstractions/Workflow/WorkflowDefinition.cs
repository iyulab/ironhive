namespace IronHive.Abstractions.Workflow;

/// <summary>
/// 작업 흐름(워크플로우)을 정의하는 클래스입니다.
/// </summary>
public sealed class WorkflowDefinition
{
    // 작업 이름 (관리용)
    public string? Name { get; set; }

    // 작업 버전 (관리용)
    public Version? Version { get; set; }

    // 노드들 (순차 실행)
    public List<WorkflowNode> Steps { get; set; } = [];

    /// <summary>
    /// 작업 정의서의 유효성을 검사합니다. 모든 노드의 ID가 고유한지 확인합니다.
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
    }

    /// <summary>
    /// 모든 노드를 재귀적으로 순회하면서 열거합니다.
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
                yield return c;
                foreach (var b in c.Branches.Values)
                    foreach (var node in EnumerateRecursively(b)) yield return node;
            }
            else if (n is ParallelNode p)
            {
                yield return p;
                foreach (var b in p.Branches)
                    foreach (var node in EnumerateRecursively(b)) yield return node;
            }
        }
    }
}
