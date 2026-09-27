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
        watch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(4));
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
