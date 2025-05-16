using IronHive.Abstractions.Prompts;
using Scriban;

namespace IronHive.Core.Prompts;

/// <inheritdoc />
public class ScribanPromptProvider : IPromptProvider
{
    /// <inheritdoc />
    public string Render(string template, object? context)
    {
        return Template.Parse(template).Render(context);
    }
}
