namespace IronHive.Abstractions.Prompts;

/// <summary>
/// Interface for prompt template rendering.
/// </summary>
public interface IPromptProvider
{
    /// <summary>
    /// Renders a template with the given context.
    /// </summary>
    /// <param name="expression">the template to render</param>
    /// <param name="model">model to use for rendering</param>
    /// <returns>rendered text</returns>
    string Render(string expression, object? model);

    /// <summary>
    /// Renders a template from a file with the given context.
    /// </summary>
    /// <param name="filePath">the path to the template file</param>
    /// <param name="model">model to use for rendering</param>
    /// <returns>rendered text</returns>
    string RenderFromFile(string filePath, object? model);
}
