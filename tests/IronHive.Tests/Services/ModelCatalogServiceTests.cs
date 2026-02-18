using System.Net;
using FluentAssertions;
using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Registries;
using IronHive.Core.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace IronHive.Tests.Services;

public class ModelCatalogServiceTests
{
    private readonly IProviderRegistry _registry = Substitute.For<IProviderRegistry>();

    private ModelCatalogService CreateService() => new(_registry);

    // Test implementation of IModelSpec
    private sealed class TestModelSpec : IModelSpec
    {
        public required string ModelId { get; init; }
        public string? DisplayName { get; init; }
        public string? Description { get; init; }
        public string? OwnedBy { get; init; }
        public DateTime? CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    private sealed class SpecialModelSpec : IModelSpec
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
        _registry.Entries<IModelCatalog>()
            .Returns(Enumerable.Empty<KeyValuePair<string, IModelCatalog>>());

        var service = CreateService();
        var result = await service.ListModelsAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ListModelsAsync_SingleProvider_ReturnsModels()
    {
        var catalog = Substitute.For<IModelCatalog>();
        catalog.ListModelsAsync(Arg.Any<CancellationToken>())
            .Returns([new TestModelSpec { ModelId = "gpt-4" }]);

        _registry.Entries<IModelCatalog>()
            .Returns([new KeyValuePair<string, IModelCatalog>("openai", catalog)]);

        var service = CreateService();
        var result = (await service.ListModelsAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].Provider.Should().Be("openai");
        result[0].Models.Should().ContainSingle(m => m.ModelId == "gpt-4");
    }

    [Fact]
    public async Task ListModelsAsync_MultipleProviders_ReturnsAll()
    {
        var catalog1 = Substitute.For<IModelCatalog>();
        catalog1.ListModelsAsync(Arg.Any<CancellationToken>())
            .Returns([new TestModelSpec { ModelId = "gpt-4" }]);

        var catalog2 = Substitute.For<IModelCatalog>();
        catalog2.ListModelsAsync(Arg.Any<CancellationToken>())
            .Returns([new TestModelSpec { ModelId = "claude-3" }]);

        _registry.Entries<IModelCatalog>()
            .Returns([
                new KeyValuePair<string, IModelCatalog>("openai", catalog1),
                new KeyValuePair<string, IModelCatalog>("anthropic", catalog2)
            ]);

        var service = CreateService();
        var result = (await service.ListModelsAsync()).ToList();

        result.Should().HaveCount(2);
        result.Select(r => r.Provider).Should().Contain(["openai", "anthropic"]);
    }

    [Fact]
    public async Task ListModelsAsync_UnauthorizedProvider_ReturnsEmptyModelsForThatProvider()
    {
        var catalog = Substitute.For<IModelCatalog>();
        catalog.ListModelsAsync(Arg.Any<CancellationToken>())
            .Throws(new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized));

        _registry.Entries<IModelCatalog>()
            .Returns([new KeyValuePair<string, IModelCatalog>("bad-provider", catalog)]);

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
        var catalog = Substitute.For<IModelCatalog>();
        catalog.ListModelsAsync(Arg.Any<CancellationToken>())
            .Returns([new TestModelSpec { ModelId = "model-1" }]);

        _registry.TryGet("openai", out Arg.Any<IModelCatalog>()!)
            .Returns(x =>
            {
                x[1] = catalog;
                return true;
            });

        var service = CreateService();
        var result = await service.ListModelsAsync("openai");

        result.Should().NotBeNull();
        result!.Provider.Should().Be("openai");
        result.Models.Should().ContainSingle();
    }

    [Fact]
    public async Task ListModelsAsync_ByProvider_UnknownProvider_ReturnsNull()
    {
        _registry.TryGet("unknown", out Arg.Any<IModelCatalog>()!)
            .Returns(false);

        var service = CreateService();
        var result = await service.ListModelsAsync("unknown");

        result.Should().BeNull();
    }

    // --- FindModelAsync ---

    [Fact]
    public async Task FindModelAsync_ExistingModel_ReturnsSpec()
    {
        var catalog = Substitute.For<IModelCatalog>();
        catalog.FindModelAsync("gpt-4", Arg.Any<CancellationToken>())
            .Returns(new TestModelSpec { ModelId = "gpt-4", DisplayName = "GPT-4" });

        _registry.TryGet("openai", out Arg.Any<IModelCatalog>()!)
            .Returns(x =>
            {
                x[1] = catalog;
                return true;
            });

        var service = CreateService();
        var result = await service.FindModelAsync("openai", "gpt-4");

        result.Should().NotBeNull();
        result!.ModelId.Should().Be("gpt-4");
    }

    [Fact]
    public async Task FindModelAsync_UnknownProvider_ReturnsNull()
    {
        _registry.TryGet("missing", out Arg.Any<IModelCatalog>()!)
            .Returns(false);

        var service = CreateService();
        var result = await service.FindModelAsync("missing", "any-model");

        result.Should().BeNull();
    }

    [Fact]
    public async Task FindModelAsync_UnknownModel_ReturnsNull()
    {
        var catalog = Substitute.For<IModelCatalog>();
        catalog.FindModelAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns((IModelSpec?)null);

        _registry.TryGet("openai", out Arg.Any<IModelCatalog>()!)
            .Returns(x =>
            {
                x[1] = catalog;
                return true;
            });

        var service = CreateService();
        var result = await service.FindModelAsync("openai", "nonexistent");

        result.Should().BeNull();
    }

    // --- FindModelAsync<T> (typed) ---

    [Fact]
    public async Task FindModelAsync_Typed_MatchingType_ReturnsTyped()
    {
        var model = new SpecialModelSpec { ModelId = "special", ContextWindow = 128000 };
        var catalog = Substitute.For<IModelCatalog>();
        catalog.FindModelAsync("special", Arg.Any<CancellationToken>())
            .Returns(model);

        _registry.TryGet("openai", out Arg.Any<IModelCatalog>()!)
            .Returns(x =>
            {
                x[1] = catalog;
                return true;
            });

        var service = CreateService();
        var result = await service.FindModelAsync<SpecialModelSpec>("openai", "special");

        result.Should().NotBeNull();
        result!.ContextWindow.Should().Be(128000);
    }

    [Fact]
    public async Task FindModelAsync_Typed_WrongType_ReturnsNull()
    {
        var model = new TestModelSpec { ModelId = "basic" };
        var catalog = Substitute.For<IModelCatalog>();
        catalog.FindModelAsync("basic", Arg.Any<CancellationToken>())
            .Returns(model);

        _registry.TryGet("openai", out Arg.Any<IModelCatalog>()!)
            .Returns(x =>
            {
                x[1] = catalog;
                return true;
            });

        var service = CreateService();
        var result = await service.FindModelAsync<SpecialModelSpec>("openai", "basic");

        result.Should().BeNull();
    }
}
