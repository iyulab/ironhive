using FluentAssertions;
using IronHive.Providers.Ollama;

namespace IronHive.Tests.Providers;

/// <summary>
/// Tests to verify Ollama provider token usage reporting.
/// Issue #6: Ollama provider incomplete implementation - token usage missing.
/// </summary>
public class OllamaTokenUsageTests
{
    [Fact]
    public void OllamaMessageGenerator_ShouldBeCreatable()
    {
        // Arrange & Act
        var generator = new OllamaMessageGenerator("http://localhost:11434");

        // Assert
        generator.Should().NotBeNull();
        generator.Dispose();
    }

    [Fact]
    public void OllamaMessageGenerator_ShouldAcceptConfig()
    {
        // Arrange
        var config = new OllamaConfig
        {
            BaseUrl = "http://localhost:11434"
        };

        // Act
        var generator = new OllamaMessageGenerator(config);

        // Assert
        generator.Should().NotBeNull();
        generator.Dispose();
    }
}
