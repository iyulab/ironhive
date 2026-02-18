using System.Net;
using FluentAssertions;
using IronHive.Abstractions.Resilience;
using IronHive.Core.Resilience;
using Polly;

namespace IronHive.Tests.Resilience;

public class ResiliencePipelineFactoryTests
{
    // --- DefaultOptions ---

    [Fact]
    public void DefaultOptions_IsNotNull()
    {
        ResiliencePipelineFactory.DefaultOptions.Should().NotBeNull();
    }

    [Fact]
    public void DefaultOptions_Enabled_IsTrue()
    {
        ResiliencePipelineFactory.DefaultOptions.Enabled.Should().BeTrue();
    }

    [Fact]
    public void DefaultOptions_RetryEnabled_IsTrue()
    {
        ResiliencePipelineFactory.DefaultOptions.Retry.Enabled.Should().BeTrue();
    }

    [Fact]
    public void DefaultOptions_CircuitBreakerEnabled_IsTrue()
    {
        ResiliencePipelineFactory.DefaultOptions.CircuitBreaker.Enabled.Should().BeTrue();
    }

    [Fact]
    public void DefaultOptions_TimeoutEnabled_IsTrue()
    {
        ResiliencePipelineFactory.DefaultOptions.Timeout.Enabled.Should().BeTrue();
    }

    // --- Create<T> ---

    [Fact]
    public void Create_NullOptions_UsesDefaults()
    {
        var pipeline = ResiliencePipelineFactory.Create<string>(null);

        pipeline.Should().NotBeNull();
        pipeline.Should().NotBeSameAs(ResiliencePipeline<string>.Empty);
    }

    [Fact]
    public void Create_DisabledOptions_ReturnsEmptyPipeline()
    {
        var options = new ResilienceOptions { Enabled = false };

        var pipeline = ResiliencePipelineFactory.Create<string>(options);

        pipeline.Should().BeSameAs(ResiliencePipeline<string>.Empty);
    }

    [Fact]
    public void Create_AllStrategiesEnabled_ReturnsNonEmptyPipeline()
    {
        var options = new ResilienceOptions
        {
            Enabled = true,
            Retry = new RetryOptions { Enabled = true },
            CircuitBreaker = new CircuitBreakerOptions { Enabled = true },
            Timeout = new TimeoutOptions { Enabled = true }
        };

        var pipeline = ResiliencePipelineFactory.Create<string>(options);

        pipeline.Should().NotBeNull();
        pipeline.Should().NotBeSameAs(ResiliencePipeline<string>.Empty);
    }

    [Fact]
    public void Create_OnlyRetryEnabled_ReturnsNonEmptyPipeline()
    {
        var options = new ResilienceOptions
        {
            Enabled = true,
            Retry = new RetryOptions { Enabled = true },
            CircuitBreaker = new CircuitBreakerOptions { Enabled = false },
            Timeout = new TimeoutOptions { Enabled = false }
        };

        var pipeline = ResiliencePipelineFactory.Create<string>(options);

        pipeline.Should().NotBeSameAs(ResiliencePipeline<string>.Empty);
    }

    [Fact]
    public void Create_OnlyCircuitBreakerEnabled_ReturnsNonEmptyPipeline()
    {
        var options = new ResilienceOptions
        {
            Enabled = true,
            Retry = new RetryOptions { Enabled = false },
            CircuitBreaker = new CircuitBreakerOptions { Enabled = true },
            Timeout = new TimeoutOptions { Enabled = false }
        };

        var pipeline = ResiliencePipelineFactory.Create<string>(options);

        pipeline.Should().NotBeSameAs(ResiliencePipeline<string>.Empty);
    }

    [Fact]
    public void Create_OnlyTimeoutEnabled_ReturnsNonEmptyPipeline()
    {
        var options = new ResilienceOptions
        {
            Enabled = true,
            Retry = new RetryOptions { Enabled = false },
            CircuitBreaker = new CircuitBreakerOptions { Enabled = false },
            Timeout = new TimeoutOptions { Enabled = true }
        };

        var pipeline = ResiliencePipelineFactory.Create<string>(options);

        pipeline.Should().NotBeSameAs(ResiliencePipeline<string>.Empty);
    }

    [Fact]
    public void Create_AllStrategiesDisabled_ButEnabledTrue_ReturnsEmptyEquivalent()
    {
        var options = new ResilienceOptions
        {
            Enabled = true,
            Retry = new RetryOptions { Enabled = false },
            CircuitBreaker = new CircuitBreakerOptions { Enabled = false },
            Timeout = new TimeoutOptions { Enabled = false }
        };

        // Even though Enabled=true, no strategies → pipeline built with no strategies
        var pipeline = ResiliencePipelineFactory.Create<string>(options);
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_SuccessfulOperation_PassesThrough()
    {
        var options = new ResilienceOptions
        {
            Enabled = true,
            Retry = new RetryOptions { Enabled = true, MaxRetries = 1 },
            CircuitBreaker = new CircuitBreakerOptions { Enabled = false },
            Timeout = new TimeoutOptions { Enabled = false }
        };

        var pipeline = ResiliencePipelineFactory.Create<string>(options);

        var result = await pipeline.ExecuteAsync(
            async _ => { await Task.CompletedTask; return "success"; });

        result.Should().Be("success");
    }

    // --- CreateAsync (non-generic) ---

    [Fact]
    public void CreateAsync_NullOptions_UsesDefaults()
    {
        var pipeline = ResiliencePipelineFactory.CreateAsync(null);

        pipeline.Should().NotBeNull();
        pipeline.Should().NotBeSameAs(ResiliencePipeline.Empty);
    }

    [Fact]
    public void CreateAsync_DisabledOptions_ReturnsEmptyPipeline()
    {
        var options = new ResilienceOptions { Enabled = false };

        var pipeline = ResiliencePipelineFactory.CreateAsync(options);

        pipeline.Should().BeSameAs(ResiliencePipeline.Empty);
    }

    [Fact]
    public void CreateAsync_AllStrategiesEnabled_ReturnsNonEmptyPipeline()
    {
        var options = new ResilienceOptions
        {
            Enabled = true,
            Retry = new RetryOptions { Enabled = true },
            CircuitBreaker = new CircuitBreakerOptions { Enabled = true },
            Timeout = new TimeoutOptions { Enabled = true }
        };

        var pipeline = ResiliencePipelineFactory.CreateAsync(options);

        pipeline.Should().NotBeSameAs(ResiliencePipeline.Empty);
    }

    [Fact]
    public async Task CreateAsync_SuccessfulOperation_PassesThrough()
    {
        var options = new ResilienceOptions
        {
            Enabled = true,
            Retry = new RetryOptions { Enabled = true, MaxRetries = 1 },
            CircuitBreaker = new CircuitBreakerOptions { Enabled = false },
            Timeout = new TimeoutOptions { Enabled = false }
        };

        var pipeline = ResiliencePipelineFactory.CreateAsync(options);
        var executed = false;

        await pipeline.ExecuteAsync(async _ => { await Task.CompletedTask; executed = true; });

        executed.Should().BeTrue();
    }

    // --- ResilienceOptions defaults ---

    [Fact]
    public void RetryOptions_Defaults_AreCorrect()
    {
        var retry = new RetryOptions();

        retry.MaxRetries.Should().Be(3);
        retry.InitialDelay.Should().Be(TimeSpan.FromSeconds(1));
        retry.MaxDelay.Should().Be(TimeSpan.FromSeconds(30));
        retry.BackoffMultiplier.Should().Be(2);
        retry.UseJitter.Should().BeTrue();
    }

    [Fact]
    public void CircuitBreakerOptions_Defaults_AreCorrect()
    {
        var cb = new CircuitBreakerOptions();

        cb.FailureRatio.Should().Be(0.5);
        cb.SamplingDuration.Should().Be(TimeSpan.FromSeconds(30));
        cb.MinimumThroughput.Should().Be(10);
        cb.BreakDuration.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void TimeoutOptions_Defaults_AreCorrect()
    {
        var timeout = new TimeoutOptions();

        timeout.TotalTimeout.Should().Be(TimeSpan.FromSeconds(120));
        timeout.ChunkTimeout.Should().Be(TimeSpan.FromSeconds(30));
    }
}
