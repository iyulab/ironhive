namespace Raggle.Abstractions.Workflow;

public interface IWorkflow
{
    string Id { get; set; }

    string Name { get; set; }

    Task RunAsync(
        CancellationToken cancellationToken = default);
}
