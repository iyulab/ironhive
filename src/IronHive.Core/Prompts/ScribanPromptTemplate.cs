using IronHive.Abstractions.Prompts;
using Scriban;

namespace IronHive.Core.Prompts;

/// <inheritdoc />
public class ScribanPromptTemplate : IPromptTemplate
{
    /// <inheritdoc />
    public string Render(string template, object? context)
    {
        return Template.Parse(template).Render(context);
    }
}
