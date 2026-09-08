using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;

namespace IronHive.Tests.Tools;

public class ToolOutputTests
{
    [Fact]
    public void Success_WithText_ShouldSetIsSuccessTrueAndTextContent()
    {
        var output = ToolOutput.Success("result data");

        output.IsSuccess.Should().BeTrue();
        output.Content.Should().ContainSingle()
            .Which.Should().BeOfType<TextMessageContent>()
            .Which.Value.Should().Be("result data");
    }

    [Fact]
    public void Success_WithNullText_ShouldHaveEmptyContent()
    {
        var output = ToolOutput.Success((string?)null);

        output.IsSuccess.Should().BeTrue();
        output.Content.Should().BeEmpty();
    }

    [Fact]
    public void Success_WithContentCollection_ShouldSetContentAsIs()
    {
        MessageContent[] content = [new TextMessageContent { Value = "a" }, new TextMessageContent { Value = "b" }];

        var output = ToolOutput.Success(content);

        output.IsSuccess.Should().BeTrue();
        output.Content.Should().BeEquivalentTo(content);
    }

    [Fact]
    public void Failure_WithText_ShouldSetIsSuccessFalseAndTextContent()
    {
        var output = ToolOutput.Failure("error message");

        output.IsSuccess.Should().BeFalse();
        output.Content.Should().ContainSingle()
            .Which.Should().BeOfType<TextMessageContent>()
            .Which.Value.Should().Be("error message");
    }

    [Fact]
    public void Failure_WithNull_ShouldHaveEmptyContent()
    {
        var output = ToolOutput.Failure(null);

        output.IsSuccess.Should().BeFalse();
        output.Content.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithParameters_ShouldSetProperties()
    {
        MessageContent[] content = [new TextMessageContent { Value = "test" }];

        var output = new ToolOutput(true, content);

        output.IsSuccess.Should().BeTrue();
        output.Content.Should().BeEquivalentTo(content);
    }

    [Fact]
    public void DefaultConstructor_ShouldHaveDefaultValues()
    {
        var output = new ToolOutput();

        output.IsSuccess.Should().BeFalse();
        output.Content.Should().BeEmpty();
    }
}
