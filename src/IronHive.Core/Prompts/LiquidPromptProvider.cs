using IronHive.Abstractions.Prompts;
using DotLiquid;
using System.Text.Encodings.Web;

namespace IronHive.Core.Prompts;

/// <inheritdoc />
public class LiquidPromptProvider : IPromptProvider
{
    //private readonly FluidParser _parser = new FluidParser();

    /// <inheritdoc />
    public string Render(string template, object? context)
    {
        return Template.Parse(template).Render(Hash.FromAnonymousObject(context));
        //return _parser.Parse(template).Render(new TemplateContext(context, new TemplateOptions
        //{
        //    ModelNamesComparer = StringComparer.InvariantCultureIgnoreCase,
        //    JavaScriptEncoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        //}));
    }
}
