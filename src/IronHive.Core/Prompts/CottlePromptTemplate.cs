using IronHive.Abstractions.Prompts;
using Cottle;

namespace IronHive.Core.Prompts;

/// <inheritdoc />
public class CottlePromptTemplate : IPromptTemplate
{
    /// <inheritdoc />
    public string Render(string template, object? context)
    {
        var doc = Document.CreateDefault(template).DocumentOrThrow;
        var symbols = context.ConvertTo<IReadOnlyDictionary<Value, Value>>()
            ?? new Dictionary<Value, Value>();
        return doc.Render(Context.CreateBuiltin(symbols));
    }
}
