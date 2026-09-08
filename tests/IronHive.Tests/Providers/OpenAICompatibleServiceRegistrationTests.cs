using AwesomeAssertions;
using IronHive.Abstractions;
using IronHive.Core;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible;
using IronHive.Providers.OpenAI.Compatible.GpuStack;
using IronHive.Providers.OpenAI.Compatible.Reranking;

namespace IronHive.Tests.Providers;

/// <summary>
/// Verifies that each <see cref="OpenAICompatibleServiceType"/>/<see cref="GpuStackServiceType"/> flag
/// registers the provider type it's documented to — including the newer <c>Images</c>/<c>Audio</c>/
/// <c>Rerank</c> flags, and that GPUStack's message generator is the shared
/// <see cref="OpenAICompatibleMessageGenerator"/> rather than a GPUStack-specific class.
/// </summary>
public class OpenAICompatibleServiceRegistrationTests
{
    [Fact]
    public void OpenAICompatible_All_RegistersEveryServiceWithTheDocumentedType()
    {
        var config = new OpenAICompatibleConfig { BaseUrl = "http://localhost:11434" };

        var hive = new HiveServiceBuilder()
            .AddOpenAICompatibleProviders("ollama", config, OpenAICompatibleServiceType.All)
            .Build();

        hive.Models.Finders["ollama"].Should().BeOfType<OpenAIModelFinder>();
        hive.Messages.Generators["ollama"].Should().BeOfType<OpenAICompatibleMessageGenerator>();
        hive.Embeddings.Generators["ollama"].Should().BeOfType<OpenAIEmbeddingGenerator>();
        hive.Rerank.Rerankers["ollama"].Should().BeOfType<CohereDocumentReranker>();
        hive.Images.Generators["ollama"].Should().BeOfType<OpenAIImageGenerator>();
        hive.Audio.Processors["ollama"].Should().BeOfType<OpenAIAudioProcessor>();
    }

    [Fact]
    public void OpenAICompatible_ImagesFlagAlone_RegistersOnlyImages()
    {
        var config = new OpenAICompatibleConfig { BaseUrl = "http://localhost:11434" };

        var hive = new HiveServiceBuilder()
            .AddOpenAICompatibleProviders("ollama", config, OpenAICompatibleServiceType.Images)
            .Build();

        hive.Images.Generators.Should().ContainKey("ollama");
        hive.Audio.Processors.Should().NotContainKey("ollama");
        hive.Messages.Generators.Should().NotContainKey("ollama");
    }

    [Fact]
    public void OpenAICompatible_AudioFlagAlone_RegistersOnlyAudio()
    {
        var config = new OpenAICompatibleConfig { BaseUrl = "http://localhost:11434" };

        var hive = new HiveServiceBuilder()
            .AddOpenAICompatibleProviders("ollama", config, OpenAICompatibleServiceType.Audio)
            .Build();

        hive.Audio.Processors.Should().ContainKey("ollama");
        hive.Images.Generators.Should().NotContainKey("ollama");
    }

    [Fact]
    public void GpuStack_All_RegistersEveryServiceWithTheDocumentedType()
    {
        var config = new GpuStackConfig { BaseUrl = "http://localhost:8080" };

        var hive = new HiveServiceBuilder()
            .AddGpuStackProviders("gpustack", config, GpuStackServiceType.All)
            .Build();

        hive.Models.Finders["gpustack"].Should().BeOfType<OpenAIModelFinder>();
        // GPUStack no longer has its own message-generator class — it shares
        // OpenAICompatibleMessageGenerator with OpenAICompatibleConfig.
        hive.Messages.Generators["gpustack"].Should().BeOfType<OpenAICompatibleMessageGenerator>();
        hive.Embeddings.Generators["gpustack"].Should().BeOfType<OpenAIEmbeddingGenerator>();
        hive.Rerank.Rerankers["gpustack"].Should().BeOfType<CohereDocumentReranker>();
        hive.Images.Generators["gpustack"].Should().BeOfType<OpenAIImageGenerator>();
        hive.Audio.Processors["gpustack"].Should().BeOfType<OpenAIAudioProcessor>();
    }

    [Fact]
    public void GpuStack_ImagesFlagAlone_RegistersOnlyImages()
    {
        var config = new GpuStackConfig { BaseUrl = "http://localhost:8080" };

        var hive = new HiveServiceBuilder()
            .AddGpuStackProviders("gpustack", config, GpuStackServiceType.Images)
            .Build();

        hive.Images.Generators.Should().ContainKey("gpustack");
        hive.Audio.Processors.Should().NotContainKey("gpustack");
        hive.Messages.Generators.Should().NotContainKey("gpustack");
    }

    [Fact]
    public void GpuStack_AudioFlagAlone_RegistersOnlyAudio()
    {
        var config = new GpuStackConfig { BaseUrl = "http://localhost:8080" };

        var hive = new HiveServiceBuilder()
            .AddGpuStackProviders("gpustack", config, GpuStackServiceType.Audio)
            .Build();

        hive.Audio.Processors.Should().ContainKey("gpustack");
        hive.Images.Generators.Should().NotContainKey("gpustack");
    }
}
