using System.Text;
using FluentAssertions;
using IronHive.Abstractions.Files;
using IronHive.Core.Files;
using NSubstitute;

namespace IronHive.Tests.Files;

public class FileParserServiceTests
{
    // --- ParseAsync argument validation ---

    [Fact]
    public async Task ParseAsync_NullStream_Throws()
    {
        var service = new FileParserService();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.ParseAsync("file.txt", null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ParseAsync_EmptyFileName_Throws(string? fileName)
    {
        var service = new FileParserService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ParseAsync(fileName!, new MemoryStream()));
    }

    // --- ParseAsync with matching parser ---

    [Fact]
    public async Task ParseAsync_MatchingParser_DelegatesToParser()
    {
        var expected = new[] { new TextBlock { Text = "hello" } };
        var parser = Substitute.For<IFileParser>();
        parser.CanParse("doc.pdf", null).Returns(true);
        parser.ParseAsync("doc.pdf", Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<FileBlock>>(expected));

        var service = new FileParserService([parser]);

        var result = await service.ParseAsync("doc.pdf", new MemoryStream());

        result.Should().BeSameAs(expected);
    }

    // --- Text fallback ---

    [Fact]
    public async Task ParseAsync_UnknownExtension_TextFile_ReturnsTextBlock()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, world!"));

        var service = new FileParserService();

        var result = await service.ParseAsync("readme.unknown", stream);

        result.Should().ContainSingle()
            .Which.Should().BeOfType<TextBlock>()
            .Which.Text.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ParseAsync_UnknownExtension_BinaryFile_ReturnsBinaryBlock()
    {
        var bytes = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        using var stream = new MemoryStream(bytes);

        var service = new FileParserService();

        var result = await service.ParseAsync("file.bin", stream);

        result.Should().ContainSingle()
            .Which.Should().BeOfType<BinaryBlock>();
    }

    [Fact]
    public async Task ParseAsync_SeekableStream_ResetsPositionBeforeParsing()
    {
        var parser = Substitute.For<IFileParser>();
        parser.CanParse("file.txt", null).Returns(true);

        long positionOnParse = -1;
        parser.ParseAsync("file.txt", Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                positionOnParse = callInfo.Arg<Stream>().Position;
                return Task.FromResult<IReadOnlyList<FileBlock>>([]);
            });

        var service = new FileParserService([parser]);
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        stream.Position = 2;

        await service.ParseAsync("file.txt", stream);

        positionOnParse.Should().Be(0);
    }
}
