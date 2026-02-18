using FluentAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;

namespace IronHive.Tests.Messages;

public class TextMessageContentTests
{
    [Fact]
    public void Merge_TextDelta_ShouldAppendValue()
    {
        var content = new TextMessageContent { Value = "Hello" };
        var delta = new TextDeltaContent { Value = " World" };

        content.Merge(delta);

        content.Value.Should().Be("Hello World");
    }

    [Fact]
    public void Merge_MultipleDelta_ShouldAccumulate()
    {
        var content = new TextMessageContent { Value = "" };

        content.Merge(new TextDeltaContent { Value = "a" });
        content.Merge(new TextDeltaContent { Value = "b" });
        content.Merge(new TextDeltaContent { Value = "c" });

        content.Value.Should().Be("abc");
    }

    [Fact]
    public void Merge_WrongDeltaType_ShouldThrow()
    {
        var content = new TextMessageContent { Value = "text" };
        var delta = new ToolDeltaContent { Input = "wrong" };

        var act = () => content.Merge(delta);

        act.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain("TextMessageContent");
    }
}

public class ThinkingMessageContentTests
{
    [Fact]
    public void Merge_ThinkingDelta_ShouldAppendData()
    {
        var content = new ThinkingMessageContent { Value = "start" };
        var delta = new ThinkingDeltaContent { Data = " more" };

        content.Merge(delta);

        content.Value.Should().Be("start more");
    }

    [Fact]
    public void Merge_WrongDeltaType_ShouldThrow()
    {
        var content = new ThinkingMessageContent { Value = "think" };
        var delta = new TextDeltaContent { Value = "wrong" };

        var act = () => content.Merge(delta);

        act.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain("ThinkingMessageContent");
    }

    [Fact]
    public void Update_ThinkingUpdated_ShouldSetSignature()
    {
        var content = new ThinkingMessageContent { Value = "data", Signature = null };
        var updated = new ThinkingUpdatedContent { Signature = "sig-abc-123" };

        content.Update(updated);

        content.Signature.Should().Be("sig-abc-123");
    }

    [Fact]
    public void Update_WrongUpdatedType_ShouldThrow()
    {
        var content = new ThinkingMessageContent { Value = "data" };
        var updated = new ToolUpdatedContent { Output = ToolOutput.Success("result") };

        var act = () => content.Update(updated);

        act.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain("ThinkingMessageContent");
    }
}

public class ToolMessageContentMergeUpdateTests
{
    [Fact]
    public void IsCompleted_WithOutput_ShouldBeTrue()
    {
        var content = new ToolMessageContent
        {
            Id = "1", Name = "tool", IsApproved = true,
            Output = ToolOutput.Success("done")
        };

        content.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void IsCompleted_WithoutOutput_ShouldBeFalse()
    {
        var content = new ToolMessageContent
        {
            Id = "1", Name = "tool", IsApproved = true,
            Output = null
        };

        content.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void Merge_ToolDelta_ShouldAppendInput()
    {
        var content = new ToolMessageContent
        {
            Id = "1", Name = "tool", IsApproved = true,
            Input = "{\"key\":"
        };
        var delta = new ToolDeltaContent { Input = "\"value\"}" };

        content.Merge(delta);

        content.Input.Should().Be("{\"key\":\"value\"}");
    }

    [Fact]
    public void Merge_ToolDelta_WithNullInput_ShouldStartFromDelta()
    {
        var content = new ToolMessageContent
        {
            Id = "1", Name = "tool", IsApproved = true,
            Input = null
        };
        var delta = new ToolDeltaContent { Input = "hello" };

        content.Merge(delta);

        // null + "hello" = "hello" (C# string concatenation)
        content.Input.Should().Be("hello");
    }

    [Fact]
    public void Merge_WrongDeltaType_ShouldThrow()
    {
        var content = new ToolMessageContent
        {
            Id = "1", Name = "tool", IsApproved = true
        };
        var delta = new TextDeltaContent { Value = "wrong" };

        var act = () => content.Merge(delta);

        act.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain("ToolMessageContent");
    }

    [Fact]
    public void Update_ToolUpdated_ShouldNotThrow()
    {
        var content = new ToolMessageContent
        {
            Id = "1", Name = "tool", IsApproved = true
        };
        var updated = new ToolUpdatedContent { Output = ToolOutput.Success("result") };

        var act = () => content.Update(updated);

        act.Should().NotThrow();
    }

    [Fact]
    public void Update_WrongUpdatedType_ShouldThrow()
    {
        var content = new ToolMessageContent
        {
            Id = "1", Name = "tool", IsApproved = true
        };
        var updated = new ThinkingUpdatedContent { Signature = "sig" };

        var act = () => content.Update(updated);

        act.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain("ToolMessageContent");
    }
}
