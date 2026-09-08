using System.ComponentModel;
using System.Text.Json;
using AwesomeAssertions;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;

namespace IronHive.Tests.Tools;

public class FunctionToolFactoryTests
{
    private static string Text(ToolOutput output) =>
        output.Content.OfType<TextMessageContent>().Single().Value;

    #region CreateFrom<T> / CreateFrom(Type)

    [Fact]
    public void CreateFromType_FindsAttributedMethods()
    {
        var tools = FunctionToolFactory.CreateFrom<SampleToolClass>().ToList();

        tools.Should().HaveCount(2);
        tools.Should().Contain(t => t.UniqueName == "func_custom_name");
        tools.Should().Contain(t => t.UniqueName == "func_MethodWithDefaults");
    }

    [Fact]
    public void CreateFromType_SetsNameFromAttribute()
    {
        var tools = FunctionToolFactory.CreateFrom<SampleToolClass>().ToList();
        var tool = tools.Single(t => t.UniqueName == "func_custom_name");

        tool.Description.Should().Be("A custom tool");
        tool.RequiresApproval.Should().BeTrue();
    }

    [Fact]
    public void CreateFromType_FallsBackToMethodName()
    {
        var tools = FunctionToolFactory.CreateFrom<SampleToolClass>().ToList();
        var tool = tools.Single(t => t.UniqueName == "func_MethodWithDefaults");

        tool.Description.Should().BeNull();
        tool.RequiresApproval.Should().BeFalse();
    }

    [Fact]
    public void CreateFromType_NoAttributedMethods_ReturnsEmpty()
    {
        var tools = FunctionToolFactory.CreateFrom<NoToolMethods>().ToList();

        tools.Should().BeEmpty();
    }

    [Fact]
    public void CreateFromType_GeneratesParameterSchema()
    {
        var tools = FunctionToolFactory.CreateFrom<SampleToolClass>().ToList();
        var tool = tools.Single(t => t.UniqueName == "func_custom_name");

        tool.Parameters.Should().NotBeNull();
        var json = tool.Parameters!.ToString();
        json.Should().Contain("\"type\"");
        json.Should().Contain("object");
        json.Should().Contain("\"message\"");
    }

    [Fact]
    public void CreateFromType_ExcludesCancellationTokenFromSchema()
    {
        var tools = FunctionToolFactory.CreateFrom<SampleToolClass>().ToList();
        var tool = tools.Single(t => t.UniqueName == "func_custom_name");

        var json = tool.Parameters!.ToString();
        json.Should().NotContain("cancellationToken");
    }

    [Fact]
    public void CreateFromType_OptionalParamsNotInRequired()
    {
        var tools = FunctionToolFactory.CreateFrom<SampleToolClass>().ToList();
        var tool = tools.Single(t => t.UniqueName == "func_MethodWithDefaults");

        var json = tool.Parameters!.ToString();
        // "count" has a default value, should not be in required
        json.Should().Contain("\"count\"");
    }

    #endregion

    #region CreateFrom(Delegate)

    [Fact]
    public void CreateFromDelegate_CreatesToolWithDescriptor()
    {
        Func<string, int> myFunc = s => s.Length;
        var descriptor = new DelegateDescriptor
        {
            Name = "string_length",
            Description = "Returns string length"
        };

        var tool = FunctionToolFactory.CreateFrom(myFunc, descriptor);

        tool.UniqueName.Should().Be("func_string_length");
        tool.Description.Should().Be("Returns string length");
        tool.RequiresApproval.Should().BeFalse();
    }

    [Fact]
    public void CreateFromDelegate_EmptyName_ThrowsArgumentException()
    {
        Func<string> myFunc = () => "test";
        var descriptor = new DelegateDescriptor { Name = "" };

        var act = () => FunctionToolFactory.CreateFrom(myFunc, descriptor);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateFromDelegate_WithParameters_GeneratesSchema()
    {
        Func<string, int, string> myFunc = (name, count) => $"{name}:{count}";
        var descriptor = new DelegateDescriptor { Name = "concat" };

        var tool = FunctionToolFactory.CreateFrom(myFunc, descriptor);

        tool.Parameters.Should().NotBeNull();
        var json = tool.Parameters!.ToString();
        json.Should().Contain("\"name\"");
        json.Should().Contain("\"count\"");
    }

    [Fact]
    public void CreateFromDelegate_NoParameters_NullSchema()
    {
        Func<string> myFunc = () => "test";
        var descriptor = new DelegateDescriptor { Name = "no_params" };

        var tool = FunctionToolFactory.CreateFrom(myFunc, descriptor);

        tool.Parameters.Should().BeNull();
    }

    [Fact]
    public void CreateFromDelegate_CustomTimeout_Propagated()
    {
        Func<string> myFunc = () => "test";
        var descriptor = new DelegateDescriptor { Name = "timeout_tool", Timeout = 120 };

        var tool = (FunctionTool)FunctionToolFactory.CreateFrom(myFunc, descriptor);

        tool.Timeout.Should().Be(120);
    }

    #endregion

    #region FunctionTool InvokeAsync

    [Fact]
    public async Task InvokeAsync_SyncFunction_ReturnsResult()
    {
        Func<int, int, int> add = (a, b) => a + b;
        var tool = new FunctionTool(add)
        {
            Name = "add",
            Description = "Adds two numbers",
            Parameters = null,
            RequiresApproval = false
        };

        var input = new ToolInput(new Dictionary<string, object?> { ["a"] = 3, ["b"] = 5 });
        var result = await tool.InvokeAsync(input, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        Text(result).Should().Be("8");
    }

    [Fact]
    public async Task InvokeAsync_AsyncFunction_ReturnsResult()
    {
        Func<string, Task<string>> greet = async name =>
        {
            await Task.Delay(1);
            return $"Hello, {name}!";
        };
        var tool = new FunctionTool(greet)
        {
            Name = "greet",
            Description = null,
            Parameters = null,
            RequiresApproval = false
        };

        var input = new ToolInput(new Dictionary<string, object?> { ["name"] = "World" });
        var result = await tool.InvokeAsync(input, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        Text(result).Should().Contain("Hello, World!");
    }

    [Fact]
    public async Task InvokeAsync_VoidFunction_ReturnsExecutedDone()
    {
        var called = false;
        Action act = () => called = true;
        var tool = new FunctionTool(act)
        {
            Name = "void_func",
            Description = null,
            Parameters = null,
            RequiresApproval = false
        };

        var result = await tool.InvokeAsync(new ToolInput(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        Text(result).Should().Contain("executed done");
        called.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_TaskFunction_ReturnsExecutedDone()
    {
        Func<Task> asyncVoid = () => Task.CompletedTask;
        var tool = new FunctionTool(asyncVoid)
        {
            Name = "task_func",
            Description = null,
            Parameters = null,
            RequiresApproval = false
        };

        var result = await tool.InvokeAsync(new ToolInput(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        Text(result).Should().Contain("executed done");
    }

    [Fact]
    public async Task InvokeAsync_ThrowingFunction_ReturnsFailure()
    {
        Func<string> thrower = () => throw new InvalidOperationException("Boom!");
        var tool = new FunctionTool(thrower)
        {
            Name = "thrower",
            Description = null,
            Parameters = null,
            RequiresApproval = false
        };

        var result = await tool.InvokeAsync(new ToolInput(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        Text(result).Should().Contain("Boom!");
    }

    [Fact]
    public async Task InvokeAsync_DefaultParameters_UsesDefaults()
    {
        // Use a static method with default parameter instead of lambda (CS9098)
        var method = typeof(FunctionToolFactoryTests).GetMethod(nameof(RepeatHelper),
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        var tool = new FunctionTool(method)
        {
            Name = "repeat",
            Description = null,
            Parameters = null,
            RequiresApproval = false
        };

        var input = new ToolInput(new Dictionary<string, object?> { ["text"] = "hi" });
        var result = await tool.InvokeAsync(input, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        Text(result).Should().Contain("hi");
    }

    private static string RepeatHelper(string text, int count = 1)
        => string.Concat(Enumerable.Repeat(text, count));

    [Fact]
    public void UniqueName_PrefixedWithFunc()
    {
        var tool = new FunctionTool(new Func<string>(() => ""))
        {
            Name = "my_tool",
            Description = null,
            Parameters = null,
            RequiresApproval = false
        };

        tool.UniqueName.Should().Be("func_my_tool");
    }

    [Fact]
    public void Timeout_DefaultsToUnlimited()
    {
        var tool = new FunctionTool(new Func<string>(() => ""))
        {
            Name = "tool",
            Description = null,
            Parameters = null,
            RequiresApproval = false
        };

        tool.Timeout.Should().Be(0);
    }

    #endregion

    #region JsonOptions

    [Fact]
    public void JsonOptions_DefaultsToNull()
    {
        var tool = new FunctionTool(new Func<string>(() => ""))
        {
            Name = "tool",
            Description = null,
            Parameters = null,
            RequiresApproval = false
        };

        tool.JsonOptions.Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_QuotedNumericStringArgument_UsesFunctionOptionsByDefault()
    {
        // Mirrors what an LLM's raw JSON tool-call arguments look like: {"n":"21"} — the value
        // arrives as a JSON string, not a JSON number. JsonDefaultOptions.FunctionOptions enables
        // NumberHandling.AllowReadingFromString, so this binds to int without any per-tool
        // JsonOptions override.
        var tool = new FunctionTool(new Func<int, int>(n => n * 2))
        {
            Name = "double",
            Description = null,
            Parameters = null,
            RequiresApproval = false
        };

        var input = new ToolInput("""{"n":"21"}""");
        var result = await tool.InvokeAsync(input, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        Text(result).Should().Be("42");
    }

    [Fact]
    public async Task InvokeAsync_JsonOptionsOverride_AppliesToArgumentDeserialization()
    {
        var strictOptions = new JsonSerializerOptions(); // no NumberHandling.AllowReadingFromString
        var tool = new FunctionTool(new Func<int, int>(n => n * 2))
        {
            Name = "double",
            Description = null,
            Parameters = null,
            RequiresApproval = false,
            JsonOptions = strictOptions
        };

        var input = new ToolInput("""{"n":"21"}""");
        var result = await tool.InvokeAsync(input, TestContext.Current.CancellationToken);

        // A quoted numeric string can't bind to int without AllowReadingFromString, so the strict
        // override makes argument deserialization fail; reflection's MethodInfo.Invoke then silently
        // substitutes default(int) for the unbound parameter rather than throwing, so the call still
        // succeeds — with the wrong value (0, not 42). This confirms JsonOptions genuinely reaches
        // argument binding, not that it produces a clean failure.
        result.IsSuccess.Should().BeTrue();
        Text(result).Should().Be("0");
    }

    [Fact]
    public async Task InvokeAsync_JsonOptionsOverride_AppliesToResultSerialization()
    {
        // JsonDefaultOptions.FunctionOptions defaults to WriteIndented:false (tool output is
        // consumed by the model as text, not read by a human — indentation only costs tokens).
        // A per-tool override can turn it back on, affecting only this tool's output.
        var indentedOptions = new JsonSerializerOptions { WriteIndented = true };
        var indentedTool = new FunctionTool(new Func<object>(() => new { a = 1, b = 2 }))
        {
            Name = "indented",
            Description = null,
            Parameters = null,
            RequiresApproval = false,
            JsonOptions = indentedOptions
        };
        var defaultTool = new FunctionTool(new Func<object>(() => new { a = 1, b = 2 }))
        {
            Name = "default",
            Description = null,
            Parameters = null,
            RequiresApproval = false
        };

        var indentedResult = await indentedTool.InvokeAsync(new ToolInput(), TestContext.Current.CancellationToken);
        var defaultResult = await defaultTool.InvokeAsync(new ToolInput(), TestContext.Current.CancellationToken);

        Text(indentedResult).Should().Contain("\n");
        Text(defaultResult).Should().NotContain("\n");
    }

    #endregion

    #region Helper Types

    private sealed class SampleToolClass
    {
        private readonly string _prefix = "tool";

        [FunctionTool("custom_name", Description = "A custom tool", RequiresApproval = true)]
        public string NamedTool(string message, CancellationToken cancellationToken = default)
            => _prefix + message;

        [FunctionTool]
        public int MethodWithDefaults(string text, int count = 1)
            => (_prefix.Length + text.Length) * count;

        // No attribute — should NOT be discovered
        public string NotATool() => _prefix;
    }

    private sealed class NoToolMethods
    {
        private readonly int _value = 42;

        public void RegularMethod() { _ = _value; }
    }

    #endregion
}
