using System.Net;
using System.Net.Sockets;

namespace IronHive.Abstractions.Http;

/// <summary>
/// The transport every provider builds when the consumer does not inject an <see cref="HttpClient"/>: a
/// <see cref="SocketsHttpHandler"/> whose connect step races the host's addresses (RFC 8305 "Happy Eyeballs") instead of
/// trying them one after another inside a single connect timeout.
/// </summary>
/// <remarks>
/// <para>
/// <c>localhost</c> resolves to <c>::1</c> first. A local server that listens on IPv4 only — llama-server, Ollama and
/// most local inference servers do by default — refuses the IPv6 attempt, and on Windows a refused connect takes about
/// two seconds. The default handler tries the addresses in order within one <see cref="SocketsHttpHandler.ConnectTimeout"/>,
/// so a short timeout (the OpenAI-compatible provider's is 2 s, chosen so a dead LAN host fails over fast) expired
/// before <c>127.0.0.1</c> was ever tried. Every request to <c>http://localhost:…</c> failed with a connect timeout.
/// </para>
/// <para>
/// Here the next address starts <see cref="AttemptDelay"/> after the previous one, or as soon as it fails. The first
/// connection wins and the rest are cancelled. The connect timeout still bounds the whole step, so an unreachable host
/// fails exactly as fast as before.
/// </para>
/// </remarks>
public static class ProviderConnect
{
    /// <summary>The delay before the next address is tried while the previous attempt is still pending (RFC 8305 §5).</summary>
    public static readonly TimeSpan AttemptDelay = TimeSpan.FromMilliseconds(250);

    /// <summary>A handler with <paramref name="connectTimeout"/> and the racing connect step.</summary>
    public static SocketsHttpHandler CreateHandler(TimeSpan connectTimeout) => new()
    {
        ConnectTimeout = connectTimeout,
        ConnectCallback = ConnectAsync,
    };

    /// <summary>
    /// A <see cref="SocketsHttpHandler.ConnectCallback"/> that races the resolved addresses of the request's host.
    /// </summary>
    public static async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        var endpoint = context.DnsEndPoint;
        var addresses = IPAddress.TryParse(endpoint.Host, out var literal)
            ? [literal]
            : Interleave(await Dns.GetHostAddressesAsync(endpoint.Host, cancellationToken).ConfigureAwait(false));
        if (addresses.Length == 0)
            throw new SocketException((int)SocketError.HostNotFound);

        var socket = await RaceAsync(addresses, endpoint.Port, cancellationToken).ConfigureAwait(false);
        return new NetworkStream(socket, ownsSocket: true);
    }

    private static async Task<Socket> RaceAsync(IPAddress[] addresses, int port, CancellationToken cancellationToken)
    {
        using var race = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var attempts = new List<Task<Socket>>(addresses.Length);
        Exception? lastError = null;
        var next = 0;

        try
        {
            while (true)
            {
                if (next < addresses.Length)
                    attempts.Add(ConnectOneAsync(addresses[next++], port, race.Token));

                var pending = attempts.Where(a => !a.IsCompleted).ToList();
                if (pending.Count == 0 && next >= addresses.Length)
                    break;

                // Wait for an attempt to finish, or for the delay after which the next address starts anyway.
                var waitOn = new List<Task>(pending);
                if (next < addresses.Length)
                    waitOn.Add(Task.Delay(AttemptDelay, race.Token));
                if (waitOn.Count > 0)
                    await Task.WhenAny(waitOn).ConfigureAwait(false);

                foreach (var done in attempts.Where(a => a.IsCompleted).ToList())
                {
                    attempts.Remove(done);
                    if (done.IsCompletedSuccessfully)
                        return done.Result;
                    lastError = done.Exception?.GetBaseException() ?? lastError;
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
        }
        finally
        {
            // Losers: cancel, then dispose any socket that connected after the winner.
            race.Cancel();
            foreach (var loser in attempts)
            {
                _ = loser.ContinueWith(static t => { if (t.IsCompletedSuccessfully) t.Result.Dispose(); },
                    CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        throw lastError ?? new SocketException((int)SocketError.ConnectionRefused);
    }

    private static async Task<Socket> ConnectOneAsync(IPAddress address, int port, CancellationToken cancellationToken)
    {
        var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
        try
        {
            await socket.ConnectAsync(new IPEndPoint(address, port), cancellationToken).ConfigureAwait(false);
            return socket;
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }

    // RFC 8305 §4: alternate address families, starting with the first one the resolver returned.
    private static IPAddress[] Interleave(IPAddress[] resolved)
    {
        if (resolved.Length < 2)
            return resolved;
        var first = resolved.Where(a => a.AddressFamily == resolved[0].AddressFamily).ToList();
        var other = resolved.Where(a => a.AddressFamily != resolved[0].AddressFamily).ToList();
        var result = new List<IPAddress>(resolved.Length);
        for (var i = 0; i < Math.Max(first.Count, other.Count); i++)
        {
            if (i < first.Count) result.Add(first[i]);
            if (i < other.Count) result.Add(other[i]);
        }
        return [.. result];
    }
}
