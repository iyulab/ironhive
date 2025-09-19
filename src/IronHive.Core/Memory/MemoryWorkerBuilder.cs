using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Queue;
using IronHive.Abstractions.Registries;
using IronHive.Abstractions.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core.Memory;

/// <summary>
/// 메모리 작업을 수행하는 워커를 구성하는 빌더입니다.
/// </summary>
public class MemoryWorkerBuilder
{
    private readonly IServiceProvider _services;

    public MemoryWorkerBuilder(IServiceProvider services)
    {
        _services = services;
    }

    /// <summary>
    /// 작업자가 사용할 큐 스토리지를 지정합니다.
    /// </summary>
    public MemoryPipelineBuilder UseQueue(string storageName)
    {
        var storages = _services.GetRequiredService<IStorageRegistry>();
        if (!storages.TryGet<IQueueStorage>(storageName, out var queue))
            throw new InvalidOperationException($"큐 스토리지 '{storageName}'(이)가 등록되어 있지 않습니다.");
        var builder = new WorkflowFactory(_services).CreateBuilder().StartWith<MemoryContext>();

        return new MemoryPipelineBuilder(queue, builder);
    }
}

/// <summary>
/// 메모리 파이프라인을 구성하는 빌더입니다.
/// </summary>
public class MemoryPipelineBuilder
{
    private readonly IQueueStorage _queue;
    private readonly WorkflowStepBuilder<MemoryContext> _builder;

    public MemoryPipelineBuilder(IQueueStorage queue, WorkflowStepBuilder<MemoryContext> builder)
    {
        _queue = queue;
        _builder = builder;
    }

    /// <summary> 파이프라인을 추가합니다. </summary>
    public MemoryPipelineBuilder Then<TPipe>(string pipelineName)
        where TPipe : class, IMemoryPipeline
    {
        _builder.Then<TPipe>(pipelineName);
        return this;
    }

    /// <summary> 옵션을 사용하는 파이프라인을 추가합니다. </summary>
    public MemoryPipelineBuilder Then<TPipe, TOptions>(string pipelineName, TOptions options)
        where TPipe : class, IMemoryPipeline<TOptions>
        where TOptions : notnull
    {
        _builder.Then<TPipe, TOptions>(pipelineName, options);
        return this;
    }

    /// <summary> 작업자를 빌드합니다. </summary>
    public IMemoryWorker Build()
    {
        var workflow = _builder.Build();
        return new MemoryWorker(_queue, workflow);
    }
}