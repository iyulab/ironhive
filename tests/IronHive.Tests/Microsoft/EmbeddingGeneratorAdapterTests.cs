using AwesomeAssertions;
using IronHive.Core.Microsoft;
using Microsoft.Extensions.AI;
using NSubstitute;
using IronHiveEmbedding = IronHive.Abstractions.Embedding;

namespace IronHive.Tests.Microsoft;

public class EmbeddingGeneratorAdapterTests : IDisposable
{
    private static readonly float[] s_vec1 = [0.1f, 0.2f, 0.3f];
    private static readonly float[] s_vec2 = [0.4f, 0.5f, 0.6f];

    private readonly IronHiveEmbedding.IEmbeddingGenerator _mockGenerator;
    private readonly EmbeddingGeneratorAdapter _adapter;

    public EmbeddingGeneratorAdapterTests()
    {
        _mockGenerator = Substitute.For<IronHiveEmbedding.IEmbeddingGenerator>();
        _adapter = new EmbeddingGeneratorAdapter(_mockGenerator, "test-model", "TestProvider");
    }

    public void Dispose()
    {
        _adapter.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Constructor

    [Fact]
    public void Constructor_NullGenerator_ThrowsArgumentNullException()
    {
        var act = () => new EmbeddingGeneratorAdapter(null!, "model");

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("generator");
    }

    [Fact]
    public void Constructor_NullModelId_ThrowsArgumentNullException()
    {
        var act = () => new EmbeddingGeneratorAdapter(_mockGenerator, null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("modelId");
    }

    [Fact]
    public void Constructor_NullProviderName_DefaultsToIronHive()
    {
        var adapter = new EmbeddingGeneratorAdapter(_mockGenerator, "model");

        adapter.Metadata.ProviderName.Should().Be("IronHive");
        adapter.Dispose();
    }

    #endregion

    #region Metadata

    [Fact]
    public void Metadata_ReturnsCorrectProviderAndModel()
    {
        _adapter.Metadata.ProviderName.Should().Be("TestProvider");
        _adapter.Metadata.DefaultModelId.Should().Be("test-model");
        _adapter.Metadata.ProviderUri.Should().BeNull();
    }

    #endregion

    #region GenerateAsync

    [Fact]
    public async Task GenerateAsync_DelegatesTo_EmbedBatchAsync()
    {
        SetupEmbeddings(s_vec1, s_vec2);

        await _adapter.GenerateAsync(["input1", "input2"]);

        await _mockGenerator.Received(1).EmbedBatchAsync(
            "test-model",
            Arg.Is<IEnumerable<string>>(x => x != null && x.Count() == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerateAsync_UsesOptionsModelId_WhenProvided()
    {
        SetupEmbeddings(s_vec1);
        var options = new EmbeddingGenerationOptions { ModelId = "override-model" };

        await _adapter.GenerateAsync(["input"], options);

        await _mockGenerator.Received(1).EmbedBatchAsync(
            "override-model",
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerateAsync_UsesDefaultModel_WhenNoOptions()
    {
        SetupEmbeddings(s_vec1);

        await _adapter.GenerateAsync(["input"]);

        await _mockGenerator.Received(1).EmbedBatchAsync(
            "test-model",
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerateAsync_ReturnsCorrectEmbeddings()
    {
        SetupEmbeddings(s_vec1, s_vec2);

        var result = await _adapter.GenerateAsync(["input1", "input2"]);

        result.Should().HaveCount(2);
        result[0].Vector.ToArray().Should().BeEquivalentTo(s_vec1);
        result[1].Vector.ToArray().Should().BeEquivalentTo(s_vec2);
    }

    [Fact]
    public async Task GenerateAsync_RefusesAPartialSet_RatherThanMisalignItWithTheInputs()
    {
        // This previously returned the surviving 2 of 3 as a valid result. Because the contract is
        // positional, the caller would then have matched s_vec2 -- the embedding of "c" -- to input
        // "b", with nothing anywhere reporting a problem.
        var results = new List<IronHiveEmbedding.EmbeddingResult>
        {
            new() { Index = 0, Embedding = s_vec1 },
            new() { Index = 1, Embedding = null },
            new() { Index = 2, Embedding = s_vec2 }
        };
        _mockGenerator.EmbedBatchAsync(
            Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(results);

        var act = async () => await _adapter.GenerateAsync(["a", "b", "c"]);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("2").And.Contain("3");
    }

    [Fact]
    public async Task GenerateAsync_RequestedDimensions_AreCheckedAgainstWhatTheModelProduced()
    {
        SetupEmbeddings(s_vec1, s_vec2);   // 3-dimensional

        var act = async () => await _adapter.GenerateAsync(
            ["a", "b"], new EmbeddingGenerationOptions { Dimensions = 1536 });

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("1536").And.Contain("3");
    }

    [Fact]
    public async Task GenerateAsync_RequestedDimensions_AreAcceptedWhenTheModelAlreadyMatches()
    {
        // Dimensions is honored "if supported", so a request the model happens to satisfy must pass
        // through untouched -- the check is on the result, not on a capability list.
        SetupEmbeddings(s_vec1, s_vec2);

        var result = await _adapter.GenerateAsync(
            ["a", "b"], new EmbeddingGenerationOptions { Dimensions = s_vec1.Length });

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GenerateAsync_UnsetDimensions_AcceptsTheModelsNativeSize()
    {
        SetupEmbeddings(s_vec1);

        var result = await _adapter.GenerateAsync(["a"], new EmbeddingGenerationOptions());

        result.Should().ContainSingle();
        result[0].Vector.Length.Should().Be(s_vec1.Length);
    }

    [Fact]
    public async Task GenerateAsync_LeavesUsageUnset_WhenTheProviderReportsNoTokenCounts()
    {
        // EmbeddingResult carries no usage, so there is nothing to report. This previously returned
        // the input string count under InputTokenCount -- a different quantity presented as a token
        // count, which silently corrupts any cost arithmetic downstream of it.
        SetupEmbeddings(s_vec1, s_vec2);

        var result = await _adapter.GenerateAsync(["a much longer input than one token", "input2"]);

        result.Should().HaveCount(2);
        result.Usage.Should().BeNull();
    }

    [Fact]
    public async Task GenerateAsync_PassesCancellationToken()
    {
        SetupEmbeddings(s_vec1);
        using var cts = new CancellationTokenSource();

        await _adapter.GenerateAsync(["input"], cancellationToken: cts.Token);

        await _mockGenerator.Received(1).EmbedBatchAsync(
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            cts.Token);
    }

    [Fact]
    public async Task GenerateAsync_SingleInput_ReturnsSingleEmbedding()
    {
        SetupEmbeddings(s_vec1);

        var result = await _adapter.GenerateAsync(["single input"]);

        result.Should().ContainSingle();
        result[0].Vector.ToArray().Should().BeEquivalentTo(s_vec1);
    }

    #endregion

    #region GetService

    [Fact]
    public void GetService_IEmbeddingGenerator_ReturnsGenerator()
    {
        var result = _adapter.GetService(typeof(IronHiveEmbedding.IEmbeddingGenerator));

        result.Should().BeSameAs(_mockGenerator);
    }

    [Fact]
    public void GetService_UnknownType_ReturnsNull()
    {
        var result = _adapter.GetService(typeof(string));

        result.Should().BeNull();
    }

    #endregion

    #region Dispose

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var act = () => _adapter.Dispose();

        act.Should().NotThrow();
    }

    #endregion

    #region Helpers

    private void SetupEmbeddings(params float[][] vectors)
    {
        var results = vectors.Select((v, i) => new IronHiveEmbedding.EmbeddingResult
        {
            Index = i,
            Embedding = v
        });
        _mockGenerator.EmbedBatchAsync(
            Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(results);
    }

    #endregion
}
