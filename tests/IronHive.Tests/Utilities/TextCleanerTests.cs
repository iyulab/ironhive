using FluentAssertions;
using IronHive.Core.Utilities;

namespace IronHive.Tests.Utilities;

public class TextCleanerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\n")]
    public void Clean_NullOrWhitespace_ReturnsEmpty(string? input)
    {
        TextCleaner.Clean(input!).Should().BeEmpty();
    }

    [Fact]
    public void Clean_NormalText_ReturnsUnchanged()
    {
        var result = TextCleaner.Clean("Hello world");
        result.Should().Be("Hello world");
    }

    [Fact]
    public void Clean_NormalizesCRLF_ToLF()
    {
        var result = TextCleaner.Clean("line1\r\nline2\rline3");
        result.Should().Be("line1\nline2\nline3");
    }

    [Fact]
    public void Clean_TrimsExcessiveLineBreaks()
    {
        var result = TextCleaner.Clean("paragraph1\n\n\n\n\nparagraph2");
        result.Should().Be("paragraph1\n\nparagraph2");
    }

    [Fact]
    public void Clean_TrimsMultipleSpaces()
    {
        var result = TextCleaner.Clean("hello    world   test");
        result.Should().Be("hello world test");
    }

    [Fact]
    public void Clean_TrimsLeadingTrailingSpacesPerLine()
    {
        var result = TextCleaner.Clean("  line1  \n  line2  ");
        result.Should().Be("line1\nline2");
    }

    [Fact]
    public void Clean_CombinedNormalization()
    {
        var result = TextCleaner.Clean("  hello   world  \r\n\r\n\r\n\r\n  next   paragraph  ");
        result.Should().Be("hello world\n\nnext paragraph");
    }
}
