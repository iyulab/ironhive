namespace Raggle.Abstractions.Workflow;

public interface IEdge
{
    string Id { get; set; }

    string SourceId { get; set; }

    string TargetId { get; set; }
}
