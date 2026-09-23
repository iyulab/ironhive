using AwesomeAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using IronHive.Core.Agent;
using NSubstitute;

namespace IronHive.Tests.Agent;

/// <summary>
/// Tests for AgentService creation methods.
/// Issue #1: Agent creation methods (CreateAgentFromYaml, CreateAgentFromJson, CreateAgentFromToml).
/// </summary>
public class AgentServiceTests
{
    private readonly AgentService _service;

    public AgentServiceTests()
    {
        var mockMessageService = Substitute.For<IMessageService>();
        _service = new AgentService(mockMessageService);
    }

    #region YAML Tests

    [Fact]
    public void CreateAgentFromYaml_ShouldCreateAgent_WithRootWrapper()
    {
        // Arrange
        var yaml = @"
agent:
  name: TestBot
  description: A test agent
  provider: openai
  model: gpt-4o-mini
  instructions: You are a helpful assistant.
  parameters:
    maxTokens: 512
    temperature: 0.7
";

        // Act
        var agent = _service.CreateAgentFromYaml(yaml);

        // Assert
        agent.Should().NotBeNull();
        agent.Name.Should().Be("TestBot");
        agent.Description.Should().Be("A test agent");
        agent.Provider.Should().Be("openai");
        agent.Model.Should().Be("gpt-4o-mini");
        agent.Instructions.Should().Be("You are a helpful assistant.");
        agent.MaxTokens.Should().Be(512);
    }

    [Fact]
    public void CreateAgentFromYaml_WithTools_ShouldThrowNotSupportedException()
    {
        // A config file has no name-to-ITool registry behind it (AgentConfigKeys). A declared tool
        // must fail loud at load, not silently never execute.
        var yaml = @"
agent:
  name: TestBot
  provider: openai
  model: gpt-4o-mini
  tools:
    - web-search
";

        var act = () => _service.CreateAgentFromYaml(yaml);

        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void CreateAgentFromYaml_ShouldCreateAgent_WithDirectConfig()
    {
        // Arrange
        var yaml = @"
name: DirectBot
description: Direct config test
provider: anthropic
model: claude-3-sonnet
";

        // Act
        var agent = _service.CreateAgentFromYaml(yaml);

        // Assert
        agent.Should().NotBeNull();
        agent.Name.Should().Be("DirectBot");
        agent.Provider.Should().Be("anthropic");
    }

    [Fact]
    public void CreateAgentFromYaml_ShouldSucceed_WhenNameMissing()
    {
        // Name is optional — agent can be created without it
        var yaml = @"
agent:
  provider: openai
  model: gpt-4o
";

        // Act
        var agent = _service.CreateAgentFromYaml(yaml);

        // Assert
        agent.Should().NotBeNull();
        agent.Provider.Should().Be("openai");
        agent.Model.Should().Be("gpt-4o");
        agent.Name.Should().BeEmpty();
    }

    [Fact]
    public void CreateAgentFromYaml_ShouldThrow_WhenYamlEmpty()
    {
        // Act
        var act = () => _service.CreateAgentFromYaml("");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region JSON Tests

    [Fact]
    public void CreateAgentFromJson_ShouldCreateAgent_WithRootWrapper()
    {
        // Arrange
        var json = @"{
            ""agent"": {
                ""name"": ""JsonBot"",
                ""description"": ""A JSON agent"",
                ""provider"": ""openai"",
                ""model"": ""gpt-4o-mini"",
                ""instructions"": ""Be helpful."",
                ""parameters"": {
                    ""maxTokens"": 1000,
                    ""temperature"": 0.5
                }
            }
        }";

        // Act
        var agent = _service.CreateAgentFromJson(json);

        // Assert
        agent.Should().NotBeNull();
        agent.Name.Should().Be("JsonBot");
        agent.Provider.Should().Be("openai");
        agent.Model.Should().Be("gpt-4o-mini");
        agent.MaxTokens.Should().Be(1000);
    }

    [Fact]
    public void CreateAgentFromJson_WithTools_ShouldThrowNotSupportedException()
    {
        var json = @"{
            ""agent"": {
                ""name"": ""JsonBot"",
                ""provider"": ""openai"",
                ""model"": ""gpt-4o-mini"",
                ""tools"": [""search""]
            }
        }";

        var act = () => _service.CreateAgentFromJson(json);

        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void CreateAgentFromJson_ShouldCreateAgent_WithDirectConfig()
    {
        // Arrange
        var json = @"{
            ""name"": ""DirectJsonBot"",
            ""description"": ""Direct JSON"",
            ""provider"": ""anthropic"",
            ""model"": ""claude-3-haiku""
        }";

        // Act
        var agent = _service.CreateAgentFromJson(json);

        // Assert
        agent.Should().NotBeNull();
        agent.Name.Should().Be("DirectJsonBot");
        agent.Provider.Should().Be("anthropic");
    }

    [Fact]
    public void CreateAgentFromJson_ShouldSucceed_WhenNameMissing()
    {
        // Arrange
        var json = @"{""agent"":{""provider"":""openai"",""model"":""gpt-4o""}}";

        // Act
        var agent = _service.CreateAgentFromJson(json);

        // Assert
        agent.Provider.Should().Be("openai");
        agent.Model.Should().Be("gpt-4o");
        agent.Name.Should().BeEmpty();
    }

    #endregion

    #region TOML Tests

    [Fact]
    public void CreateAgentFromToml_ShouldCreateAgent_WithRootWrapper()
    {
        // Arrange
        var toml = @"
[agent]
name = ""TomlBot""
description = ""A TOML agent""
provider = ""openai""
model = ""gpt-4o-mini""
instructions = ""Be concise.""

[agent.parameters]
maxTokens = 256
temperature = 0.3
";

        // Act
        var agent = _service.CreateAgentFromToml(toml);

        // Assert
        agent.Should().NotBeNull();
        agent.Name.Should().Be("TomlBot");
        agent.Provider.Should().Be("openai");
        agent.Model.Should().Be("gpt-4o-mini");
        agent.Instructions.Should().Be("Be concise.");
        agent.MaxTokens.Should().Be(256);
    }

    [Fact]
    public void CreateAgentFromToml_WithTools_ShouldThrowNotSupportedException()
    {
        var toml = @"
[agent]
name = ""TomlBot""
provider = ""openai""
model = ""gpt-4o-mini""
tools = [""calculator""]
";

        var act = () => _service.CreateAgentFromToml(toml);

        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void CreateAgentFromToml_ShouldCreateAgent_WithDirectConfig()
    {
        // Arrange
        var toml = @"
name = ""DirectTomlBot""
description = ""Direct TOML""
provider = ""ollama""
model = ""llama3""
";

        // Act
        var agent = _service.CreateAgentFromToml(toml);

        // Assert
        agent.Should().NotBeNull();
        agent.Name.Should().Be("DirectTomlBot");
        agent.Provider.Should().Be("ollama");
    }

    [Fact]
    public void CreateAgentFromToml_ShouldSupport_DefaultProviderAlias()
    {
        // Arrange - using defaultProvider/defaultModel as in the interface docs
        var toml = @"
[agent]
name = ""AliasBot""
description = ""Test alias""
defaultProvider = ""openai""
defaultModel = ""gpt-4o""
";

        // Act
        var agent = _service.CreateAgentFromToml(toml);

        // Assert
        agent.Should().NotBeNull();
        agent.Provider.Should().Be("openai");
        agent.Model.Should().Be("gpt-4o");
    }

    [Fact]
    public void CreateAgentFromToml_ShouldThrow_WhenModelMissing()
    {
        // Arrange
        var toml = @"
name = ""NoModelBot""
provider = ""openai""
";

        // Act
        var act = () => _service.CreateAgentFromToml(toml);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*model*");
    }

    #endregion

    [Theory]
    [InlineData("yaml", "agent:\n  model: gpt-4o-mini\n  maxToken: 100\n", "maxToken")]
    [InlineData("yaml", "model: gpt-4o-mini\nparameters:\n  temprature: 0.2\n", "parameters.temprature")]
    [InlineData("yaml", "agent:\n  model: gpt-4o-mini\nextra: 1\n", "extra")]
    [InlineData("json", "{\"model\":\"gpt-4o-mini\",\"instruction\":\"x\"}", "instruction")]
    [InlineData("json", "{\"agent\":{\"model\":\"gpt-4o-mini\",\"parameters\":{\"top_p\":1}}}", "parameters.top_p")]
    [InlineData("toml", "[agent]\nmodel = \"gpt-4o-mini\"\nprovder = \"openai\"\n", "provder")]
    public void CreateAgentFrom_UnknownKey_ShouldThrowNamingTheKey(string format, string text, string key)
    {
        // The deserializers drop unknown keys; before this check a typo produced an agent that
        // silently ran without the setting.
        var act = () => Create(format, text);

        act.Should().Throw<ArgumentException>().WithMessage($"*'{key}'*");
    }

    [Theory]
    [InlineData("yaml", "model: gpt-4o-mini\ntoolOptions:\n  web-search:\n    maxResults: 5\n")]
    [InlineData("json", "{\"Model\":\"gpt-4o-mini\",\"ToolOptions\":{}}")]
    [InlineData("toml", "model = \"gpt-4o-mini\"\n[toolOptions.web-search]\nmaxResults = 5\n")]
    public void CreateAgentFrom_ToolOptionsKey_ShouldThrowNotSupportedException(string format, string text)
    {
        var act = () => Create(format, text);

        act.Should().Throw<NotSupportedException>().WithMessage("*IAgent.Tools*");
    }

    [Theory]
    [InlineData("json", "{\"MODEL\":\"gpt-4o-mini\",\"Parameters\":{\"MaxTokens\":64}}")]
    [InlineData("toml", "[agent]\ndefaultModel = \"gpt-4o-mini\"\n[agent.parameters]\nstopSequences = [\"x\"]\n")]
    [InlineData("yaml", "agent:\n  model: gpt-4o-mini\n  parameters:\n    topK: 5\n")]
    public void CreateAgentFrom_EveryKnownKeySpelling_ShouldLoad(string format, string text)
    {
        var agent = Create(format, text);

        agent.Model.Should().Be("gpt-4o-mini");
    }

    private IAgent Create(string format, string text) => format switch
    {
        "yaml" => _service.CreateAgentFromYaml(text),
        "json" => _service.CreateAgentFromJson(text),
        "toml" => _service.CreateAgentFromToml(text),
        _ => throw new ArgumentOutOfRangeException(nameof(format)),
    };
}
