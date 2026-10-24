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
        openAI.BaseUrl.Should().Be("http://localhost:8080/v1/");
    }

    [Fact]
    public void ToOpenAI_CustomBaseUrl_AppendsSuffix()
    {
        var config = new GpuStackConfig { BaseUrl = "http://172.19.10.10:8080" };
        var openAI = config.ToOpenAI();
        openAI.BaseUrl.Should().Be("http://172.19.10.10:8080/v1/");
    }

    [Fact]
    public void ToOpenAI_BaseUrlWithTrailingSlash_DoesNotDoubleSuffix()
    {
        var config = new GpuStackConfig { BaseUrl = "http://172.19.10.10:8080/" };
        var openAI = config.ToOpenAI();
        openAI.BaseUrl.Should().Be("http://172.19.10.10:8080/v1/");
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
        openAI.BaseUrl.Should().Be("http://localhost:8080/v1/");
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
    public void ToRerankConfig_DefaultBaseUrl_TargetsV1()
    {
        // GPUStack serves its Jina/Cohere-shaped rerank endpoint at /v1/rerank — a different path
        // whatever Path the chat/embeddings/models surface is set to.
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
        compatible.ToOpenAI().BaseUrl.Should().Be("http://localhost:8080/v1");
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

        compatible.ToOpenAI().BaseUrl.Should().Be("http://localhost:8080/v1");

        current = "http://localhost:9090";
        compatible.ToOpenAI().BaseUrl.Should().Be("http://localhost:9090/v1");
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

    [Fact]
    public void ToOpenAICompatible_CarriesEverySettingAndTargetsTheOpenAIPath()
    {
        // The public path for a consumer that builds the generator itself (outside the registry):
        // it has to reproduce what AddGpuStackProviders gets, field for field, or the two GPUStack
        // routes silently diverge -- the exact duplication 0.23.0 removed the generator to end.
        Func<string> baseUrl = () => "http://10.0.0.5:8080";
        Func<string?> apiKey = () => "rotated";
        var config = new GpuStackConfig
        {
            BaseUrl = "http://172.19.10.10:8080",
            ApiKey = "static",
            BaseUrlResolver = baseUrl,
            ApiKeyResolver = apiKey,
            ConnectTimeout = TimeSpan.FromSeconds(7),
            TokenLimitParameter = TokenLimitParameter.MaxTokens,
        };

        var compatible = config.ToOpenAICompatible();

        compatible.Path.Should().Be("/v1/", "GPUStack documents its OpenAI-compatible surface at the standard /v1/");
        compatible.BaseUrl.Should().Be("http://10.0.0.5:8080", "the resolver's answer is baked in as the static fallback");
        compatible.ApiKey.Should().Be("rotated");
        compatible.BaseUrlResolver.Should().BeSameAs(baseUrl, "rotation keeps working because the delegate is reused, not re-wrapped");
        compatible.ApiKeyResolver.Should().BeSameAs(apiKey);
        compatible.ConnectTimeout.Should().Be(TimeSpan.FromSeconds(7));
        compatible.TokenLimitParameter.Should().Be(TokenLimitParameter.MaxTokens);
    }

    [Theory]
    [InlineData("/v1-openai/", "http://gpu:8080/v1-openai/")]
    [InlineData("v1-openai", "http://gpu:8080/v1-openai/")]
    [InlineData("/v1-openai", "http://gpu:8080/v1-openai/")]
    [InlineData("", "http://gpu:8080/")]
    public void Path_Override_IsNormalizedForBothConversions(string path, string expected)
    {
        // A GPUStack 0.x server answers only at /v1-openai/; an empty path serves a gateway mounted at the root.
        var config = new GpuStackConfig { BaseUrl = "http://gpu:8080", Path = path };

        config.ToOpenAI().BaseUrl.Should().Be(expected);
        config.ToOpenAICompatible().ToOpenAI().BaseUrl.TrimEnd('/').Should().Be(expected.TrimEnd('/'));
    }

    [Fact]
    public void Path_Override_DoesNotMoveRerank()
    {
        var config = new GpuStackConfig { BaseUrl = "http://gpu:8080", Path = "/v1-openai/" };
        config.ToRerankConfig().BaseUrl.Should().Be("http://gpu:8080/v1/");
    }

    [Fact]
    public void EverySettingOfTheCompatibleConfig_ExistsOnTheGpuStackConfig()
    {
        // GPUStack is an OpenAI-compatible route with its own config class; a setting added to OpenAICompatibleConfig
        // but not here cannot be turned on for GPUStack (0.70.0 shipped the instructions placement that way).
        var gpuStack = typeof(GpuStackConfig).GetProperties().Where(p => p.CanWrite).Select(p => p.Name).ToHashSet();

        typeof(OpenAICompatibleConfig).GetProperties().Where(p => p.CanWrite).Select(p => p.Name)
            .Where(name => !gpuStack.Contains(name))
            .Should().BeEmpty("each OpenAICompatibleConfig setting needs its GpuStackConfig twin, carried by ToOpenAICompatible()");
    }

    [Fact]
    public void ToOpenAICompatible_CarriesTheInstructionsPlacement()
    {
        new GpuStackConfig { InstructionsPlacement = InstructionsPlacement.LeadingUserTurn }.ToOpenAICompatible()
            .InstructionsPlacement.Should().Be(InstructionsPlacement.LeadingUserTurn);
    }
}
