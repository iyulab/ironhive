# 워크플로우 시스템

코드로 정의하는 타입 안전 워크플로우 엔진입니다.

## 개요

`IWorkflow<TContext>`는 컨텍스트 객체를 공유하며 단계별로 실행되는 워크플로우를 정의합니다. 노드(step) 기반의 DAG 구조를 지원합니다.

```csharp
public interface IWorkflow<TContext>
{
    string? Name { get; }
    Version? Version { get; }
    event EventHandler<WorkflowEventArgs<TContext>>? Progressed;

    Task RunAsync(TContext context, CancellationToken ct = default);
    Task RunFromAsync(string nodeId, TContext context, CancellationToken ct = default);
}
```

---

## 워크플로우 정의

`WorkflowFactory` + Fluent Builder API로 워크플로우를 구성합니다:

```csharp
var factory = new WorkflowFactory(serviceProvider);

var workflow = factory.CreateBuilder()
    .WithName("document-processing")
    .WithVersion(new Version(1, 0))
    .StartWith<MyContext>()
        .Then<ValidateStep>("validate")
        .Then<ProcessStep, ProcessOptions>("process", new ProcessOptions { BatchSize = 10 })
        .Then<NotifyStep>("notify")
    .Build();
```

---

## 워크플로우 스텝 구현

### IWorkflowTask (기본 스텝)

```csharp
public class ValidateStep : IWorkflowTask<MyContext>
{
    public Task<TaskStepResult> ExecuteAsync(
        MyContext context,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(context.DocumentPath))
            return Task.FromResult(TaskStepResult.Fail(new ArgumentException("DocumentPath is required")));

        context.IsValid = true;
        return Task.FromResult(TaskStepResult.Success());
    }
}
```

### IWorkflowTask<TContext, TOptions> (옵션 포함)

```csharp
public class ProcessOptions
{
    public int BatchSize { get; set; } = 10;
}

public class ProcessStep : IWorkflowTask<MyContext, ProcessOptions>
{
    public Task<TaskStepResult> ExecuteAsync(
        MyContext context,
        ProcessOptions options,
        CancellationToken ct = default)
    {
        // options.BatchSize 사용
        context.ProcessedCount = options.BatchSize;
        return Task.FromResult(TaskStepResult.Success());
    }
}
```

---

## 워크플로우 실행

```csharp
var context = new MyContext { DocumentPath = "./doc.pdf" };

// 처음부터 실행
await workflow.RunAsync(context, cancellationToken);

// 특정 노드부터 재개
await workflow.RunFromAsync("process", context, cancellationToken);

// 진행 이벤트 구독
workflow.Progressed += (_, args) =>
{
    Console.WriteLine($"[{args.NodeId}] {args.Type}");
};
```

---

## MemoryWorker와의 관계

`MemoryWorker`의 RAG 파이프라인은 내부적으로 워크플로우 시스템을 사용합니다:

```csharp
// MemoryPipelineBuilder는 WorkflowStepBuilder<MemoryContext> 래퍼
var worker = hive.CreateMemoryWorkerFrom(builder =>
    builder
        .UseQueue("tasks")                           // IQueueStorage 지정
        .Then<TextExtractionPipeline>("extract")     // IMemoryPipeline 스텝
        .Then<TextChunkingPipeline, TextChunkingPipeline.Options>("chunk", new TextChunkingPipeline.Options())
        .Build());

// IMemoryPipeline은 IWorkflowTask<MemoryContext>와 동등
```

---

## WorkflowFactory

```csharp
var factory = new WorkflowFactory(serviceProvider); // sp는 선택사항

// 빌더 생성
var builder = factory.CreateBuilder();
```

DI와 함께 사용:

```csharp
// Program.cs
builder.Services.AddSingleton<WorkflowFactory>();

// 서비스에서 사용
public class MyService(WorkflowFactory factory)
{
    public async Task RunAsync(MyContext ctx)
    {
        var wf = factory.CreateBuilder()
            .StartWith<MyContext>()
            .Then<Step1>("step1")
            .Then<Step2>("step2")
            .Build();

        await wf.RunAsync(ctx);
    }
}
```

---

## 스텝 결과와 분기

작업 스텝(`IWorkflowTask`)은 `TaskStepResult` 를 돌려준다:

```csharp
// 성공 (메시지는 선택)
return TaskStepResult.Success("processed 10 items");

// 실패 — 워크플로우가 중단된다. 예외를 던져도 같다.
return TaskStepResult.Fail(new InvalidOperationException("오류 메시지"));
```

분기는 조건 스텝(`IWorkflowCondition<TContext>`)이 키를 고르고 `Switch` 가 그 키로 다음 경로를 정한다:

```csharp
public class ReviewCondition : IWorkflowCondition<MyContext>
{
    public Task<ConditionStepResult> EvaluateAsync(MyContext context, CancellationToken ct = default)
        => Task.FromResult(ConditionStepResult.Select(context.IsValid ? "approved" : "rejected"));
}

var workflow = factory.CreateBuilder()
    .StartWith<MyContext>()
        .Then<ValidateStep>("validate")
        .Switch<ReviewCondition>("review", new Dictionary<string, Action<WorkflowStepBuilder<MyContext>>>
        {
            ["approved"] = b => b.Then<ProcessStep, ProcessOptions>("process", new ProcessOptions()),
            ["rejected"] = b => b.Then<NotifyStep>("notify"),
        })
    .Build();
```

정의되지 않은 키는 `Switch` 의 세 번째 인자(기본 경로)로 간다.

---

## Planning 시스템 (실험적)

에이전트 기반 계획-실행 패턴을 위한 인터페이스:

```csharp
// 계획 생성
public interface ITaskPlanner
{
    Task<TaskPlan> CreatePlanAsync(string goal, PlanningContext context, CancellationToken ct = default);
    Task<TaskPlan> ReplanAsync(TaskPlan current, string failureReason, CancellationToken ct = default);
}

// 계획 실행
public interface IPlanExecutor
{
    Task<StepResult> ExecuteStepAsync(PlanStep step, PlanningContext context, CancellationToken ct = default);
}

// 계획 평가
public interface IPlanEvaluator
{
    Task<EvaluationResult> EvaluateAsync(TaskPlan plan, PlanningContext context, CancellationToken ct = default);
}

// TaskPlan 구조
public class TaskPlan
{
    public string Goal { get; set; }
    public List<PlanStep> Steps { get; set; }
    public PlanStatus Status { get; set; }
}
```

---

## 관련 문서

- [MEMORY.md](MEMORY.md) — RAG 파이프라인 (MemoryWorker)
- [ORCHESTRATION.md](ORCHESTRATION.md) — 멀티에이전트 오케스트레이션
