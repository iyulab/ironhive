using FluentAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Core;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace IronHive.Tests.Core;

/// <summary>
/// Tests for HiveService agent creation methods.
/// P0-1: HiveService.CreateAgentFrom implementation tests.
/// </summary>
public class HiveServiceAgentCreationTests
{
    private readonly HiveService _service;
    private readonly IAgentService _mockAgentService;

    public HiveServiceAgentCreationTests()
    {
        var builder = new HiveServiceBuilder();

        // Replace with mock
        _mockAgentService = Substitute.For<IAgentService>();
        builder.Services.AddSingleton(_mockAgentService);

        _service = (HiveService)builder.Build();
    }

    [Fact]
    public void CreateAgentFrom_ShouldCallAgentServiceWithWorkflow()
    {
        // Arrange
        var expectedYaml = "agent:\n  name: Test\n  provider: openai\n  model: gpt-4o";
        var card = new AgentCard
        {
            Name = "TestAgent",
            Workflow = expectedYaml
        };

        var mockAgent = Substitute.For<IAgent>();
        _mockAgentService
            .CreateAgentFromYaml(expectedYaml)
            .Returns(mockAgent);

        // Act
        var result = _service.CreateAgentFrom(card);

        // Assert
        result.Should().NotBeNull();
        _mockAgentService.Received(1).CreateAgentFromYaml(expectedYaml);
    }

    [Fact]
    public void CreateAgentFrom_ShouldThrow_WhenCardIsNull()
    {
        // Act
        var act = () => _service.CreateAgentFrom(null!);

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

    [Fact]
    public void CreateAgentFromYaml_ShouldDelegateToAgentService()
    {
        // Arrange
        var yaml = "agent:\n  name: Test\n  provider: openai\n  model: gpt-4o";
        var mockAgent = Substitute.For<IAgent>();
        _mockAgentService
            .CreateAgentFromYaml(yaml)
            .Returns(mockAgent);

        // Act
        var result = _service.CreateAgentFromYaml(yaml);

        // Assert
        result.Should().Be(mockAgent);
        _mockAgentService.Received(1).CreateAgentFromYaml(yaml);
    }
}
