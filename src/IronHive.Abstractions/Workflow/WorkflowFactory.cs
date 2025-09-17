using System.Text.Json;
using YamlDotNet.Serialization;

namespace IronHive.Abstractions.Workflow;

/// <summary>
/// 워크플로우 관련 오브젝트를 생성하는 팩토리.
/// <para>
/// - DI(<see cref="IServiceProvider"/>)로 스텝(<see cref="IWorkflowStep"/>)들을 키값에 따라 Resolve합니다.<br/>
/// - 필요 시 YAML/JSON 정의로부터 역직렬화하여 생성하는 진입점도 제공합니다.
/// </para>
/// </summary>
public class WorkflowFactory
{
    private readonly IServiceProvider? _services;

    public WorkflowFactory(IServiceProvider? services = null)
    {
        _services = services;
    }

    /// <summary>
    /// 현재 DI 컨텍스트를 바인딩한 <see cref="WorkflowBuilder"/>를 생성합니다.
    /// </summary>
    public WorkflowBuilder CreateBuilder()
    {
        return new WorkflowBuilder(_services);
    }

    /// <summary>
    /// 주어진 워크플로우 정의로부터 실행 가능한 워크플로우 엔진을 생성합니다.
    /// </summary>
    /// <typeparam name="TContext">워크플로우 실행 시 공유되는 컨텍스트 타입.</typeparam>
    /// <param name="definition">노드/스텝 구성이 포함된 워크플로우 정의.</param>
    /// <returns><see cref="IWorkflow{TContext}"/> 구현체.</returns>
    public IWorkflow<TContext> CreateFrom<TContext>(WorkflowDefinition definition)
    {
        // 1) 정의에 등장하는 모든 노드를 깊이 순회하며 '필요한 스텝 이름'만 수집합니다.
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var n in WorkflowDefinition.EnumerateRecursively(definition.Steps))
        {
            if (n is TaskNode t) names.Add(t.Step);        // Task 노드가 참조하는 스텝 이름
            if (n is ConditionNode c) names.Add(c.Step);   // Condition 노드가 참조하는 스텝 이름
        }

        // 2) 수집된 스텝 이름을 기준으로 DI에서 실제 스텝 인스턴스 해결
        var steps = new Dictionary<string, IWorkflowStep>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in names)
        {
            var step = _services?.GetKeyedServiceOrCreate<IWorkflowStep>(name)
                       ?? throw new InvalidOperationException($"'{name}' 스텝이 DI에 등록되지 않았습니다.");
            steps[name] = step;
        }

        // 3) 엔진 인스턴스를 구성하여 반환
        return new WorkflowEngine<TContext>(steps)
        {
            Name = definition.Name,         // 워크플로우 이름(옵션)
            Version = definition.Version,   // 워크플로우 버전(옵션)
            Nodes = definition.Steps        // 루트 노드 집합
        };
    }

    /// <summary>
    /// YAML 문자열로부터 <see cref="WorkflowDefinition"/>을 역직렬화하여 워크플로우를 생성합니다.
    /// </summary>
    /// <typeparam name="TContext">워크플로우 실행 컨텍스트 타입.</typeparam>
    /// <param name="yaml">YAML 형식의 워크플로우 정의 문자열.</param>
    /// <returns><see cref="IWorkflow{TContext}"/> 구현체.</returns>
    public IWorkflow<TContext> CreateFromYaml<TContext>(string yaml)
    {
        var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties() // 정의에 없는 속성 무시
            .WithTypeDiscriminatingNodeDeserializer(options =>
            {
                options.AddKeyValueTypeDiscriminator<WorkflowNode>("type", new Dictionary<string, Type>(StringComparer.Ordinal)
                {
                    { "task", typeof(TaskNode) },
                    { "condition", typeof(ConditionNode) },
                    { "parallel", typeof(ParallelNode) }
                });
            })
            .Build();
        var def = deserializer.Deserialize<WorkflowDefinition>(yaml)
                  ?? throw new InvalidOperationException("YAML 역직렬화 결과가 null입니다.");
        return CreateFrom<TContext>(def);
    }

    /// <summary>
    /// JSON 문자열로부터 <see cref="WorkflowDefinition"/>을 역직렬화하여 워크플로우를 생성합니다.
    /// </summary>
    /// <typeparam name="TContext">워크플로우 실행 컨텍스트 타입.</typeparam>
    /// <param name="json">JSON 형식의 워크플로우 정의 문자열.</param>
    /// <returns><see cref="IWorkflow{TContext}"/> 구현체.</returns>
    public IWorkflow<TContext> CreateFromJson<TContext>(string json)
    {
        var def = JsonSerializer.Deserialize<WorkflowDefinition>(json)
                  ?? throw new InvalidOperationException("JSON 역직렬화 결과가 null입니다.");
        return CreateFrom<TContext>(def);
    }
}
