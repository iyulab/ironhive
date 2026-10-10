using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using AwesomeAssertions;
using IronHive.Abstractions.Http;

namespace IronHive.Tests.Providers;

/// <summary>
/// The providers' transport races a host's addresses: <c>http://localhost:…</c> to a server listening on IPv4 only
/// connects at once even with a 2 s connect timeout. Before, <c>::1</c> was tried first, a refused IPv6 connect takes
/// about 2 s on Windows, and the timeout expired before <c>127.0.0.1</c> was reached. An unreachable host still fails
/// within the timeout.
/// </summary>
public class ProviderConnectTests
{
    [Fact]
    public async Task Localhost_ToAnIpv4OnlyServer_ConnectsWellInsideTheTimeout()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var server = RespondOnceAsync(listener);

        using var client = new HttpClient(ProviderConnect.CreateHandler(TimeSpan.FromSeconds(2)));
        var watch = Stopwatch.StartNew();
        var body = await client.GetStringAsync($"http://localhost:{port}/", TestContext.Current.CancellationToken);
        watch.Stop();

        body.Should().Be("ok");
        watch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1.5),
            "127.0.0.1 is tried 250 ms after ::1 rather than after ::1 has been refused");
        await server;
    }

    // The connect timeout bounds the step by itself: an attempt that does not observe its token (a connect that is slow
    // to cancel on a loaded machine) must not hold the race past the caller's cancellation.
    [Fact]
    public async Task ACallerCancellation_EndsTheRace_EvenWhenNoAttemptObservesItsToken()
    {
        var never = new TaskCompletionSource<Socket>();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        // After every address has started (AttemptDelay 250 ms): before that the stagger delay would wake the race anyway.
        timeout.CancelAfter(TimeSpan.FromMilliseconds(600));

        var race = ProviderConnect.RaceAsync(
            [IPAddress.IPv6Loopback, IPAddress.Loopback], 1, (_, _, _) => never.Task, timeout.Token);
        var finished = await Task.WhenAny(race, Task.Delay(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken));

        finished.Should().BeSameAs(race, "the race returns on the caller's cancellation, not when an attempt gives up");
        var act = () => race;
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // End to end on a real refused port. The precise bound lives in the fact above; wall time here includes scheduling on a
    // loaded test host (14 s was seen against a 2 s timeout while a full suite ran), so this bound only says «no hang».
    [Fact]
    public async Task NoListener_FailsWithinTheConnectTimeout()
    {
        int port;
        using (var probe = new TcpListener(IPAddress.Loopback, 0))
        {
            probe.Start();
            port = ((IPEndPoint)probe.LocalEndpoint).Port;
        }

        using var client = new HttpClient(ProviderConnect.CreateHandler(TimeSpan.FromSeconds(2)));
        var watch = Stopwatch.StartNew();
        var act = () => client.GetStringAsync($"http://localhost:{port}/", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<Exception>();
        watch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task LiteralAddress_Connects()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var server = RespondOnceAsync(listener);

        using var client = new HttpClient(ProviderConnect.CreateHandler(TimeSpan.FromSeconds(2)));
        var body = await client.GetStringAsync($"http://127.0.0.1:{port}/", TestContext.Current.CancellationToken);

        body.Should().Be("ok");
        await server;
    }

    // Windows reports a refused connect after about 2 s; with the compatible providers' old 2 s default the connect
    // timeout won that race, and a server that was down read as a timeout.
    [Theory]
    [InlineData("compatible")]
    [InlineData("gpustack")]
    public async Task ARefusedConnection_UnderTheCompatibleDefaults_IsReportedAsRefused(string provider)
    {
        var timeout = provider == "gpustack"
            ? new IronHive.Providers.OpenAI.Compatible.GpuStack.GpuStackConfig().ConnectTimeout
            : new IronHive.Providers.OpenAI.Compatible.OpenAICompatibleConfig().ConnectTimeout;
        int port;
        using (var probe = new TcpListener(IPAddress.Loopback, 0))
        {
            probe.Start();
            port = ((IPEndPoint)probe.LocalEndpoint).Port;
        }

        using var client = new HttpClient(ProviderConnect.CreateHandler(timeout));
        var failure = await Assert.ThrowsAnyAsync<Exception>(
            () => client.GetStringAsync($"http://127.0.0.1:{port}/", TestContext.Current.CancellationToken));

        var chain = new List<Exception>();
        for (var e = failure; e is not null; e = e.InnerException) chain.Add(e);
        chain.OfType<SocketException>().Should().ContainSingle()
            .Which.SocketErrorCode.Should().Be(SocketError.ConnectionRefused);
    }

    // An attempt that has already finished when the race looks at it. Loopback connects sometimes complete
    // synchronously; until 0.41.1 the race then saw "nothing pending, no address left", threw a ConnectionRefused it
    // made up, and disposed the connected socket as a loser — about one request in 300 to a local server.
    [Fact]
    public async Task AnAttemptThatCompletedSynchronously_Wins()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var connected = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        connected.Connect(IPAddress.Loopback, port);

        using var socket = await ProviderConnect.RaceAsync(
            [IPAddress.Loopback], port, (_, _, _) => Task.FromResult(connected), TestContext.Current.CancellationToken);

        socket.Should().BeSameAs(connected);
        socket.Connected.Should().BeTrue("the winner must not have been disposed as a loser");
    }

    [Fact]
    public async Task ASynchronousFailureThenASynchronousSuccess_ReturnsTheSuccess()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var connected = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        connected.Connect(IPAddress.Loopback, port);

        using var socket = await ProviderConnect.RaceAsync(
            [IPAddress.IPv6Loopback, IPAddress.Loopback], port,
            (address, _, _) => address.Equals(IPAddress.IPv6Loopback)
                ? Task.FromException<Socket>(new SocketException((int)SocketError.ConnectionRefused))
                : Task.FromResult(connected),
            TestContext.Current.CancellationToken);

        socket.Should().BeSameAs(connected);
    }

    [Fact]
    public async Task EveryAttemptFailingSynchronously_ReportsTheRealError()
    {
        var act = () => ProviderConnect.RaceAsync(
            [IPAddress.Loopback], 1,
            (_, _, _) => Task.FromException<Socket>(new SocketException((int)SocketError.HostUnreachable)),
            TestContext.Current.CancellationToken);

        (await act.Should().ThrowAsync<SocketException>()).Which.SocketErrorCode.Should().Be(SocketError.HostUnreachable);
    }

    private static async Task RespondOnceAsync(TcpListener listener)
    {
        using var socket = await listener.AcceptSocketAsync();
        var buffer = new byte[4096];
        await socket.ReceiveAsync(buffer, SocketFlags.None);
        await socket.SendAsync(Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Length: 2\r\nConnection: close\r\n\r\nok"), SocketFlags.None);
    }
}
