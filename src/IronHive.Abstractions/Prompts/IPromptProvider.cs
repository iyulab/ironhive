namespace IronHive.Abstractions.Prompts;

/// <summary>
/// Interface for prompt template rendering.
/// </summary>
public interface IPromptProvider
{
    /// <summary>
    /// Renders a template with the given context.
    /// </summary>
    /// <param name="template">the template to render</param>
    /// <param name="context">model to use for rendering</param>
    /// <returns>rendered text</returns>
    string Render(string template, object? context);
}
