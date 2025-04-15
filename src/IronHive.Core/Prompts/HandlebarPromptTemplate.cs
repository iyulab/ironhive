using IronHive.Abstractions.Prompts;
using HandlebarsDotNet;

namespace IronHive.Core.Prompts;

/// <inheritdoc />
public class HandlebarPromptTemplate : IPromptTemplate
{
    /// <inheritdoc />
    public string Render(string template, object? context)
    {
        var binder = Handlebars.Compile(template);
        return binder(context);
    }
}
