using FluentAssertions;
using IronHive.Core.Files.Decoders;

namespace IronHive.Tests.Files;

public class ImageDecoderTests
{
    private readonly ImageDecoder _decoder = new();

    [Theory]
    [InlineData("image/png", true)]
    [InlineData("image/jpeg", true)]
    [InlineData("image/gif", true)]
    [InlineData("image/webp", false)]
    [InlineData("image/svg+xml", false)]
    [InlineData("text/plain", false)]
    [InlineData("application/pdf", false)]
    public void SupportsMimeType_ReturnsExpected(string mimeType, bool expected)
    {
        _decoder.SupportsMimeType(mimeType).Should().Be(expected);
    }

    [Fact]
    public async Task DecodeAsync_ReturnsBase64String()
    {
        var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG header
        using var stream = new MemoryStream(bytes);

        var result = await _decoder.DecodeAsync(stream);

        result.Should().Be(Convert.ToBase64String(bytes));
    }

    [Fact]
    public async Task DecodeAsync_EmptyStream_ReturnsEmptyBase64()
    {
        using var stream = new MemoryStream();

        var result = await _decoder.DecodeAsync(stream);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DecodeAsync_Cancellation_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        using var stream = new MemoryStream(new byte[1024]);

        var act = () => _decoder.DecodeAsync(stream, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}

public class PDFDecoderMimeTests
{
    private readonly PDFDecoder _decoder = new();

    [Theory]
    [InlineData("application/pdf", true)]
    [InlineData("application/x-pdf", false)]
    [InlineData("text/pdf", false)]
    [InlineData("image/png", false)]
    public void SupportsMimeType_ReturnsExpected(string mimeType, bool expected)
    {
        _decoder.SupportsMimeType(mimeType).Should().Be(expected);
    }
}

public class PPTDecoderMimeTests
{
    private readonly PPTDecoder _decoder = new();

    [Theory]
    [InlineData("application/vnd.openxmlformats-officedocument.presentationml.presentation", true)]
    [InlineData("application/vnd.ms-powerpoint", false)]
    [InlineData("application/pdf", false)]
    public void SupportsMimeType_ReturnsExpected(string mimeType, bool expected)
    {
        _decoder.SupportsMimeType(mimeType).Should().Be(expected);
    }
}

public class WordDecoderMimeTests
{
    private readonly WordDecoder _decoder = new();

    [Theory]
    [InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document", true)]
    [InlineData("application/msword", false)]
    [InlineData("text/plain", false)]
    public void SupportsMimeType_ReturnsExpected(string mimeType, bool expected)
    {
        _decoder.SupportsMimeType(mimeType).Should().Be(expected);
    }
}
