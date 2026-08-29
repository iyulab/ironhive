using AwesomeAssertions;
using IronHive.Abstractions.Tools;
using IronHive.Core.Microsoft;
using Microsoft.Extensions.AI;

namespace IronHive.Tests.Microsoft;

public class AIToolAdapterTests
{
    [Fact]
    public void Constructor_NullAiTool_ThrowsArgumentNullException()
    {
        var act = () => new AIToolAdapter(null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("aiTool");
    }

    [Fact]
    public void Properties_ProjectFromWrappedAIFunction()
    {
        var function = AIFunctionFactory.Create(
            (string name) => $"hello {name}",
            name: "greet",
            description: "Greets a name");
        var tool = new AIToolAdapter(function);

        tool.UniqueName.Should().Be("greet");
        tool.Description.Should().Be("Greets a name");
        tool.Parameters.Should().NotBeNull();
        tool.RequiresApproval.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_ExecutableAIFunction_ReturnsSuccessWithResult()
    {
        var function = AIFunctionFactory.Create(
            (string name) => $"hello {name}",
            name: "greet");
        var tool = new AIToolAdapter(function);

        var output = await tool.InvokeAsync(new ToolInput(new Dictionary<string, object?>
        {
            ["name"] = "world"
        }), TestContext.Current.CancellationToken);

        output.IsSuccess.Should().BeTrue();
        output.Result.Should().Contain("hello world");
    }

    [Fact]
    public async Task InvokeAsync_FunctionThrows_ReturnsFailureWithMessage()
    {
        var function = AIFunctionFactory.Create(
            () => { throw new InvalidOperationException("boom"); },
            name: "explode");
        var tool = new AIToolAdapter(function);

        var output = await tool.InvokeAsync(new ToolInput(), TestContext.Current.CancellationToken);

        output.IsSuccess.Should().BeFalse();
        output.Result.Should().Be("boom");
    }

    [Fact]
    public async Task InvokeAsync_DeclarationOnlyAITool_ReturnsFailure()
    {
        var function = AIFunctionFactory.Create((string name) => $"hello {name}", name: "greet");
        var declarationOnly = function.AsDeclarationOnly();
        var tool = new AIToolAdapter(declarationOnly);

        var output = await tool.InvokeAsync(new ToolInput(), TestContext.Current.CancellationToken);

        output.IsSuccess.Should().BeFalse();
        output.Result.Should().Contain("greet");
    }
}
