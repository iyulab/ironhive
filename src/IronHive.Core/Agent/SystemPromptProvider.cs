using IronHive.Abstractions.Agent;
using Scriban;

namespace IronHive.Core.Agent;

/// <inheritdoc />
public class SystemPromptProvider : IPromptProvider
{
    /// <inheritdoc />
    public string Render(string expression, object? model)
    {
        var template = Template.Parse(expression);
        var result = template.Render(model, member => member.Name);
        return result;
    }

    /// <inheritdoc />
    public string RenderFromFile(string filePath, object? model)
    {
        var expression = File.ReadAllText(filePath);
        return Render(expression, model);
    }
}
