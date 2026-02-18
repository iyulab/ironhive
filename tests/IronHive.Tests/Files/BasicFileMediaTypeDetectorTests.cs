using FluentAssertions;
using IronHive.Core.Files.Detectors;

namespace IronHive.Tests.Files;

public class BasicFileMediaTypeDetectorTests
{
    private readonly BasicFileMediaTypeDetector _detector = new();

    // --- Extensions / MediaTypes properties ---

    [Fact]
    public void Extensions_ShouldContainKnownExtensions()
    {
        var extensions = _detector.Extensions.ToList();

        extensions.Should().NotBeEmpty();
        extensions.Should().Contain(".jpg");
        extensions.Should().Contain(".pdf");
        extensions.Should().Contain(".html");
        extensions.Should().Contain(".json");
    }

    [Fact]
    public void MediaTypes_ShouldContainKnownTypes()
    {
        var types = _detector.MediaTypes.ToList();

        types.Should().NotBeEmpty();
        types.Should().Contain("image/jpeg");
        types.Should().Contain("application/pdf");
        types.Should().Contain("text/html");
        types.Should().Contain("application/json");
    }

    // --- Detect ---

    [Theory]
    [InlineData("photo.jpg", "image/jpeg")]
    [InlineData("document.pdf", "application/pdf")]
    [InlineData("page.html", "text/html")]
    [InlineData("data.json", "application/json")]
    [InlineData("style.css", "text/css")]
    [InlineData("image.png", "image/png")]
    [InlineData("archive.zip", "application/x-zip-compressed")]
    [InlineData("readme.txt", "text/plain")]
    [InlineData("video.mp4", "video/mp4")]
    [InlineData("song.mp3", "audio/mpeg")]
    public void Detect_KnownExtension_ReturnsCorrectMimeType(string fileName, string expected)
    {
        _detector.Detect(fileName).Should().Be(expected);
    }

    [Theory]
    [InlineData("photo.JPG", "image/jpeg")]
    [InlineData("DOCUMENT.PDF", "application/pdf")]
    [InlineData("Page.Html", "text/html")]
    [InlineData("DATA.JSON", "application/json")]
    public void Detect_CaseInsensitive_ReturnsCorrectMimeType(string fileName, string expected)
    {
        _detector.Detect(fileName).Should().Be(expected);
    }

    [Fact]
    public void Detect_WithDirectoryPath_ExtractsExtension()
    {
        _detector.Detect("/some/path/to/file.pdf").Should().Be("application/pdf");
        _detector.Detect("C:\\Users\\docs\\readme.txt").Should().Be("text/plain");
    }

    [Fact]
    public void Detect_UnknownExtension_ReturnsNull()
    {
        _detector.Detect("file.xyz123").Should().BeNull();
    }

    [Fact]
    public void Detect_NoExtension_ReturnsNull()
    {
        _detector.Detect("Makefile").Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Detect_NullOrWhitespace_ReturnsNull(string? fileName)
    {
        _detector.Detect(fileName!).Should().BeNull();
    }

    // --- TryDetect ---

    [Fact]
    public void TryDetect_KnownExtension_ReturnsTrueWithMimeType()
    {
        var result = _detector.TryDetect("file.png", out var mimeType);

        result.Should().BeTrue();
        mimeType.Should().Be("image/png");
    }

    [Fact]
    public void TryDetect_UnknownExtension_ReturnsFalse()
    {
        var result = _detector.TryDetect("file.unknown", out var mimeType);

        result.Should().BeFalse();
        mimeType.Should().BeNull();
    }

    [Fact]
    public void TryDetect_NullInput_ReturnsFalse()
    {
        var result = _detector.TryDetect(null!, out var mimeType);

        result.Should().BeFalse();
        mimeType.Should().BeNull();
    }

    [Fact]
    public void TryDetect_EmptyInput_ReturnsFalse()
    {
        var result = _detector.TryDetect("", out var mimeType);

        result.Should().BeFalse();
        mimeType.Should().BeNull();
    }

    // --- Multiple extensions for same MIME type ---

    [Theory]
    [InlineData("file.htm", "text/html")]
    [InlineData("file.html", "text/html")]
    public void Detect_MultipleExtensionsSameMime_BothWork(string fileName, string expected)
    {
        _detector.Detect(fileName).Should().Be(expected);
    }

    [Theory]
    [InlineData("file.jpeg", "image/jpeg")]
    [InlineData("file.jpg", "image/jpeg")]
    [InlineData("file.jpe", "image/jpeg")]
    public void Detect_JpegVariants_AllReturnImageJpeg(string fileName, string expected)
    {
        _detector.Detect(fileName).Should().Be(expected);
    }

    // --- Office document types ---

    [Theory]
    [InlineData("report.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("data.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [InlineData("slides.pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation")]
    public void Detect_OfficeFormats_ReturnsCorrectMimeType(string fileName, string expected)
    {
        _detector.Detect(fileName).Should().Be(expected);
    }

    // --- Web/font types ---

    [Theory]
    [InlineData("font.woff2", "font/woff2")]
    [InlineData("font.otf", "font/otf")]
    [InlineData("icon.svg", "image/svg+xml")]
    [InlineData("image.webp", "image/webp")]
    public void Detect_ModernWebFormats_ReturnsCorrectMimeType(string fileName, string expected)
    {
        _detector.Detect(fileName).Should().Be(expected);
    }
}
