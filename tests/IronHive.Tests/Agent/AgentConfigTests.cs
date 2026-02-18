using FluentAssertions;
using IronHive.Abstractions.Agent;

namespace IronHive.Tests.Agent;

public class AgentConfigTests
{
    [Fact]
    public void Validate_ValidConfig_ShouldNotThrow()
    {
        var config = new AgentConfig
        {
            Name = "test-agent",
            Provider = "openai",
            Model = "gpt-4o"
        };

        var act = () => config.Validate();

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_MissingName_ShouldThrow(string? name)
    {
        var config = new AgentConfig
        {
            Name = name!,
            Provider = "openai",
            Model = "gpt-4o"
        };

        var act = () => config.Validate();

        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("Name");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_MissingProvider_ShouldThrow(string? provider)
    {
        var config = new AgentConfig
        {
            Name = "agent",
            Provider = provider!,
            Model = "gpt-4o"
        };

        var act = () => config.Validate();

        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("Provider");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_MissingModel_ShouldThrow(string? model)
    {
        var config = new AgentConfig
        {
            Name = "agent",
            Provider = "openai",
            Model = model!
        };

        var act = () => config.Validate();

        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("Model");
    }

    [Fact]
    public void ToToolItems_NullTools_ShouldReturnNull()
    {
        var config = new AgentConfig { Tools = null };

        config.ToToolItems().Should().BeNull();
    }

    [Fact]
    public void ToToolItems_EmptyTools_ShouldReturnNull()
    {
        var config = new AgentConfig { Tools = [] };

        config.ToToolItems().Should().BeNull();
    }

    [Fact]
    public void ToToolItems_WithTools_ShouldConvert()
    {
        var config = new AgentConfig
        {
            Tools = ["tool1", "tool2"]
        };

        var items = config.ToToolItems()!.ToList();

        items.Should().HaveCount(2);
        items[0].Name.Should().Be("tool1");
        items[1].Name.Should().Be("tool2");
    }

    [Fact]
    public void ToToolItems_WithToolOptions_ShouldMapOptions()
    {
        var config = new AgentConfig
        {
            Tools = ["tool1", "tool2"],
            ToolOptions = new Dictionary<string, object?>
            {
                ["tool1"] = "opt-value"
            }
        };

        var items = config.ToToolItems()!.ToList();

        items[0].Options.Should().Be("opt-value");
        items[1].Options.Should().BeNull();
    }

    [Fact]
    public void ToParameters_NullParameters_ShouldReturnNull()
    {
        var config = new AgentConfig { Parameters = null };

        config.ToParameters().Should().BeNull();
    }

    [Fact]
    public void ToParameters_WithParameters_ShouldConvert()
    {
        var config = new AgentConfig
        {
            Parameters = new AgentParametersConfig
            {
                MaxTokens = 1000,
                Temperature = 0.7f,
                TopP = 0.9f,
                TopK = 50,
                StopSequences = ["END", "STOP"]
            }
        };

        var result = config.ToParameters()!;

        result.MaxTokens.Should().Be(1000);
        result.Temperature.Should().Be(0.7f);
        result.TopP.Should().Be(0.9f);
        result.TopK.Should().Be(50);
        result.StopSequences.Should().HaveCount(2);
    }

    [Fact]
    public void ToParameters_WithPartialParameters_ShouldHaveNulls()
    {
        var config = new AgentConfig
        {
            Parameters = new AgentParametersConfig
            {
                MaxTokens = 500
            }
        };

        var result = config.ToParameters()!;

        result.MaxTokens.Should().Be(500);
        result.Temperature.Should().BeNull();
        result.TopP.Should().BeNull();
        result.TopK.Should().BeNull();
        result.StopSequences.Should().BeNull();
    }
}
