using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Abstractions.Workflow;

public class WorkflowFactory
{
    private readonly IServiceProvider? _services;

    public WorkflowFactory(IServiceProvider? services = null)
    {
        _services = services;
    }

    public WorkflowBuilder<TContext> CreateBuilder<TContext>()
    {
        return new WorkflowBuilder<TContext>(_services);
    }

    public IWorkflow<TContext> CreateFrom<TContext>(WorkflowDefinition definition)
    {
        // Definition에 등장하는 모든 Step 이름 수집 → DI로 리졸브
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var n in WorkflowDefinition.EnumerateRecursively(definition.Steps))
        {
            if (n is TaskNode t) names.Add(t.Step);
            if (n is ConditionNode c) names.Add(c.Step);
        }

        var steps = new Dictionary<string, IWorkflowStep>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in names)
        {
            var step = _services?.GetKeyedServiceOrCreate<IWorkflowStep>(name)
                       ?? throw new InvalidOperationException($"'{name}' 스텝이 DI에 등록되지 않았습니다.");
            steps[name] = step;
        }

        return new WorkflowEngine<TContext>(steps)
        {
            Id = definition.Id,
            Version = definition.Version,
            Nodes = definition.Steps
        };
    }

    public IWorkflow<TContext> CreateFromYaml<TContext>(string yaml)
    {
        // YamlDotNet 등으로 yaml → WorkflowDefinition 역직렬화
        // var def = LoadYaml(yaml);
        // return CreateFrom<TContext>(def);
        throw new NotImplementedException("Yaml 역직렬화를 연결해 주세요.");
    }

    public IWorkflow<TContext> CreateFromJson<TContext>(string json)
    {
        // JsonSerializer 등으로 json → WorkflowDefinition 역직렬화
        // var def = LoadJson(json);
        // return CreateFrom<TContext>(def);
        throw new NotImplementedException("Json 역직렬화를 연결해 주세요.");
    }
}