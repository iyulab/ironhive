using System.Net;
using FluentAssertions;
using IronHive.Abstractions.Models;
using IronHive.Core.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace IronHive.Tests.Services;

public class ModelServiceTests
{
    private readonly Dictionary<string, IModelFinder> _catalogs = new();

    private ModelService CreateService() => new(_catalogs);

    // Test implementation of IModelCard
    private sealed class TestModelCard : IModelCard
    {
        public required string ModelId { get; init; }
        public string? DisplayName { get; init; }
        public string? Description { get; init; }
        public string? OwnedBy { get; init; }
        public DateTime? CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    private sealed class SpecialModelCard : IModelCard
    {
        public required string ModelId { get; init; }
        public string? DisplayName { get; init; }
        public string? Description { get; init; }
        public string? OwnedBy { get; init; }
        public DateTime? CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public int ContextWindow { get; init; }
    }

    // --- ListModelsAsync (all providers) ---

    [Fact]
    public async Task ListModelsAsync_NoProviders_ReturnsEmpty()
    {
        var service = CreateService();
        var result = await service.ListModelsAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ListModelsAsync_SingleProvider_ReturnsModels()
    {
        var catalog = Substitute.For<IModelFinder>();
        catalog.ListModelsAsync(Arg.Any<CancellationToken>())
            .Returns([new TestModelCard { ModelId = "gpt-4" }]);
        _catalogs["openai"] = catalog;

        var service = CreateService();
        var result = (await service.ListModelsAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].Provider.Should().Be("openai");
        result[0].Models.Should().ContainSingle(m => m.ModelId == "gpt-4");
    }

    [Fact]
    public async Task ListModelsAsync_MultipleProviders_ReturnsAll()
    {
        var catalog1 = Substitute.For<IModelFinder>();
        catalog1.ListModelsAsync(Arg.Any<CancellationToken>())
            .Returns([new TestModelCard { ModelId = "gpt-4" }]);

        var catalog2 = Substitute.For<IModelFinder>();
        catalog2.ListModelsAsync(Arg.Any<CancellationToken>())
            .Returns([new TestModelCard { ModelId = "claude-3" }]);

        _catalogs["openai"] = catalog1;
        _catalogs["anthropic"] = catalog2;

        var service = CreateService();
        var result = (await service.ListModelsAsync()).ToList();

        result.Should().HaveCount(2);
        result.Select(r => r.Provider).Should().Contain(["openai", "anthropic"]);
    }

    [Fact]
    public async Task ListModelsAsync_UnauthorizedProvider_ReturnsEmptyModelsForThatProvider()
    {
        var catalog = Substitute.For<IModelFinder>();
        catalog.ListModelsAsync(Arg.Any<CancellationToken>())
            .Throws(new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized));
        _catalogs["bad-provider"] = catalog;

        var service = CreateService();
        var result = (await service.ListModelsAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].Provider.Should().Be("bad-provider");
        result[0].Models.Should().BeEmpty();
    }

    // --- ListModelsAsync (single provider) ---

    [Fact]
    public async Task ListModelsAsync_ByProvider_ExistingProvider_ReturnsModels()
    {
        var catalog = Substitute.For<IModelFinder>();
        catalog.ListModelsAsync(Arg.Any<CancellationToken>())
            .Returns([new TestModelCard { ModelId = "model-1" }]);
        _catalogs["openai"] = catalog;

        var service = CreateService();
        var result = await service.ListModelsAsync("openai");

        result.Should().NotBeNull();
        result!.Provider.Should().Be("openai");
        result.Models.Should().ContainSingle();
    }

    [Fact]
    public async Task ListModelsAsync_ByProvider_UnknownProvider_ReturnsNull()
    {
        var service = CreateService();
        var result = await service.ListModelsAsync("unknown");

        result.Should().BeNull();
    }

    // --- FindModelAsync ---

    [Fact]
    public async Task FindModelAsync_ExistingModel_ReturnsSpec()
    {
        var catalog = Substitute.For<IModelFinder>();
        catalog.FindModelAsync("gpt-4", Arg.Any<CancellationToken>())
            .Returns(new TestModelCard { ModelId = "gpt-4", DisplayName = "GPT-4" });
        _catalogs["openai"] = catalog;

        var service = CreateService();
        var result = await service.FindModelAsync("openai", "gpt-4");

        result.Should().NotBeNull();
        result!.ModelId.Should().Be("gpt-4");
    }

    [Fact]
    public async Task FindModelAsync_UnknownProvider_ReturnsNull()
    {
        var service = CreateService();
        var result = await service.FindModelAsync("missing", "any-model");

        result.Should().BeNull();
    }

    [Fact]
    public async Task FindModelAsync_UnknownModel_ReturnsNull()
    {
        var catalog = Substitute.For<IModelFinder>();
        catalog.FindModelAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns((IModelCard?)null);
        _catalogs["openai"] = catalog;

        var service = CreateService();
        var result = await service.FindModelAsync("openai", "nonexistent");

        result.Should().BeNull();
    }

    // --- FindModelAsync<T> (typed) ---

    [Fact]
    public async Task FindModelAsync_Typed_MatchingType_ReturnsTyped()
    {
        var model = new SpecialModelCard { ModelId = "special", ContextWindow = 128000 };
        var catalog = Substitute.For<IModelFinder>();
        catalog.FindModelAsync("special", Arg.Any<CancellationToken>())
            .Returns(model);
        _catalogs["openai"] = catalog;

        var service = CreateService();
        var result = await service.FindModelAsync<SpecialModelCard>("openai", "special");

        result.Should().NotBeNull();
        result!.ContextWindow.Should().Be(128000);
    }

    [Fact]
    public async Task FindModelAsync_Typed_WrongType_ReturnsNull()
    {
        var model = new TestModelCard { ModelId = "basic" };
        var catalog = Substitute.For<IModelFinder>();
        catalog.FindModelAsync("basic", Arg.Any<CancellationToken>())
            .Returns(model);
        _catalogs["openai"] = catalog;

        var service = CreateService();
        var result = await service.FindModelAsync<SpecialModelCard>("openai", "basic");

        result.Should().BeNull();
    }
}
