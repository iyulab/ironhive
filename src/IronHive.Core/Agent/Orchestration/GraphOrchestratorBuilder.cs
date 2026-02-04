using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// 그래프 오케스트레이터를 구성하는 빌더입니다.
/// </summary>
public class GraphOrchestratorBuilder
{
    private readonly Dictionary<string, AgentGraphNode> _nodes = new();
    private readonly List<AgentGraphEdge> _edges = [];
    private string? _startNodeId;
    private string? _outputNodeId;
    private GraphOrchestratorOptions? _options;

    /// <summary>
    /// 오케스트레이터 옵션을 설정합니다.
    /// </summary>
    public GraphOrchestratorBuilder WithOptions(GraphOrchestratorOptions options)
    {
        _options = options;
        return this;
    }

    /// <summary>
    /// 그래프에 노드를 추가합니다.
    /// </summary>
    public GraphOrchestratorBuilder AddNode(string id, IAgent agent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(agent);

        _nodes[id] = new AgentGraphNode { Id = id, Agent = agent };
        return this;
    }

    /// <summary>
    /// 그래프에 엣지를 추가합니다.
    /// </summary>
    public GraphOrchestratorBuilder AddEdge(string from, string to, Func<AgentStepResult, bool>? condition = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(from);
        ArgumentException.ThrowIfNullOrWhiteSpace(to);

        _edges.Add(new AgentGraphEdge
        {
            SourceId = from,
            TargetId = to,
            Condition = condition
        });
        return this;
    }

    /// <summary>
    /// 시작 노드를 설정합니다.
    /// </summary>
    public GraphOrchestratorBuilder SetStartNode(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        _startNodeId = id;
        return this;
    }

    /// <summary>
    /// 출력 노드를 설정합니다.
    /// </summary>
    public GraphOrchestratorBuilder SetOutputNode(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        _outputNodeId = id;
        return this;
    }

    /// <summary>
    /// 그래프를 검증하고 오케스트레이터를 빌드합니다.
    /// </summary>
    public GraphOrchestrator Build()
    {
        // 노드 존재 확인
        if (_nodes.Count == 0)
            throw new InvalidOperationException("Graph must have at least one node.");

        if (_startNodeId == null)
            throw new InvalidOperationException("Start node must be set.");

        if (_outputNodeId == null)
            throw new InvalidOperationException("Output node must be set.");

        if (!_nodes.ContainsKey(_startNodeId))
            throw new InvalidOperationException($"Start node '{_startNodeId}' not found in graph.");

        if (!_nodes.ContainsKey(_outputNodeId))
            throw new InvalidOperationException($"Output node '{_outputNodeId}' not found in graph.");

        // 엣지의 노드 참조 확인
        foreach (var edge in _edges)
        {
            if (!_nodes.ContainsKey(edge.SourceId))
                throw new InvalidOperationException($"Edge source '{edge.SourceId}' not found in graph.");
            if (!_nodes.ContainsKey(edge.TargetId))
                throw new InvalidOperationException($"Edge target '{edge.TargetId}' not found in graph.");
        }

        // 순환 감지 (Kahn 알고리즘)
        ValidateNoCycles();

        return new GraphOrchestrator(
            _options ?? new GraphOrchestratorOptions(),
            _nodes,
            _edges,
            _startNodeId,
            _outputNodeId);
    }

    private void ValidateNoCycles()
    {
        var inDegree = new Dictionary<string, int>();
        var adjacency = new Dictionary<string, List<string>>();

        foreach (var nodeId in _nodes.Keys)
        {
            inDegree[nodeId] = 0;
            adjacency[nodeId] = [];
        }

        foreach (var edge in _edges)
        {
            adjacency[edge.SourceId].Add(edge.TargetId);
            inDegree[edge.TargetId]++;
        }

        var queue = new Queue<string>();
        foreach (var kvp in inDegree)
        {
            if (kvp.Value == 0)
                queue.Enqueue(kvp.Key);
        }

        var visited = 0;
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            visited++;

            foreach (var neighbor in adjacency[current])
            {
                inDegree[neighbor]--;
                if (inDegree[neighbor] == 0)
                    queue.Enqueue(neighbor);
            }
        }

        if (visited != _nodes.Count)
        {
            throw new InvalidOperationException("Graph contains a cycle. Only DAG (Directed Acyclic Graph) is supported.");
        }
    }
}
