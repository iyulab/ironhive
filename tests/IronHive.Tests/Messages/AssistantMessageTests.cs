using FluentAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;

namespace IronHive.Tests.Messages;

public class AssistantMessageTests
{
    [Fact]
    public void GroupContentByToolBoundary_EmptyContent_ShouldReturnEmpty()
    {
        var message = new AssistantMessage { Content = [] };

        var result = message.GroupContentByToolBoundary().ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void GroupContentByToolBoundary_AllText_ShouldReturnSingleGroup()
    {
        var message = new AssistantMessage
        {
            Content =
            [
                new TextMessageContent { Value = "hello" },
                new TextMessageContent { Value = "world" }
            ]
        };

        var result = message.GroupContentByToolBoundary().ToList();

        result.Should().HaveCount(1);
        result[0].Should().HaveCount(2);
    }

    [Fact]
    public void GroupContentByToolBoundary_AllTools_ShouldReturnSingleGroup()
    {
        var message = new AssistantMessage
        {
            Content =
            [
                new ToolMessageContent { Id = "1", Name = "tool1", IsApproved = true },
                new ToolMessageContent { Id = "2", Name = "tool2", IsApproved = true }
            ]
        };

        var result = message.GroupContentByToolBoundary().ToList();

        result.Should().HaveCount(1);
        result[0].Should().HaveCount(2);
    }

    [Fact]
    public void GroupContentByToolBoundary_ToolThenText_ShouldSplitIntoTwoGroups()
    {
        var message = new AssistantMessage
        {
            Content =
            [
                new ToolMessageContent { Id = "1", Name = "tool1", IsApproved = true },
                new TextMessageContent { Value = "result" }
            ]
        };

        var result = message.GroupContentByToolBoundary().ToList();

        result.Should().HaveCount(2);
        result[0].Should().HaveCount(1);
        result[0].First().Should().BeOfType<ToolMessageContent>();
        result[1].Should().HaveCount(1);
        result[1].First().Should().BeOfType<TextMessageContent>();
    }

    [Fact]
    public void GroupContentByToolBoundary_TextThenTool_ShouldReturnSingleGroup()
    {
        var message = new AssistantMessage
        {
            Content =
            [
                new TextMessageContent { Value = "intro" },
                new ToolMessageContent { Id = "1", Name = "tool1", IsApproved = true }
            ]
        };

        // Text -> Tool: 이전이 Tool이 아니므로 분리 안 됨
        var result = message.GroupContentByToolBoundary().ToList();

        result.Should().HaveCount(1);
        result[0].Should().HaveCount(2);
    }

    [Fact]
    public void GroupContentByToolBoundary_ComplexExample_FromDocComment()
    {
        // 문서 예제: [Thinking, Tool, Tool, Text, Tool, Text]
        //        => [Thinking, Tool, Tool], [Text, Tool], [Text]
        var message = new AssistantMessage
        {
            Content =
            [
                new ThinkingMessageContent { Value = "thinking" },
                new ToolMessageContent { Id = "1", Name = "tool1", IsApproved = true },
                new ToolMessageContent { Id = "2", Name = "tool2", IsApproved = true },
                new TextMessageContent { Value = "text1" },
                new ToolMessageContent { Id = "3", Name = "tool3", IsApproved = true },
                new TextMessageContent { Value = "text2" }
            ]
        };

        var result = message.GroupContentByToolBoundary().ToList();

        result.Should().HaveCount(3);
        result[0].Should().HaveCount(3); // Thinking, Tool, Tool
        result[1].Should().HaveCount(2); // Text, Tool
        result[2].Should().HaveCount(1); // Text
    }

    [Fact]
    public void GroupContentByToolBoundary_SingleText_ShouldReturnSingleGroup()
    {
        var message = new AssistantMessage
        {
            Content = [new TextMessageContent { Value = "only text" }]
        };

        var result = message.GroupContentByToolBoundary().ToList();

        result.Should().HaveCount(1);
        result[0].Should().HaveCount(1);
    }

    [Fact]
    public void GroupContentByToolBoundary_SingleTool_ShouldReturnSingleGroup()
    {
        var message = new AssistantMessage
        {
            Content = [new ToolMessageContent { Id = "1", Name = "tool1", IsApproved = true }]
        };

        var result = message.GroupContentByToolBoundary().ToList();

        result.Should().HaveCount(1);
        result[0].Should().HaveCount(1);
    }
}
