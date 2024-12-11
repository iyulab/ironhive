namespace Raggle.Abstractions.AI;

public interface IPromptProvider
{
    Task LoadTemplateAsync(
        string template,
        CancellationToken cancellationToken = default);

    Task<string> GetPromptAsync(
        IDictionary<string, object> parameters,
        CancellationToken cancellationToken = default);
}
