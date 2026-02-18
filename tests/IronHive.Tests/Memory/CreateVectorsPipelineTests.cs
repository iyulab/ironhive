using FluentAssertions;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Vector;
using IronHive.Core.Memory.Pipelines;
using NSubstitute;

namespace IronHive.Tests.Memory;

public class CreateVectorsPipelineTests
{
    private static readonly float[] s_vec1 = [0.1f, 0.2f];
    private static readonly float[] s_vec2 = [0.3f, 0.4f];
    private static readonly float[] s_vec3 = [1.0f, 2.0f, 3.0f];
    private static readonly string[] s_twoTexts = ["hello world", "foo bar"];
    private static readonly string[] s_oneText = ["my text"];

    private readonly IEmbeddingService _mockEmbedder;
    private readonly CreateVectorsPipeline _pipeline;

    public CreateVectorsPipelineTests()
    {
        _mockEmbedder = Substitute.For<IEmbeddingService>();
        _pipeline = new CreateVectorsPipeline(_mockEmbedder);
    }

    #region Text Chunks

    [Fact]
    public async Task ExecuteAsync_TextChunks_CreatesVectorRecords()
    {
        var context = CreateContext(s_twoTexts);
        SetupEmbeddings(s_vec1, s_vec2);

        var result = await _pipeline.ExecuteAsync(context);

        result.IsError.Should().BeFalse();
        var vectors = context.Payload["vectors"] as List<VectorRecord>;
        vectors.Should().NotBeNull();
        vectors.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_TextChunks_SetsTextPayload()
    {
        var context = CreateContext(s_oneText);
        SetupEmbeddings(s_vec1);

        await _pipeline.ExecuteAsync(context);

        var vectors = context.Payload["vectors"] as List<VectorRecord>;
        vectors![0].Payload.Should().ContainKey("text");
        vectors[0].Payload!["text"].Should().Be("my text");
    }

    [Fact]
    public async Task ExecuteAsync_TextChunks_SetsSourceId()
    {
        var context = CreateContext(s_oneText);
        SetupEmbeddings(s_vec1);

        await _pipeline.ExecuteAsync(context);

        var vectors = context.Payload["vectors"] as List<VectorRecord>;
        vectors![0].SourceId.Should().Be("test-source");
    }

    [Fact]
    public async Task ExecuteAsync_TextChunks_SetsVectorValues()
    {
        var context = CreateContext(s_oneText);
        SetupEmbeddings(s_vec3);

        await _pipeline.ExecuteAsync(context);

        var vectors = context.Payload["vectors"] as List<VectorRecord>;
        vectors![0].Vectors.Should().BeEquivalentTo(s_vec3);
    }

    [Fact]
    public async Task ExecuteAsync_TextChunks_PassesProviderAndModel()
    {
        var context = CreateContext(s_oneText);
        SetupEmbeddings(s_vec1);

        await _pipeline.ExecuteAsync(context);

        await _mockEmbedder.Received(1).EmbedBatchAsync(
            "test-provider",
            "test-model",
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_TextChunks_NullEmbeddings_Throws()
    {
        var context = CreateContext(s_oneText);
        _mockEmbedder.EmbedBatchAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns((IEnumerable<EmbeddingResult>)null!);

        var act = () => _pipeline.ExecuteAsync(context);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*embeddings*");
    }

    [Fact]
    public async Task ExecuteAsync_TextChunks_CountMismatch_Throws()
    {
        var context = CreateContext(s_twoTexts);
        // Return only 1 embedding for 2 inputs
        SetupEmbeddings(s_vec1);

        var act = () => _pipeline.ExecuteAsync(context);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*embeddings*");
    }

    #endregion

    #region Dialogue Chunks

    [Fact]
    public async Task ExecuteAsync_Dialogues_CreatesVectorRecords()
    {
        var dialogues = new List<DialogueExtractionPipeline.Dialogue>
        {
            new("What is AI?", "AI is artificial intelligence."),
            new("What is ML?", "ML is machine learning.")
        };
        var context = CreateContext(dialogues);
        SetupEmbeddings(s_vec1, s_vec2);

        var result = await _pipeline.ExecuteAsync(context);

        result.IsError.Should().BeFalse();
        var vectors = context.Payload["vectors"] as List<VectorRecord>;
        vectors.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_Dialogues_SetsQuestionAndAnswerPayload()
    {
        var dialogues = new List<DialogueExtractionPipeline.Dialogue>
        {
            new("What is AI?", "AI is artificial intelligence.")
        };
        var context = CreateContext(dialogues);
        SetupEmbeddings(s_vec1);

        await _pipeline.ExecuteAsync(context);

        var vectors = context.Payload["vectors"] as List<VectorRecord>;
        vectors![0].Payload.Should().ContainKey("question");
        vectors[0].Payload!["question"].Should().Be("What is AI?");
        vectors[0].Payload.Should().ContainKey("answer");
        vectors[0].Payload!["answer"].Should().Be("AI is artificial intelligence.");
    }

    [Fact]
    public async Task ExecuteAsync_Dialogues_EmbedsQuestions()
    {
        var dialogues = new List<DialogueExtractionPipeline.Dialogue>
        {
            new("Q1?", "A1."),
            new("Q2?", "A2.")
        };
        var context = CreateContext(dialogues);
        SetupEmbeddings(s_vec1, s_vec2);

        await _pipeline.ExecuteAsync(context);

        await _mockEmbedder.Received(1).EmbedBatchAsync(
            "test-provider", "test-model",
            Arg.Is<IEnumerable<string>>(x => x.Contains("Q1?") && x.Contains("Q2?")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Dialogues_NullEmbeddings_Throws()
    {
        var dialogues = new List<DialogueExtractionPipeline.Dialogue>
        {
            new("Q?", "A.")
        };
        var context = CreateContext(dialogues);
        _mockEmbedder.EmbedBatchAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns((IEnumerable<EmbeddingResult>)null!);

        var act = () => _pipeline.ExecuteAsync(context);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*embeddings*dialogues*");
    }

    #endregion

    #region Error Handling

    [Fact]
    public async Task ExecuteAsync_NonVectorTarget_Throws()
    {
        var context = new MemoryContext
        {
            Source = Substitute.For<IMemorySource>(),
            Target = Substitute.For<IMemoryTarget>()
        };
        context.Payload["chunks"] = s_oneText;

        var act = () => _pipeline.ExecuteAsync(context);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*target*");
    }

    [Fact]
    public async Task ExecuteAsync_MissingChunks_Throws()
    {
        var context = new MemoryContext
        {
            Source = Substitute.For<IMemorySource>(),
            Target = new VectorMemoryTarget
            {
                StorageName = "s", CollectionName = "c",
                EmbeddingProvider = "p", EmbeddingModel = "m"
            }
        };

        var act = () => _pipeline.ExecuteAsync(context);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*chunks*");
    }

    [Fact]
    public async Task ExecuteAsync_WrongChunksType_ReturnsFail()
    {
        var context = CreateContextRaw(42); // not IEnumerable<string> or Dialogue

        var result = await _pipeline.ExecuteAsync(context);

        result.IsError.Should().BeTrue();
        result.Exception.Should().BeOfType<InvalidOperationException>();
    }

    #endregion

    #region CancellationToken

    [Fact]
    public async Task ExecuteAsync_PassesCancellationToken()
    {
        var context = CreateContext(s_oneText);
        SetupEmbeddings(s_vec1);
        using var cts = new CancellationTokenSource();

        await _pipeline.ExecuteAsync(context, cts.Token);

        await _mockEmbedder.Received(1).EmbedBatchAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            cts.Token);
    }

    #endregion

    #region Helpers

    private static MemoryContext CreateContext(IEnumerable<string> chunks)
    {
        var context = CreateBaseContext();
        context.Payload["chunks"] = chunks;
        return context;
    }

    private static MemoryContext CreateContext(IEnumerable<DialogueExtractionPipeline.Dialogue> dialogues)
    {
        var context = CreateBaseContext();
        context.Payload["chunks"] = dialogues;
        return context;
    }

    private static MemoryContext CreateContextRaw(object chunks)
    {
        var context = CreateBaseContext();
        context.Payload["chunks"] = chunks;
        return context;
    }

    private static MemoryContext CreateBaseContext()
    {
        return new MemoryContext
        {
            Source = new TextMemorySource { Id = "test-source", Value = "text" },
            Target = new VectorMemoryTarget
            {
                StorageName = "test-storage",
                CollectionName = "test-collection",
                EmbeddingProvider = "test-provider",
                EmbeddingModel = "test-model"
            }
        };
    }

    private void SetupEmbeddings(params float[][] vectors)
    {
        var results = vectors.Select((v, i) => new EmbeddingResult
        {
            Index = i,
            Embedding = v
        });
        _mockEmbedder.EmbedBatchAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(results);
    }

    #endregion
}
