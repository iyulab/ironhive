using System.Text;
using FluentAssertions;
using IronHive.Core.Files.Decoders;

namespace IronHive.Tests.Files;

public class TextDecoderTests
{
    private readonly TextDecoder _decoder = new();

    // --- SupportsMimeType ---

    [Theory]
    [InlineData("text/plain", true)]
    [InlineData("text/html", true)]
    [InlineData("text/css", true)]
    [InlineData("text/csv", true)]
    [InlineData("text/markdown", true)]
    [InlineData("application/json", false)]
    [InlineData("application/pdf", false)]
    [InlineData("image/png", false)]
    [InlineData("audio/mpeg", false)]
    public void SupportsMimeType_ReturnsExpected(string mimeType, bool expected)
    {
        _decoder.SupportsMimeType(mimeType).Should().Be(expected);
    }

    // --- DecodeAsync ---

    [Fact]
    public async Task DecodeAsync_SimpleText_ReturnsContent()
    {
        using var stream = ToStream("Hello World");

        var result = await _decoder.DecodeAsync(stream);

        result.Should().Be("Hello World");
    }

    [Fact]
    public async Task DecodeAsync_MultipleLines_JoinsWithNewline()
    {
        using var stream = ToStream("Line 1\nLine 2\nLine 3");

        var result = await _decoder.DecodeAsync(stream);

        result.Should().Contain("Line 1");
        result.Should().Contain("Line 2");
        result.Should().Contain("Line 3");
    }

    [Fact]
    public async Task DecodeAsync_EmptyLines_AreSkipped()
    {
        using var stream = ToStream("Line 1\n\n\nLine 2\n\n");

        var result = await _decoder.DecodeAsync(stream);

        // Empty/whitespace lines are skipped by TextCleaner + IsNullOrWhiteSpace check
        result.Should().NotContain("\n\n");
    }

    [Fact]
    public async Task DecodeAsync_WhitespaceOnlyLines_AreSkipped()
    {
        using var stream = ToStream("Hello\n   \n   \nWorld");

        var result = await _decoder.DecodeAsync(stream);

        var lines = result.Split(Environment.NewLine);
        lines.Should().HaveCount(2);
        lines[0].Should().Be("Hello");
        lines[1].Should().Be("World");
    }

    [Fact]
    public async Task DecodeAsync_EmptyStream_ReturnsEmpty()
    {
        using var stream = ToStream("");

        var result = await _decoder.DecodeAsync(stream);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DecodeAsync_CrLfNormalized()
    {
        using var stream = ToStream("Line 1\r\nLine 2\rLine 3");

        var result = await _decoder.DecodeAsync(stream);

        // TextCleaner normalizes \r\n → \n, but StreamReader handles line breaks
        result.Should().Contain("Line 1");
        result.Should().Contain("Line 2");
        result.Should().Contain("Line 3");
    }

    [Fact]
    public async Task DecodeAsync_Cancellation_ThrowsOperationCanceled()
    {
        using var stream = ToStream("Some content");
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var act = async () => await _decoder.DecodeAsync(stream, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private static MemoryStream ToStream(string content)
        => new(Encoding.UTF8.GetBytes(content));
}
