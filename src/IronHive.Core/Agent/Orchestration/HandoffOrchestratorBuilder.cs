using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// 핸드오프 오케스트레이터 빌더
/// </summary>
public class HandoffOrchestratorBuilder
{
    private readonly Dictionary<string, IAgent> _agentMap = [];
    private readonly Dictionary<string, List<HandoffTarget>> _handoffMap = [];
    private string? _initialAgentName;
    private int _maxTransitions = 20;
    private string? _name;
    private TimeSpan _timeout = TimeSpan.FromMinutes(5);
    private TimeSpan _agentTimeout = TimeSpan.FromMinutes(2);
    private Func<string, AgentStepResult, Task<Message?>>? _noHandoffHandler;

    /// <summary>
    /// 에이전트와 핸드오프 대상들을 등록합니다.
    /// </summary>
    public HandoffOrchestratorBuilder AddAgent(IAgent agent, params HandoffTarget[] targets)
    {
        ArgumentNullException.ThrowIfNull(agent);

        if (_agentMap.ContainsKey(agent.Name))
            throw new InvalidOperationException($"Agent '{agent.Name}' is already registered.");

        _agentMap[agent.Name] = agent;
        _handoffMap[agent.Name] = [.. targets];
        return this;
    }

    /// <summary>
    /// 초기 에이전트를 설정합니다.
    /// </summary>
    public HandoffOrchestratorBuilder SetInitialAgent(string agentName)
    {
        _initialAgentName = agentName;
        return this;
    }

    /// <summary>
    /// 최대 전환 횟수를 설정합니다.
    /// </summary>
    public HandoffOrchestratorBuilder SetMaxTransitions(int maxTransitions)
    {
        _maxTransitions = maxTransitions;
        return this;
    }

    /// <summary>
    /// 오케스트레이터 이름을 설정합니다.
    /// </summary>
    public HandoffOrchestratorBuilder SetName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// 전체 타임아웃을 설정합니다.
    /// </summary>
    public HandoffOrchestratorBuilder SetTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    /// <summary>
    /// 개별 에이전트 타임아웃을 설정합니다.
    /// </summary>
    public HandoffOrchestratorBuilder SetAgentTimeout(TimeSpan agentTimeout)
    {
        _agentTimeout = agentTimeout;
        return this;
    }

    /// <summary>
    /// 핸드오프 없을 때의 핸들러를 설정합니다.
    /// </summary>
    public HandoffOrchestratorBuilder SetNoHandoffHandler(
        Func<string, AgentStepResult, Task<Message?>> handler)
    {
        _noHandoffHandler = handler;
        return this;
    }

    /// <summary>
    /// 핸드오프 오케스트레이터를 빌드합니다.
    /// </summary>
    public HandoffOrchestrator Build()
    {
        // 유효성 검증
        if (_agentMap.Count == 0)
            throw new InvalidOperationException("At least one agent must be registered.");

        if (string.IsNullOrEmpty(_initialAgentName))
            throw new InvalidOperationException("Initial agent must be specified.");

        if (!_agentMap.ContainsKey(_initialAgentName))
            throw new InvalidOperationException($"Initial agent '{_initialAgentName}' is not registered.");

        // 모든 handoff 대상이 실제 에이전트인지 확인
        foreach (var (agentName, targets) in _handoffMap)
        {
            foreach (var target in targets)
            {
                if (!_agentMap.ContainsKey(target.AgentName))
                {
                    throw new InvalidOperationException(
                        $"Handoff target '{target.AgentName}' from agent '{agentName}' is not a registered agent.");
                }
            }
        }

        var options = new HandoffOrchestratorOptions
        {
            Name = _name,
            Timeout = _timeout,
            AgentTimeout = _agentTimeout,
            InitialAgentName = _initialAgentName,
            MaxTransitions = _maxTransitions,
            NoHandoffHandler = _noHandoffHandler
        };

        return new HandoffOrchestrator(options, new Dictionary<string, IAgent>(_agentMap), new Dictionary<string, List<HandoffTarget>>(_handoffMap));
    }
}
