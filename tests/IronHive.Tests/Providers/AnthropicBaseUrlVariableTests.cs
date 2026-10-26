using Anthropic;
using AwesomeAssertions;
using IronHive.Providers.Anthropic;

namespace IronHive.Tests.Providers;

/// <summary>Collection for tests that change process-wide environment variables: nothing else runs beside them.</summary>
[CollectionDefinition(nameof(ProcessEnvironment), DisableParallelization = true)]
public sealed class ProcessEnvironment;

/// <summary>
/// The vendor SDK reads <c>ANTHROPIC_BASE_URL</c> itself. A variable that exists but is blank must not become the base
/// URL (every request would throw <see cref="UriFormatException"/>), and one naming a gateway must keep working.
/// </summary>
[Collection(nameof(ProcessEnvironment))]
public class AnthropicBaseUrlVariableTests
{
    private const string Gateway = "https://gateway.example/v1";

    private static AnthropicClient Create(AnthropicConfig config)
        => (AnthropicClient)AnthropicClientFactory.Create(config);

    [Theory]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithoutABaseUrl_UsesTheApiRoot_WhetherTheVariableIsUnsetOrBlank(string? variable)
    {
        var before = Environment.GetEnvironmentVariable(AnthropicClientFactory.BaseUrlVariable);
        try
        {
            Environment.SetEnvironmentVariable(AnthropicClientFactory.BaseUrlVariable, variable);

            Create(new AnthropicConfig { ApiKey = "sk-test" }).BaseUrl.Should().Be("https://api.anthropic.com");
        }
        finally
        {
            Environment.SetEnvironmentVariable(AnthropicClientFactory.BaseUrlVariable, before);
        }
    }

    [Fact]
    public void Create_WithAVariableThatNamesAGateway_KeepsTheSdkReadingIt()
    {
        var before = Environment.GetEnvironmentVariable(AnthropicClientFactory.BaseUrlVariable);
        try
        {
            Environment.SetEnvironmentVariable(AnthropicClientFactory.BaseUrlVariable, Gateway);

            Create(new AnthropicConfig { ApiKey = "sk-test" }).BaseUrl.Should().Be(Gateway);
        }
        finally
        {
            Environment.SetEnvironmentVariable(AnthropicClientFactory.BaseUrlVariable, before);
        }
    }
}
