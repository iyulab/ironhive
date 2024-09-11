using System.Reflection;

namespace Raggle.Abstractions.Tools;

public class FunctionTool
{
    private readonly Delegate _function;

    public string? Name { get; set; }
    public string? Description { get; set; }
    public TypeSchema? Parameters { get; set; }

    public FunctionTool(Delegate function)
    {
        _function = function;
    }

    public async Task<object?> InvokeAsync(params object[] parameters)
    {
        var result = _function.DynamicInvoke(parameters);
        if (result is Task task)
        {
            await task;
            var property = task.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
            return property?.GetValue(task);
        }
        else if (result is ValueTask valueTask)
        {
            await valueTask;
            var property = valueTask.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
            return property?.GetValue(valueTask);
        }
        else if (result is IAsyncEnumerable<object?> asyncEnumerable)
        {
            var items = new List<object?>();
            await foreach (var item in asyncEnumerable)
            {
                items.Add(item);
            }
            return items;
        }
        else
        {
            return result;
        }
    }
}
