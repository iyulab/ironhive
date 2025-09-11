using IronHive.Abstractions.Workflow;

namespace IronHive.Abstractions.Memory;

public interface IMemoryPipeline<TOptions> : IWorkflowTask<MemoryContext, TOptions>
{ }

public interface IMemoryPipeline : IWorkflowTask<MemoryContext>
{ }
