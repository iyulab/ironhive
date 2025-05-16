using IronHive.Abstractions.Prompts;
using HandlebarsDotNet;

namespace IronHive.Core.Prompts;

/// <inheritdoc />
public class HandlebarPromptProvider : IPromptProvider
{
    /// <inheritdoc />
    public string Render(string template, object? context)
    {
        var binder = Handlebars.Compile(template);
        return binder(context);
    }
}
