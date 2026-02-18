using FluentAssertions;
using IronHive.Abstractions.Files;
using IronHive.Core.Files;
using NSubstitute;

namespace IronHive.Tests.Files;

public class FileExtractionServiceTests
{
    // --- Constructor ---

    [Fact]
    public void Constructor_NullDetector_Throws()
    {
        var act = () => new FileExtractionService<string>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullDecoders_CreatesEmptyService()
    {
        var detector = Substitute.For<IFileMediaTypeDetector>();
        detector.Extensions.Returns([]);
        detector.MediaTypes.Returns([]);

        var service = new FileExtractionService<string>(detector, null);

        service.SupportedExtensions.Should().BeEmpty();
        service.SupportedMimeTypes.Should().BeEmpty();
    }

    // --- SupportedExtensions ---

    [Fact]
    public void SupportedExtensions_ReturnsOnlyExtensionsWithMatchingDecoder()
    {
        var detector = Substitute.For<IFileMediaTypeDetector>();
        var extensions = new[] { ".pdf", ".jpg", ".xyz" };
        detector.Extensions.Returns(extensions);
        detector.TryDetect(".pdf", out Arg.Any<string?>())
            .Returns(x => { x[1] = "application/pdf"; return true; });
        detector.TryDetect(".jpg", out Arg.Any<string?>())
            .Returns(x => { x[1] = "image/jpeg"; return true; });
        detector.TryDetect(".xyz", out Arg.Any<string?>())
            .Returns(x => { x[1] = "application/xyz"; return true; });

        var pdfDecoder = Substitute.For<IFileDecoder<string>>();
        pdfDecoder.SupportsMimeType("application/pdf").Returns(true);
        pdfDecoder.SupportsMimeType(Arg.Is<string>(m => m != "application/pdf")).Returns(false);

        var service = new FileExtractionService<string>(
            detector, new[] { pdfDecoder });

        service.SupportedExtensions.Should().ContainSingle(".pdf");
    }

    // --- SupportedMimeTypes ---

    [Fact]
    public void SupportedMimeTypes_ReturnsOnlyTypesWithMatchingDecoder()
    {
        var detector = Substitute.For<IFileMediaTypeDetector>();
        detector.Extensions.Returns(Array.Empty<string>());
        var mediaTypes = new[] { "text/plain", "application/pdf", "image/png" };
        detector.MediaTypes.Returns(mediaTypes);

        var textDecoder = Substitute.For<IFileDecoder<string>>();
        textDecoder.SupportsMimeType("text/plain").Returns(true);
        textDecoder.SupportsMimeType(Arg.Is<string>(m => m != "text/plain")).Returns(false);

        var service = new FileExtractionService<string>(
            detector, new[] { textDecoder });

        service.SupportedMimeTypes.Should().ContainSingle("text/plain");
    }

    // --- DecodeAsync ---

    [Fact]
    public async Task DecodeAsync_NullData_Throws()
    {
        var detector = Substitute.For<IFileMediaTypeDetector>();
        detector.Extensions.Returns(Array.Empty<string>());
        detector.MediaTypes.Returns(Array.Empty<string>());

        var service = new FileExtractionService<string>(detector);

        var act = () => service.DecodeAsync("file.txt", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DecodeAsync_EmptyFileName_Throws(string? fileName)
    {
        var detector = Substitute.For<IFileMediaTypeDetector>();
        detector.Extensions.Returns(Array.Empty<string>());
        detector.MediaTypes.Returns(Array.Empty<string>());

        var service = new FileExtractionService<string>(detector);

        var act = () => service.DecodeAsync(fileName!, new MemoryStream());

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DecodeAsync_UnknownExtension_ThrowsNotSupported()
    {
        var detector = Substitute.For<IFileMediaTypeDetector>();
        detector.Extensions.Returns(Array.Empty<string>());
        detector.MediaTypes.Returns(Array.Empty<string>());
        detector.TryDetect("file.xyz", out Arg.Any<string?>()).Returns(false);

        var service = new FileExtractionService<string>(detector);

        var act = () => service.DecodeAsync("file.xyz", new MemoryStream());

        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*Unknown content type*");
    }

    [Fact]
    public async Task DecodeAsync_NoMatchingDecoder_ThrowsNotSupported()
    {
        var detector = Substitute.For<IFileMediaTypeDetector>();
        detector.Extensions.Returns(Array.Empty<string>());
        detector.MediaTypes.Returns(Array.Empty<string>());
        detector.TryDetect("file.pdf", out Arg.Any<string?>())
            .Returns(x => { x[1] = "application/pdf"; return true; });

        // No decoders registered
        var service = new FileExtractionService<string>(detector);

        var act = () => service.DecodeAsync("file.pdf", new MemoryStream());

        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*No decoder*");
    }

    [Fact]
    public async Task DecodeAsync_MatchingDecoder_ReturnsDecodedResult()
    {
        var detector = Substitute.For<IFileMediaTypeDetector>();
        detector.Extensions.Returns(Array.Empty<string>());
        detector.MediaTypes.Returns(Array.Empty<string>());
        detector.TryDetect("doc.pdf", out Arg.Any<string?>())
            .Returns(x => { x[1] = "application/pdf"; return true; });

        var decoder = Substitute.For<IFileDecoder<string>>();
        decoder.SupportsMimeType("application/pdf").Returns(true);
        decoder.DecodeAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("decoded content"));

        var service = new FileExtractionService<string>(
            detector, new[] { decoder });

        var result = await service.DecodeAsync("doc.pdf", new MemoryStream());

        result.Should().Be("decoded content");
    }

    [Fact]
    public async Task DecodeAsync_SeekableStream_ResetsPosition()
    {
        var detector = Substitute.For<IFileMediaTypeDetector>();
        detector.Extensions.Returns(Array.Empty<string>());
        detector.MediaTypes.Returns(Array.Empty<string>());
        detector.TryDetect("file.txt", out Arg.Any<string?>())
            .Returns(x => { x[1] = "text/plain"; return true; });

        long positionWhenDecoded = -1;
        var decoder = Substitute.For<IFileDecoder<string>>();
        decoder.SupportsMimeType("text/plain").Returns(true);
        decoder.DecodeAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                positionWhenDecoded = callInfo.Arg<Stream>().Position;
                return Task.FromResult("text");
            });

        var service = new FileExtractionService<string>(
            detector, new[] { decoder });

        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        stream.Position = 2; // Move to non-zero position

        await service.DecodeAsync("file.txt", stream);

        positionWhenDecoded.Should().Be(0, "stream position should be reset before decoding");
    }

    [Fact]
    public async Task DecodeAsync_FirstMatchingDecoder_IsUsed()
    {
        var detector = Substitute.For<IFileMediaTypeDetector>();
        detector.Extensions.Returns(Array.Empty<string>());
        detector.MediaTypes.Returns(Array.Empty<string>());
        detector.TryDetect("file.pdf", out Arg.Any<string?>())
            .Returns(x => { x[1] = "application/pdf"; return true; });

        var firstDecoder = Substitute.For<IFileDecoder<string>>();
        firstDecoder.SupportsMimeType("application/pdf").Returns(true);
        firstDecoder.DecodeAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("first"));

        var secondDecoder = Substitute.For<IFileDecoder<string>>();
        secondDecoder.SupportsMimeType("application/pdf").Returns(true);
        secondDecoder.DecodeAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("second"));

        var service = new FileExtractionService<string>(
            detector, new[] { firstDecoder, secondDecoder });

        var result = await service.DecodeAsync("file.pdf", new MemoryStream());

        result.Should().Be("first");
    }
}
