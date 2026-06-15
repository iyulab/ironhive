using FluentAssertions;
using IronHive.Providers.OpenAI.Compatible.GpuStack;

namespace IronHive.Tests.Providers;

public class GpuStackConfigTests
{
    [Fact]
    public void ToOpenAI_DefaultBaseUrl_AppendsSuffix()
    {
        var config = new GpuStackConfig();
        var openAI = config.ToOpenAI();
        openAI.BaseUrl.Should().Be("http://localhost:8080/v1-openai/");
    }

    [Fact]
    public void ToOpenAI_CustomBaseUrl_AppendsSuffix()
    {
        var config = new GpuStackConfig { BaseUrl = "http://172.19.10.10:8080" };
        var openAI = config.ToOpenAI();
        openAI.BaseUrl.Should().Be("http://172.19.10.10:8080/v1-openai/");
    }

    [Fact]
    public void ToOpenAI_BaseUrlWithTrailingSlash_DoesNotDoubleSuffix()
    {
        var config = new GpuStackConfig { BaseUrl = "http://172.19.10.10:8080/" };
        var openAI = config.ToOpenAI();
        openAI.BaseUrl.Should().Be("http://172.19.10.10:8080/v1-openai/");
    }

    [Fact]
    public void ToOpenAI_ApiKey_IsPreserved()
    {
        var config = new GpuStackConfig { ApiKey = "my-key" };
        var openAI = config.ToOpenAI();
        openAI.ApiKey.Should().Be("my-key");
    }

    [Fact]
    public void ToOpenAI_NoApiKey_EmptyString()
    {
        var config = new GpuStackConfig();
        var openAI = config.ToOpenAI();
        openAI.ApiKey.Should().BeEmpty();
    }

    [Fact]
    public void ToOpenAI_EmptyBaseUrl_FallsBackToDefault()
    {
        var config = new GpuStackConfig { BaseUrl = "" };
        var openAI = config.ToOpenAI();
        openAI.BaseUrl.Should().Be("http://localhost:8080/v1-openai/");
    }

    [Fact]
    public void ToOpenAI_ApiKeyResolver_IsUsed()
    {
        var config = new GpuStackConfig { ApiKey = "static", ApiKeyResolver = () => "dynamic" };
        var openAI = config.ToOpenAI();
        openAI.ApiKey.Should().Be("dynamic");
    }

    [Fact]
    public void ToOpenAI_ApiKeyResolver_TakesPrecedenceOverStatic()
    {
        var config = new GpuStackConfig { ApiKey = "static" };
        config.ToOpenAI().ApiKey.Should().Be("static");

        config.ApiKeyResolver = () => "rotated";
        config.ToOpenAI().ApiKey.Should().Be("rotated");
    }

    [Fact]
    public void ToOpenAI_ApiKeyResolverReturnsNullOrEmpty_FallsBackToStatic()
    {
        var nullResolver = new GpuStackConfig { ApiKey = "static", ApiKeyResolver = () => null };
        nullResolver.ToOpenAI().ApiKey.Should().Be("static");

        var emptyResolver = new GpuStackConfig { ApiKey = "static", ApiKeyResolver = () => "  " };
        emptyResolver.ToOpenAI().ApiKey.Should().Be("static");
    }

    [Fact]
    public void ToOpenAI_ApiKeyResolver_EvaluatedOnEveryCall()
    {
        var current = "key-1";
        var config = new GpuStackConfig { ApiKeyResolver = () => current };
        config.ToOpenAI().ApiKey.Should().Be("key-1");

        current = "key-2";
        config.ToOpenAI().ApiKey.Should().Be("key-2");
    }
}
