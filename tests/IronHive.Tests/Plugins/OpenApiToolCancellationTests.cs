using AwesomeAssertions;
using IronHive.Abstractions.Tools;
using IronHive.Plugins.OpenAPI;

namespace IronHive.Tests.Plugins;

/// <summary>
/// An OpenAPI tool tries its servers in turn. A caller that cancels between two servers must see the cancellation, not
/// «request failed on all servers» — that is a tool failure the model would be told about and might retry.
/// </summary>
public class OpenApiToolCancellationTests
{
    [Fact]
    public async Task InvokeAsync_CancelledBetweenServers_Throws_InsteadOfReportingAllServersFailed()
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var http = new HttpClient(new FailAndCancel(cts));
        var tool = new OpenApiTool(http)
        {
            ClientName = "pets",
            OperationId = "listPets",
            BaseUris = [new Uri("https://first.test"), new Uri("https://second.test")],
            Method = HttpMethod.Get,
            Path = "/pets",
            Properties = new OpenApiProperties(),
        };

        var act = () => tool.InvokeAsync(new ToolInput(), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // The first server answers 503, and the caller cancels while it does — after the request itself completed, so no
    // awaited call observes the token before the next server is tried.
    private sealed class FailAndCancel(CancellationTokenSource cts) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cts.Cancel();
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("busy"),
            });
        }
    }
}
