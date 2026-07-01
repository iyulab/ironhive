using FluentAssertions;
using IronHive.Providers.OpenAI.Compatible;

namespace IronHive.Tests.Providers;

public class OpenAICompatibleConfigTests
{
    [Fact]
    public void ToOpenAI_DefaultBaseUrl_AppendsV1()
    {
        var openAI = new OpenAICompatibleConfig().ToOpenAI();
        openAI.BaseUrl.Should().Be("http://localhost:11434/v1");
    }

    [Fact]
    public void ToOpenAI_CustomBaseUrl_AppendsV1()
    {
        var config = new OpenAICompatibleConfig { BaseUrl = "http://localhost:1234" };
        config.ToOpenAI().BaseUrl.Should().Be("http://localhost:1234/v1");
    }

    [Fact]
    public void ToOpenAI_TrailingSlash_DoesNotDoubleSlash()
    {
        var config = new OpenAICompatibleConfig { BaseUrl = "http://localhost:8000/" };
        config.ToOpenAI().BaseUrl.Should().Be("http://localhost:8000/v1");
    }

    [Fact]
    public void ToOpenAI_BaseUrlAlreadyEndsWithPath_IsIdempotent()
    {
        var config = new OpenAICompatibleConfig { BaseUrl = "http://localhost:11434/v1" };
        config.ToOpenAI().BaseUrl.Should().Be("http://localhost:11434/v1");
    }

    [Fact]
    public void ToOpenAI_CustomPath_IsApplied()
    {
        var config = new OpenAICompatibleConfig { BaseUrl = "http://host:9000", Path = "/openai/v1" };
        config.ToOpenAI().BaseUrl.Should().Be("http://host:9000/openai/v1");
    }

    [Fact]
    public void KeyOptional_UsableWithoutKey()
    {
        var config = new OpenAICompatibleConfig { BaseUrl = "http://localhost:11434" };
        config.IsUsable.Should().BeTrue();      // endpoint reachable
        config.IsConfigured.Should().BeFalse(); // but no key — fine for a LAN service
        config.ToOpenAI().ApiKey.Should().BeEmpty();
    }

    [Fact]
    public void KeyPresent_IsConfigured()
    {
        var config = new OpenAICompatibleConfig { ApiKey = "k" };
        config.IsConfigured.Should().BeTrue();
        config.ToOpenAI().ApiKey.Should().Be("k");
    }

    [Fact]
    public void BaseUrlResolver_TakesPrecedence_AndEvaluatedEveryCall()
    {
        var current = "http://a:1234";
        var config = new OpenAICompatibleConfig { BaseUrl = "http://static:11434", BaseUrlResolver = () => current };
        config.ToOpenAI().BaseUrl.Should().Be("http://a:1234/v1");

        current = "http://b:1234";
        config.ToOpenAI().BaseUrl.Should().Be("http://b:1234/v1");
    }

    [Fact]
    public void BaseUrlResolver_NullOrEmpty_FallsBackToStatic()
    {
        var config = new OpenAICompatibleConfig { BaseUrl = "http://static:11434", BaseUrlResolver = () => "  " };
        config.ToOpenAI().BaseUrl.Should().Be("http://static:11434/v1");
    }

    [Fact]
    public void ApiKeyResolver_TakesPrecedence_AndEvaluatedEveryCall()
    {
        var current = "key-1";
        var config = new OpenAICompatibleConfig { ApiKeyResolver = () => current };
        config.ToOpenAI().ApiKey.Should().Be("key-1");

        current = "key-2";
        config.ToOpenAI().ApiKey.Should().Be("key-2");
    }
}
