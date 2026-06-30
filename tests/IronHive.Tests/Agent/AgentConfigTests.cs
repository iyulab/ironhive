using FluentAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Core.Agent;

namespace IronHive.Tests.Agent;

public class AgentConfigTests
{
    [Fact]
    public void Validate_ValidConfig_ShouldNotThrow()
    {
        var config = new AgentConfig
        {
            Provider = "openai",
            Model = "gpt-4o"
        };

        var act = () => config.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_ModelOnly_ShouldNotThrow()
    {
        var config = new AgentConfig
        {
            Provider = "openai",
            Model = "gpt-4o"
            // Name omitted — default empty string, should not throw
        };

        var act = () => config.Validate();

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_MissingModel_ShouldThrow(string? model)
    {
        var config = new AgentConfig
        {
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
    public void Parameters_MaxTokens_NullConfig_ShouldBeNull()
    {
        var config = new AgentConfig { Parameters = null };

        config.Parameters?.MaxTokens.Should().BeNull();
    }

    [Fact]
    public void Parameters_MaxTokens_ShouldConvert()
    {
        var config = new AgentConfig
        {
            Parameters = new AgentParametersConfig
            {
                MaxTokens = 1000,
            }
        };

        config.Parameters!.MaxTokens.Should().Be(1000);
    }
}
