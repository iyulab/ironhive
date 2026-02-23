using FluentAssertions;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Core.Agent.Orchestration;

namespace IronHive.Tests.Agent.Orchestration;

public class FileCheckpointStoreTests : IDisposable
{
    private readonly string _testDir;
    private readonly FileCheckpointStore _store;

    public FileCheckpointStoreTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "ironhive-tests", Guid.NewGuid().ToString("N"));
        _store = new FileCheckpointStore(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, recursive: true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task SaveAsync_CreatesDirectoryAndFile()
    {
        // Arrange
        var checkpoint = CreateCheckpoint("orch-1", "TestOrchestrator", 2);

        // Act
        await _store.SaveAsync("orch-1", checkpoint);

        // Assert
        Directory.Exists(_testDir).Should().BeTrue();
        File.Exists(Path.Combine(_testDir, "orch-1.json")).Should().BeTrue();
    }

    [Fact]
    public async Task SaveAndLoad_RoundTrips()
    {
        // Arrange
        var checkpoint = CreateCheckpoint("orch-2", "Sequential", 3);

        // Act
        await _store.SaveAsync("orch-2", checkpoint);
        var loaded = await _store.LoadAsync("orch-2");

        // Assert
        loaded.Should().NotBeNull();
        loaded!.OrchestrationId.Should().Be("orch-2");
        loaded.OrchestratorName.Should().Be("Sequential");
        loaded.CompletedStepCount.Should().Be(3);
        loaded.CompletedSteps.Should().HaveCount(1);
        loaded.CompletedSteps[0].AgentName.Should().Be("agent-1");
        loaded.CompletedSteps[0].IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAndLoad_PreservesMessages()
    {
        // Arrange
        var checkpoint = CreateCheckpointWithMessages("orch-msg");

        // Act
        await _store.SaveAsync("orch-msg", checkpoint);
        var loaded = await _store.LoadAsync("orch-msg");

        // Assert
        loaded.Should().NotBeNull();
        loaded!.CurrentMessages.Should().HaveCount(2);

        var userMsg = loaded.CurrentMessages[0].Should().BeOfType<UserMessage>().Subject;
        userMsg.Content.Should().HaveCount(1);
        var textContent = userMsg.Content.First().Should().BeOfType<TextMessageContent>().Subject;
        textContent.Value.Should().Be("Hello");

        var assistantMsg = loaded.CurrentMessages[1].Should().BeOfType<AssistantMessage>().Subject;
        assistantMsg.Name.Should().Be("TestAgent");
    }

    [Fact]
    public async Task LoadAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _store.LoadAsync("nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_RemovesFile()
    {
        // Arrange
        var checkpoint = CreateCheckpoint("orch-del", "Test", 1);
        await _store.SaveAsync("orch-del", checkpoint);

        // Act
        await _store.DeleteAsync("orch-del");

        // Assert
        var loaded = await _store.LoadAsync("orch-del");
        loaded.Should().BeNull();
        File.Exists(Path.Combine(_testDir, "orch-del.json")).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_DoesNotThrow_WhenNotFound()
    {
        // Should not throw
        await _store.DeleteAsync("nonexistent");
    }

    [Fact]
    public async Task SaveAsync_OverwritesExisting()
    {
        // Arrange
        var v1 = CreateCheckpoint("orch-ow", "Sequential", 1);
        var v2 = CreateCheckpoint("orch-ow", "Sequential", 5);

        // Act
        await _store.SaveAsync("orch-ow", v1);
        await _store.SaveAsync("orch-ow", v2);
        var loaded = await _store.LoadAsync("orch-ow");

        // Assert
        loaded!.CompletedStepCount.Should().Be(5);
    }

    [Fact]
    public async Task SaveAndLoad_WorksWithUnsafeId()
    {
        // IDs with path separators should be safely handled
        var checkpoint = CreateCheckpoint("path/to/something", "Test", 1);

        await _store.SaveAsync("path/to/something", checkpoint);
        var loaded = await _store.LoadAsync("path/to/something");

        loaded.Should().NotBeNull();
        loaded!.OrchestrationId.Should().Be("path/to/something");
    }

    [Fact]
    public async Task SaveAndLoad_WorksWithLongId()
    {
        var longId = new string('a', 250);
        var checkpoint = CreateCheckpoint(longId, "Test", 1);

        await _store.SaveAsync(longId, checkpoint);
        var loaded = await _store.LoadAsync(longId);

        loaded.Should().NotBeNull();
        loaded!.OrchestrationId.Should().Be(longId);
    }

    [Fact]
    public async Task SaveAndLoad_PreservesTimestamp()
    {
        // Arrange
        var createdAt = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var checkpoint = new OrchestrationCheckpoint
        {
            OrchestrationId = "orch-ts",
            OrchestratorName = "Test",
            CompletedStepCount = 0,
            CreatedAt = createdAt
        };

        // Act
        await _store.SaveAsync("orch-ts", checkpoint);
        var loaded = await _store.LoadAsync("orch-ts");

        // Assert
        loaded!.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public async Task SaveAndLoad_PreservesStepResult()
    {
        // Arrange
        var step = new AgentStepResult
        {
            AgentName = "summarizer",
            IsSuccess = false,
            Error = "Rate limit exceeded",
            Duration = TimeSpan.FromSeconds(3.5),
            Input = [new UserMessage { Content = [new TextMessageContent { Value = "Summarize" }] }],
            Response = new MessageResponse
            {
                Id = "resp-1",
                DoneReason = MessageDoneReason.MaxTokens,
                Message = new AssistantMessage { Content = [new TextMessageContent { Value = "Partial..." }] },
                TokenUsage = new MessageTokenUsage { InputTokens = 100, OutputTokens = 50 }
            }
        };

        var checkpoint = new OrchestrationCheckpoint
        {
            OrchestrationId = "orch-step",
            OrchestratorName = "Sequential",
            CompletedStepCount = 1,
            CompletedSteps = [step]
        };

        // Act
        await _store.SaveAsync("orch-step", checkpoint);
        var loaded = await _store.LoadAsync("orch-step");

        // Assert
        loaded!.CompletedSteps.Should().HaveCount(1);
        var loadedStep = loaded.CompletedSteps[0];
        loadedStep.AgentName.Should().Be("summarizer");
        loadedStep.IsSuccess.Should().BeFalse();
        loadedStep.Error.Should().Be("Rate limit exceeded");
        loadedStep.Duration.Should().BeCloseTo(TimeSpan.FromSeconds(3.5), TimeSpan.FromMilliseconds(1));
        loadedStep.Response.Should().NotBeNull();
        loadedStep.Response!.DoneReason.Should().Be(MessageDoneReason.MaxTokens);
        loadedStep.Response.TokenUsage!.InputTokens.Should().Be(100);
        loadedStep.Response.TokenUsage.OutputTokens.Should().Be(50);
    }

    [Fact]
    public void Constructor_ThrowsOnNullOrEmpty()
    {
        Assert.Throws<ArgumentNullException>(() => new FileCheckpointStore(null!));
        Assert.Throws<ArgumentException>(() => new FileCheckpointStore(""));
        Assert.Throws<ArgumentException>(() => new FileCheckpointStore("   "));
    }

    private static OrchestrationCheckpoint CreateCheckpoint(string id, string name, int steps)
    {
        return new OrchestrationCheckpoint
        {
            OrchestrationId = id,
            OrchestratorName = name,
            CompletedStepCount = steps,
            CompletedSteps =
            [
                new AgentStepResult
                {
                    AgentName = "agent-1",
                    IsSuccess = true,
                    Duration = TimeSpan.FromSeconds(1.5)
                }
            ]
        };
    }

    private static OrchestrationCheckpoint CreateCheckpointWithMessages(string id)
    {
        return new OrchestrationCheckpoint
        {
            OrchestrationId = id,
            OrchestratorName = "Sequential",
            CompletedStepCount = 1,
            CurrentMessages =
            [
                new UserMessage
                {
                    Content = [new TextMessageContent { Value = "Hello" }]
                },
                new AssistantMessage
                {
                    Name = "TestAgent",
                    Content = [new TextMessageContent { Value = "Hi there!" }]
                }
            ]
        };
    }
}
