using FluentAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Core;

namespace IronHive.Tests.Core;

/// <summary>
/// Tests for HiveService agent creation methods.
/// </summary>
public class HiveServiceAgentCreationTests
{
    private readonly HiveService _service;

    public HiveServiceAgentCreationTests()
    {
        var builder = new HiveServiceBuilder();
        _service = (HiveService)builder.Build();
    }

    [Fact]
    public void CreateAgentFrom_ShouldThrow_WhenCardIsNull()
    {
        // Act
        var act = () => _service.CreateAgentFrom((AgentCard)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("card");
    }

    [Fact]
    public void CreateAgentFrom_ShouldThrow_WhenWorkflowIsEmpty()
    {
        // Arrange
        var card = new AgentCard
        {
            Name = "TestAgent",
            Workflow = ""
        };

        // Act
        var act = () => _service.CreateAgentFrom(card);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Workflow*");
    }

    [Fact]
    public void CreateAgentFrom_ShouldThrow_WhenWorkflowIsWhitespace()
    {
        // Arrange
        var card = new AgentCard
        {
            Name = "TestAgent",
            Workflow = "   "
        };

        // Act
        var act = () => _service.CreateAgentFrom(card);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Workflow*");
    }
}
