using AwesomeAssertions;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;
using OpenAIConfig = IronHive.Providers.OpenAI.OpenAIConfig;
using OpenAIMapper = IronHive.Providers.OpenAI.OpenAIExceptionMapper;
using AnthropicMapper = IronHive.Providers.Anthropic.AnthropicExceptionMapper;
using GoogleAIMapper = IronHive.Providers.GoogleAI.GoogleAIExceptionMapper;

namespace IronHive.Tests.Providers;

/// <summary>
/// A provider SDK's own network timeout cancels the in-flight request via an internal
/// <see cref="CancellationTokenSource"/> the caller never sees — it surfaces as a bare
/// <see cref="OperationCanceledException"/>, identical in shape to the caller's own token
/// being canceled. Each provider mapper (and the raw <see cref="ChatCompletionHttpClient"/>)
/// must tell the two apart: only when the caller's own token is NOT what requested
/// cancellation is this really a timeout, and it must surface as <see cref="TimeoutException"/>
/// rather than a plain cancel.
/// </summary>
public class TimeoutMappingTests
{
    [Fact]
    public void Anthropic_Map_TranslatesUnrelatedCancellation_ToTimeoutException()
    {
        var sdkTimeout = new OperationCanceledException("sdk network timeout");

        var mapped = AnthropicMapper.Map(sdkTimeout, CancellationToken.None);

        mapped.Should().BeOfType<TimeoutException>().Which.InnerException.Should().BeSameAs(sdkTimeout);
    }

    [Fact]
    public void Anthropic_Map_LeavesCallerCancellation_Unmapped()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var callerCancel = new OperationCanceledException("caller canceled", cts.Token);

        AnthropicMapper.Map(callerCancel, cts.Token).Should().BeNull();
    }

    [Fact]
    public void OpenAI_Map_TranslatesUnrelatedCancellation_ToTimeoutException()
    {
        var sdkTimeout = new OperationCanceledException("sdk network timeout");

        var mapped = OpenAIMapper.Map(sdkTimeout, CancellationToken.None);

        mapped.Should().BeOfType<TimeoutException>().Which.InnerException.Should().BeSameAs(sdkTimeout);
    }

    [Fact]
    public void OpenAI_Map_LeavesCallerCancellation_Unmapped()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var callerCancel = new OperationCanceledException("caller canceled", cts.Token);

        OpenAIMapper.Map(callerCancel, cts.Token).Should().BeNull();
    }

    [Fact]
    public void Google_Map_TranslatesUnrelatedCancellation_ToTimeoutException()
    {
        var sdkTimeout = new OperationCanceledException("sdk network timeout");

        var mapped = GoogleAIMapper.Map(sdkTimeout, CancellationToken.None);

        mapped.Should().BeOfType<TimeoutException>().Which.InnerException.Should().BeSameAs(sdkTimeout);
    }

    [Fact]
    public void Google_Map_LeavesCallerCancellation_Unmapped()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var callerCancel = new OperationCanceledException("caller canceled", cts.Token);

        GoogleAIMapper.Map(callerCancel, cts.Token).Should().BeNull();
    }

    // ---- OpenAI.Compatible (raw HttpClient — no SDK, no shared Map() seam) ----

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            // Simulates what HttpClient itself throws when its own Timeout elapses:
            // a TaskCanceledException whose token is unrelated to the caller's.
            => throw new TaskCanceledException("The request was canceled due to the configured HttpClient.Timeout.");
    }

    [Fact]
    public async Task ChatCompletionHttpClient_PostAsync_TranslatesHttpClientTimeout_ToTimeoutException()
    {
        var config = new OpenAIConfig
        {
            BaseUrl = "https://example.invalid",
            HttpClient = new HttpClient(new ThrowingHandler()),
        };
        using var client = new ChatCompletionHttpClient(config);
        var request = new ChatCompletionRequest { Model = "test-model", Messages = [] };

        var act = () => client.PostAsync(request, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<TimeoutException>();
    }

    [Fact]
    public async Task ChatCompletionHttpClient_PostAsync_PropagatesCallerCancellation_AsIs()
    {
        var config = new OpenAIConfig
        {
            BaseUrl = "https://example.invalid",
            HttpClient = new HttpClient(new ThrowingHandler()),
        };
        using var client = new ChatCompletionHttpClient(config);
        var request = new ChatCompletionRequest { Model = "test-model", Messages = [] };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => client.PostAsync(request, cts.Token);

        var ex = await act.Should().ThrowAsync<OperationCanceledException>();
        ex.Which.Should().NotBeOfType<TimeoutException>();
    }
}
