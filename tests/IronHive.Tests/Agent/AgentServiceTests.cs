using FluentAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Core.Agent;
using Moq;

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
        var mockMessageService = new Mock<IMessageService>();
        _service = new AgentService(mockMessageService.Object);
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
  tools:
    - web-search
    - calculator
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
        agent.Tools.Should().HaveCount(2);
        agent.Parameters.Should().NotBeNull();
        agent.Parameters!.MaxTokens.Should().Be(512);
        agent.Parameters!.Temperature.Should().Be(0.7f);
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
    public void CreateAgentFromYaml_ShouldThrow_WhenNameMissing()
    {
        // Arrange
        var yaml = @"
agent:
  provider: openai
  model: gpt-4o
";

        // Act
        var act = () => _service.CreateAgentFromYaml(yaml);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*name*");
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
                ""tools"": [""search""],
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
        agent.Tools.Should().HaveCount(1);
        agent.Parameters!.MaxTokens.Should().Be(1000);
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
    public void CreateAgentFromJson_ShouldThrow_WhenProviderMissing()
    {
        // Arrange
        var json = @"{
            ""name"": ""NoProviderBot"",
            ""model"": ""gpt-4o""
        }";

        // Act
        var act = () => _service.CreateAgentFromJson(json);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*provider*");
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
tools = [""calculator""]

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
        agent.Tools.Should().HaveCount(1);
        agent.Parameters!.MaxTokens.Should().Be(256);
        agent.Parameters!.Temperature.Should().Be(0.3f);
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
}
