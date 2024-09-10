using System.Reflection;

namespace Raggle.Abstractions.Tools;

public class FunctionTool
{
    public required MethodInfo Method { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public ParametersSchema? Parameters { get; set; }

    public async Task<object?> InvokeAsync(params object[] parameters)
    {
        try
        {
            var result = Method.Invoke(null, parameters);
            if (result is Task task)
            {
                await task.ConfigureAwait(false);
                var resultProperty = task.GetType().GetProperty("Result");
                return resultProperty?.GetValue(task);
            }
            return result;
        }
        catch (TargetInvocationException ex)
        {
            throw new Exception("Method invocation failed.", ex.InnerException);
        }
        catch (Exception ex)
        {
            throw new Exception("Error during method execution.", ex);
        }
    }
}
