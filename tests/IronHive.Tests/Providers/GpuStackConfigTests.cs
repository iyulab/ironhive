using AwesomeAssertions;
using IronHive.Providers.OpenAI.Compatible;
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

    [Fact]
    public void ToRerankConfig_DefaultBaseUrl_TargetsV1NotV1Openai()
    {
        // GPUStack serves its Jina/Cohere-shaped rerank endpoint at /v1/rerank — a different path
        // than the /v1-openai/ surface ToOpenAI() targets for chat/embeddings/models.
        var config = new GpuStackConfig();
        var rerankConfig = config.ToRerankConfig();
        rerankConfig.BaseUrl.Should().Be("http://localhost:8080/v1/");
    }

    [Fact]
    public void ToRerankConfig_CustomBaseUrl_AppendsSuffix()
    {
        var config = new GpuStackConfig { BaseUrl = "http://172.19.10.10:8080" };
        var rerankConfig = config.ToRerankConfig();
        rerankConfig.BaseUrl.Should().Be("http://172.19.10.10:8080/v1/");
    }

    [Fact]
    public void ToRerankConfig_ApiKey_IsPreserved()
    {
        var config = new GpuStackConfig { ApiKey = "my-key" };
        var rerankConfig = config.ToRerankConfig();
        rerankConfig.ApiKey.Should().Be("my-key");
    }

    [Fact]
    public void ToOpenAICompatible_DefaultBaseUrl_TargetsV1OpenaiPath()
    {
        var config = new GpuStackConfig();
        var compatible = config.ToOpenAICompatible();

        // OpenAICompatibleConfig.ToOpenAI() doesn't add a trailing slash (unlike
        // GpuStackConfig.ToOpenAI()) — both are normalized downstream via EnsureSuffix('/')
        // wherever the BaseUrl is actually dialed, so this is a difference without effect.
        compatible.ToOpenAI().BaseUrl.Should().Be("http://localhost:8080/v1-openai");
    }

    [Fact]
    public void ToOpenAICompatible_ApiKey_IsPreserved()
    {
        var config = new GpuStackConfig { ApiKey = "my-key" };
        config.ToOpenAICompatible().ApiKey.Should().Be("my-key");
    }

    [Fact]
    public void ToOpenAICompatible_TokenLimitParameter_IsPreserved()
    {
        var config = new GpuStackConfig { TokenLimitParameter = TokenLimitParameter.MaxTokens };
        config.ToOpenAICompatible().TokenLimitParameter.Should().Be(TokenLimitParameter.MaxTokens);
    }

    [Fact]
    public void ToOpenAICompatible_ConnectTimeout_IsPreserved()
    {
        var config = new GpuStackConfig { ConnectTimeout = TimeSpan.FromSeconds(7) };
        config.ToOpenAICompatible().ConnectTimeout.Should().Be(TimeSpan.FromSeconds(7));
    }

    [Fact]
    public void ToOpenAICompatible_BaseUrlResolver_StillEvaluatedLiveByTheConvertedConfig()
    {
        // The resolver delegate is carried over as-is, so a change after conversion is still
        // observed — dynamic rotation must not be frozen at conversion time.
        var current = "http://localhost:8080";
        var config = new GpuStackConfig { BaseUrlResolver = () => current };
        var compatible = config.ToOpenAICompatible();

        compatible.ToOpenAI().BaseUrl.Should().Be("http://localhost:8080/v1-openai");

        current = "http://localhost:9090";
        compatible.ToOpenAI().BaseUrl.Should().Be("http://localhost:9090/v1-openai");
    }

    [Fact]
    public void ToOpenAICompatible_ApiKeyResolver_StillEvaluatedLiveByTheConvertedConfig()
    {
        var current = "key-1";
        var config = new GpuStackConfig { ApiKeyResolver = () => current };
        var compatible = config.ToOpenAICompatible();

        compatible.ToOpenAI().ApiKey.Should().Be("key-1");

        current = "key-2";
        compatible.ToOpenAI().ApiKey.Should().Be("key-2");
    }
}
