using System.Reflection;
using Raggle.Abstractions.Converters;

namespace Raggle.Abstractions.Models;

public class FunctionTool
{
    public required MethodInfo Method { get; set; }

    public JsonSchema ToJson()
    {
        return FunctionJsonConverter.Convert(Method);
    }
}
