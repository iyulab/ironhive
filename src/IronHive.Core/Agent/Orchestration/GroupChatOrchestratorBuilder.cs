using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// GroupChat 오케스트레이터 빌더
/// </summary>
public class GroupChatOrchestratorBuilder
{
    private readonly Dictionary<string, IAgent> _agentMap = [];
    private ISpeakerSelector? _speakerSelector;
    private ITerminationCondition? _terminationCondition;
    private int _maxRounds = 50;
    private string? _name;
    private TimeSpan _timeout = TimeSpan.FromMinutes(5);
    private TimeSpan _agentTimeout = TimeSpan.FromMinutes(2);
    private ICheckpointStore? _checkpointStore;
    private string? _orchestrationId;
    private Func<string, AgentStepResult?, Task<bool>>? _approvalHandler;
    private HashSet<string>? _requireApprovalForAgents;

    /// <summary>
    /// 에이전트를 추가합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder AddAgent(IAgent agent)
    {
        ArgumentNullException.ThrowIfNull(agent);

        if (_agentMap.ContainsKey(agent.Name))
            throw new InvalidOperationException($"Agent '{agent.Name}' is already registered.");

        _agentMap[agent.Name] = agent;
        return this;
    }

    /// <summary>
    /// 라운드 로빈 발언자 선택을 설정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder WithRoundRobin()
    {
        _speakerSelector = new RoundRobinSpeakerSelector();
        return this;
    }

    /// <summary>
    /// 랜덤 발언자 선택을 설정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder WithRandom()
    {
        _speakerSelector = new RandomSpeakerSelector();
        return this;
    }

    /// <summary>
    /// LLM 관리자 기반 발언자 선택을 설정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder WithLlmManager(IAgent manager)
    {
        _speakerSelector = new LlmSpeakerSelector(manager);
        return this;
    }

    /// <summary>
    /// 커스텀 발언자 선택기를 설정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder WithSpeakerSelector(ISpeakerSelector selector)
    {
        _speakerSelector = selector ?? throw new ArgumentNullException(nameof(selector));
        return this;
    }

    /// <summary>
    /// 키워드 종료 조건을 설정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder TerminateOnKeyword(string keyword)
    {
        _terminationCondition = new KeywordTermination(keyword);
        return this;
    }

    /// <summary>
    /// 라운드 수 종료 조건을 설정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder TerminateAfterRounds(int maxRounds)
    {
        _terminationCondition = new MaxRoundsTermination(maxRounds);
        return this;
    }

    /// <summary>
    /// 토큰 예산 종료 조건을 설정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder TerminateOnTokenBudget(int maxTokens)
    {
        _terminationCondition = new TokenBudgetTermination(maxTokens);
        return this;
    }

    /// <summary>
    /// 커스텀 종료 조건을 설정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder WithTerminationCondition(ITerminationCondition condition)
    {
        _terminationCondition = condition ?? throw new ArgumentNullException(nameof(condition));
        return this;
    }

    /// <summary>
    /// 최대 라운드 수 (안전 한도)를 설정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder SetMaxRounds(int maxRounds)
    {
        _maxRounds = maxRounds;
        return this;
    }

    /// <summary>
    /// 오케스트레이터 이름을 설정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder SetName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// 전체 타임아웃을 설정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder SetTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    /// <summary>
    /// 개별 에이전트 타임아웃을 설정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder SetAgentTimeout(TimeSpan agentTimeout)
    {
        _agentTimeout = agentTimeout;
        return this;
    }

    /// <summary>
    /// 체크포인트 저장소를 설정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder SetCheckpointStore(ICheckpointStore store)
    {
        _checkpointStore = store;
        return this;
    }

    /// <summary>
    /// 오케스트레이션 ID를 설정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder SetOrchestrationId(string orchestrationId)
    {
        _orchestrationId = orchestrationId;
        return this;
    }

    /// <summary>
    /// 에이전트 실행 전 승인 핸들러를 설정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder SetApprovalHandler(
        Func<string, AgentStepResult?, Task<bool>> handler)
    {
        _approvalHandler = handler;
        return this;
    }

    /// <summary>
    /// 승인이 필요한 에이전트를 지정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder SetRequireApprovalForAgents(params string[] agents)
    {
        _requireApprovalForAgents = [.. agents];
        return this;
    }

    /// <summary>
    /// GroupChat 오케스트레이터를 빌드합니다.
    /// </summary>
    public GroupChatOrchestrator Build()
    {
        if (_agentMap.Count == 0)
            throw new InvalidOperationException("At least one agent must be registered.");

        if (_speakerSelector == null)
            throw new InvalidOperationException("Speaker selector must be specified.");

        if (_terminationCondition == null)
            throw new InvalidOperationException("Termination condition must be specified.");

        var options = new GroupChatOrchestratorOptions
        {
            Name = _name,
            Timeout = _timeout,
            AgentTimeout = _agentTimeout,
            SpeakerSelector = _speakerSelector,
            TerminationCondition = _terminationCondition,
            MaxRounds = _maxRounds,
            CheckpointStore = _checkpointStore,
            OrchestrationId = _orchestrationId,
            ApprovalHandler = _approvalHandler,
            RequireApprovalForAgents = _requireApprovalForAgents,
        };

        return new GroupChatOrchestrator(options, new Dictionary<string, IAgent>(_agentMap));
    }
}
