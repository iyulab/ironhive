using System.ComponentModel;
using FluentAssertions;
using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;

namespace IronHive.Tests.Tools;

public class FunctionToolFactoryTests
{
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
        var result = await tool.InvokeAsync(input);

        result.IsSuccess.Should().BeTrue();
        result.Result.Should().Be("8");
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
        var result = await tool.InvokeAsync(input);

        result.IsSuccess.Should().BeTrue();
        result.Result.Should().Contain("Hello, World!");
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

        var result = await tool.InvokeAsync(new ToolInput());

        result.IsSuccess.Should().BeTrue();
        result.Result.Should().Contain("executed done");
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

        var result = await tool.InvokeAsync(new ToolInput());

        result.IsSuccess.Should().BeTrue();
        result.Result.Should().Contain("executed done");
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

        var result = await tool.InvokeAsync(new ToolInput());

        result.IsSuccess.Should().BeFalse();
        result.Result.Should().Contain("Boom!");
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
        var result = await tool.InvokeAsync(input);

        result.IsSuccess.Should().BeTrue();
        result.Result.Should().Contain("hi");
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
    public void Timeout_DefaultsTo60Seconds()
    {
        var tool = new FunctionTool(new Func<string>(() => ""))
        {
            Name = "tool",
            Description = null,
            Parameters = null,
            RequiresApproval = false
        };

        tool.Timeout.Should().Be(60);
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
