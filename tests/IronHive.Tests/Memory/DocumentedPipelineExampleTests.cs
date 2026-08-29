using AwesomeAssertions;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Workflow;

namespace IronHive.Tests.Memory;

/// <summary>
/// The custom-pipeline example in <c>docs/MEMORY.md</c>, compiled. It previously described a signature
/// the interface never had — <c>Task&lt;MemoryContext&gt;</c> returning the context — and reached for
/// <c>context.Text</c>, a property that does not exist. Prose cannot notice that; a compiler can, so the
/// example lives here and the document quotes what is verified rather than the reverse.
/// </summary>
public class DocumentedPipelineExampleTests
{
    // --- docs/MEMORY.md § 커스텀 파이프라인 --------------------------------------------------

    private sealed class MyFilterPipeline : IMemoryPipeline
    {
        public Task<TaskStepResult> ExecuteAsync(MemoryContext context, CancellationToken ct = default)
        {
            if (context.Payload.TryGetValue("text", out var value) && value is string text)
                context.Payload["text"] = text.ToUpperInvariant();

            return Task.FromResult(TaskStepResult.Success());
        }
    }

    private sealed class MyPipelineOptions { public int MaxLength { get; set; } = 1000; }

    private sealed class MyOptionsPipeline : IMemoryPipeline<MyPipelineOptions>
    {
        public Task<TaskStepResult> ExecuteAsync(
            MemoryContext context, MyPipelineOptions options, CancellationToken ct = default)
        {
            _ = options.MaxLength;
            return Task.FromResult(TaskStepResult.Success());
        }
    }

    // --- docs/MEMORY.md § Source / Target --------------------------------------------------

    private static MemoryContext Context() => new()
    {
        Source = new FileMemorySource { StorageName = "local", FilePath = "notes.md" },
        Target = new VectorMemoryTarget
        {
            StorageName = "qdrant",
            CollectionName = "notes",
            EmbeddingProvider = "openai",
            EmbeddingModel = "text-embedding-3-small",
        },
    };

    [Fact]
    public async Task TheDocumentedFilterPipeline_ReadsAndWritesThePayloadKey()
    {
        var context = Context();
        context.Payload["text"] = "hello";

        var result = await new MyFilterPipeline().ExecuteAsync(context, TestContext.Current.CancellationToken);

        result.IsError.Should().BeFalse();
        context.Payload["text"].Should().Be("HELLO");
    }

    [Fact]
    public async Task TheDocumentedFilterPipeline_WithoutThePayloadKey_SucceedsWithoutTouchingIt()
    {
        var context = Context();

        var result = await new MyFilterPipeline().ExecuteAsync(context, TestContext.Current.CancellationToken);

        result.IsError.Should().BeFalse();
        context.Payload.Should().NotContainKey("text");
    }

    [Fact]
    public async Task TheDocumentedOptionsPipeline_MatchesTheGenericInterface()
    {
        var result = await new MyOptionsPipeline().ExecuteAsync(Context(), new MyPipelineOptions(), TestContext.Current.CancellationToken);

        result.IsError.Should().BeFalse();
    }

    /// <summary>
    /// The documented narrowing step. A pipeline that cannot narrow the target was wired against a
    /// target it does not serve, which is a configuration error rather than a runtime condition.
    /// </summary>
    [Fact]
    public void TheDocumentedTargetNarrowing_ReachesTheVectorSettings()
    {
        var context = Context();

        context.Target.Should().BeOfType<VectorMemoryTarget>();
        var target = (VectorMemoryTarget)context.Target;

        target.EmbeddingProvider.Should().Be("openai");
        target.EmbeddingModel.Should().Be("text-embedding-3-small");
        target.StorageName.Should().Be("qdrant");
        target.CollectionName.Should().Be("notes");
    }

    [Fact]
    public void MemoryContextPayload_ComparesKeysAsOrdinal_AsDocumented()
    {
        var context = Context();
        context.Payload["text"] = "a";

        context.Payload.Should().NotContainKey("Text", "the payload is documented as an ordinal map");
    }
}
