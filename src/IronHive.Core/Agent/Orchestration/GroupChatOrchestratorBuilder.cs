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
    private bool _stopOnAgentFailure = true;
    private IList<IAgentMiddleware>? _agentMiddlewares;
    private IContextScope? _contextScope;
    private IResultDistiller? _resultDistiller;
    private ResultDistillationOptions? _resultDistillationOptions;

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
    /// 에이전트가 실패하면 오케스트레이션을 멈출지 설정합니다(기본 <see langword="true"/>).
    /// </summary>
    public GroupChatOrchestratorBuilder SetStopOnAgentFailure(bool stopOnAgentFailure)
    {
        _stopOnAgentFailure = stopOnAgentFailure;
        return this;
    }

    /// <summary>
    /// 각 에이전트 실행을 감싸는 미들웨어를 설정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder SetAgentMiddlewares(IList<IAgentMiddleware>? middlewares)
    {
        _agentMiddlewares = middlewares;
        return this;
    }

    /// <summary>
    /// 에이전트에 넘길 메시지 범위를 정하는 스코프를 설정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder SetContextScope(IContextScope? scope)
    {
        _contextScope = scope;
        return this;
    }

    /// <summary>
    /// 에이전트 결과를 다음 단계로 넘기기 전에 줄이는 distiller 를 설정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder SetResultDistiller(IResultDistiller? distiller)
    {
        _resultDistiller = distiller;
        return this;
    }

    /// <summary>
    /// <see cref="SetResultDistiller"/> 에 넘길 옵션을 설정합니다.
    /// </summary>
    public GroupChatOrchestratorBuilder SetResultDistillationOptions(ResultDistillationOptions? options)
    {
        _resultDistillationOptions = options;
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
            StopOnAgentFailure = _stopOnAgentFailure,
            AgentMiddlewares = _agentMiddlewares,
            ContextScope = _contextScope,
            ResultDistiller = _resultDistiller,
            ResultDistillationOptions = _resultDistillationOptions,
        };

        return new GroupChatOrchestrator(options, new Dictionary<string, IAgent>(_agentMap));
    }
}
