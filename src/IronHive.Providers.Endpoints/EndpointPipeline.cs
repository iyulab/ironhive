using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace IronHive.Providers.Endpoints;

/// <summary>The steps every provider wire shares: read, parse, choose the client, call it, write the reply or the error.</summary>
internal static partial class EndpointPipeline
{
    public static async Task HandleAsync(HttpContext context, IWireCodec codec, ProviderEndpointOptions options)
    {
        var cancellationToken = context.RequestAborted;
        var logger = context.RequestServices.GetService(typeof(ILoggerFactory)) is ILoggerFactory factory
            ? factory.CreateLogger("IronHive.Providers.Endpoints")
            : Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        try
        {
            JsonDocument document;
            try
            {
                document = await ReadBodyAsync(context.Request, options.MaxRequestBodyBytes, cancellationToken).ConfigureAwait(false);
            }
            catch (BodyTooLargeException)
            {
                await codec.WriteErrorAsync(context,
                    new WireError(StatusCodes.Status413PayloadTooLarge, WireErrorKind.PayloadTooLarge,
                        $"The request body is larger than {options.MaxRequestBodyBytes} bytes."), cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (JsonException)
            {
                await codec.WriteErrorAsync(context,
                    new WireError(StatusCodes.Status400BadRequest, WireErrorKind.InvalidRequest, "The request body is not valid JSON."),
                    cancellationToken).ConfigureAwait(false);
                return;
            }

            using (document)
            {
                WireRequest request;
                try
                {
                    request = codec.Parse(document.RootElement, context);
                }
                catch (WireRequestException ex)
                {
                    await codec.WriteErrorAsync(context,
                        new WireError(StatusCodes.Status400BadRequest, WireErrorKind.InvalidRequest, ex.Message, ex.Param),
                        cancellationToken).ConfigureAwait(false);
                    return;
                }

                var client = await options.ClientResolver!(context, request.Model).ConfigureAwait(false);
                if (client is null)
                {
                    await codec.WriteErrorAsync(context,
                        new WireError(StatusCodes.Status404NotFound, WireErrorKind.NotFound,
                            request.Model is null ? "No model is available for this request." : $"The model '{request.Model}' does not exist or is not available.",
                            "model"), cancellationToken).ConfigureAwait(false);
                    return;
                }

                if (options.ConfigureChatOptions is { } configure)
                {
                    await configure(context, request.Options).ConfigureAwait(false);
                }

                if (request.Stream)
                {
                    await StreamAsync(context, codec, options, client, request, logger, cancellationToken).ConfigureAwait(false);
                    return;
                }

                ChatResponse response;
                try
                {
                    response = await client.GetResponseAsync(request.Messages, request.Options, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
                {
                    LogClientFailure(logger, codec.Name, ex);
                    await codec.WriteErrorAsync(context, ToWireError(ex, options), cancellationToken).ConfigureAwait(false);
                    return;
                }

                await codec.WriteResponseAsync(context, request, response, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The caller went away; there is nobody to answer.
        }
    }

    private static async Task StreamAsync(
        HttpContext context, IWireCodec codec, ProviderEndpointOptions options, IChatClient client, WireRequest request,
        ILogger logger, CancellationToken cancellationToken)
    {
        await using var enumerator = client.GetStreamingResponseAsync(request.Messages, request.Options, cancellationToken)
            .GetAsyncEnumerator(cancellationToken);
        bool hasFirst;
        try
        {
            // The first update is read before anything is sent, so an upstream failure that comes first is still an ordinary error response.
            hasFirst = await enumerator.MoveNextAsync().ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            LogClientFailure(logger, codec.Name, ex);
            await codec.WriteErrorAsync(context, ToWireError(ex, options), cancellationToken).ConfigureAwait(false);
            return;
        }

        try
        {
            await codec.WriteStreamAsync(context, request, Replay(enumerator, hasFirst, cancellationToken), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            LogClientFailure(logger, codec.Name, ex);
            var error = ToWireError(ex, options);
            if (context.Response.HasStarted)
            {
                await codec.WriteStreamErrorAsync(context, error, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await codec.WriteErrorAsync(context, error, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static async IAsyncEnumerable<ChatResponseUpdate> Replay(
        IAsyncEnumerator<ChatResponseUpdate> enumerator, bool hasCurrent,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!hasCurrent)
        {
            yield break;
        }

        yield return enumerator.Current;
        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return enumerator.Current;
        }
    }

    private static WireError ToWireError(Exception exception, ProviderEndpointOptions options)
    {
        var detail = options.ExposeErrorDetails ? exception.Message : "The server had an error while processing the request.";
        if (exception is HttpRequestException { StatusCode: System.Net.HttpStatusCode.TooManyRequests })
        {
            return new WireError(StatusCodes.Status429TooManyRequests, WireErrorKind.RateLimited,
                options.ExposeErrorDetails ? exception.Message : "The model is rate limited; retry later.");
        }

        return new WireError(StatusCodes.Status500InternalServerError, WireErrorKind.Server, detail);
    }

    private static async Task<JsonDocument> ReadBodyAsync(HttpRequest request, long limit, CancellationToken cancellationToken)
    {
        if (request.ContentLength is { } declared && declared > limit)
        {
            throw new BodyTooLargeException();
        }

        using var buffer = new MemoryStream();
        var chunk = new byte[16 * 1024];
        int read;
        while ((read = await request.Body.ReadAsync(chunk, cancellationToken).ConfigureAwait(false)) > 0)
        {
            if (buffer.Length + read > limit)
            {
                throw new BodyTooLargeException();
            }

            buffer.Write(chunk, 0, read);
        }

        buffer.Position = 0;
        return await JsonDocument.ParseAsync(buffer, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "The chat client failed while answering a {Wire} request")]
    private static partial void LogClientFailure(ILogger logger, string wire, Exception exception);

    private sealed class BodyTooLargeException : Exception;
}
