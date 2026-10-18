using AwesomeAssertions;
using IronHive.Abstractions.Messages;

namespace IronHive.Tests.Messages;

/// <summary>
/// <see cref="SuggestionCollector"/>: suggestion blocks leave the answer text — whole or split across pieces — and come back
/// as <see cref="Suggestion"/>s; text it held back to see whether a block starts is returned by <see cref="SuggestionCollector.Flush"/>.
/// </summary>
public class SuggestionCollectorTests
{
    private const string Answer = "Here is the plan.\n<suggestion>\nQ: Which format?\nA: PDF\nA: Word\n</suggestion>";

    [Fact]
    public void A_block_leaves_the_text_and_is_collected()
    {
        var collector = new SuggestionCollector();

        var text = collector.Feed(Answer) + collector.Flush();

        text.Should().Be("Here is the plan.\n");
        var suggestion = collector.Drain().Should().ContainSingle().Subject;
        suggestion.Question.Should().Be("Which format?");
        suggestion.Items.Should().Equal("PDF", "Word");
        collector.Drain().Should().BeNull("drained");
    }

    [Fact]
    public void A_block_split_across_pieces_is_collected_the_same()
    {
        var collector = new SuggestionCollector();

        var text = string.Concat(Answer.Chunk(3).Select(piece => collector.Feed(new string(piece)))) + collector.Flush();

        text.Should().Be("Here is the plan.\n");
        collector.Drain().Should().ContainSingle().Which.Items.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("a < b")]
    [InlineData("ends with <")]
    [InlineData("ends with <sugg")]
    [InlineData("x <suggestion>Q: never closed")]
    public void Text_that_is_not_a_closed_block_survives_the_flush(string answer)
    {
        var collector = new SuggestionCollector();

        (collector.Feed(answer) + collector.Flush()).Should().Be(answer);
        collector.Drain().Should().BeNull();
    }

    [Fact]
    public void The_prompt_is_appended_only_when_options_are_given()
    {
        SuggestionCollector.Prompt("sys", null).Should().Be("sys");
        SuggestionCollector.Prompt("sys", new SuggestionOptions { Mode = SuggestionMode.Always })
            .Should().StartWith("sys\n\nAlways end your response with the following block:").And.Contain("<suggestion>");
        SuggestionCollector.Prompt(null, new SuggestionOptions { MaxCount = 2 }).Should().Contain("up to 2 of the following blocks");
    }
}
