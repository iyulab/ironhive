# Changelog

All notable changes to IronHive are documented here. Pre-1.0 (0.x): breaking
changes are expected and used freely for structural correctness (see
`docs/CONSTITUTION.md`).

## Unreleased

## 0.26.1 — 2026-09-12

### Fixed

- `OpenAIMessageGenerator`: a streamed response that ended incomplete (`max_output_tokens`,
  `content_filter`) reported its `ResponseId` as `openai_<id>`, while the buffered call and a
  completed stream reported the raw `<id>`. `MessageService` adds the `<provider>_` prefix itself,
  so the incomplete path alone surfaced as `openai_openai_<id>`.

### Changed

- `OpenAIMessageGenerator` now fills `Model` and `Timestamp` on every path — the buffered response
  and the completed stream's done frame carried neither, the incomplete stream carried both.
- `AnthropicMessageGenerator` now fills `Model` on both paths (from the response, and from
  `message_start` on the stream); it carried it on neither.
- Provider generators gain equivalence tests that drive the real SDK and the real mapping with
  recorded vendor bodies through an injected `HttpClient` (`StubHttpHandler`), asserting that the
  buffered and streaming halves agree on done reason, usage, id, model, timestamp and content.
  OpenAI Responses first; the other generators follow.

## 0.26.0 — 2026-09-11

### Added

- Every built-in agent middleware now takes part in streaming calls. `TimeoutMiddleware`,
  `RetryMiddleware`, `RateLimitMiddleware`, `CircuitBreakerMiddleware`, `BulkheadMiddleware`,
  `FallbackMiddleware` and `CachingMiddleware` implemented only `IAgentMiddleware`, so
  `InvokeStreamingAsync` and an orchestrator's `ExecuteStreamingAsync` skipped them without a word: a
  timeout, retry or rate limit configured for an agent — or through `MiddlewarePacks` — did nothing
  on a streaming call. They now implement `IStreamingAgentMiddleware` with these semantics:
  - `Timeout` bounds the whole stream, from the first frame to the last, including the time the
    caller spends between frames; it does not measure time-to-first-frame or idle gaps separately.
  - `Retry` and `Fallback` act only on a failure that happens before any frame reached the caller;
    a later failure propagates, since delivered frames cannot be taken back. `ResponseValidator`
    needs a complete response and applies to `InvokeAsync` only.
  - `RateLimit`, `Bulkhead` and `CircuitBreaker` take their slot or check their state before the
    first frame and settle it when the stream ends — completed, failed, or abandoned by the caller.
    A stream the caller stops reading is recorded as neither success nor failure by the breaker.
  - `Caching` stores a stream that was read to a normal end (`EndTurn` / `MaxTokens`) and replays its
    frames to the next streaming call with the same input. Buffered responses and streams are stored
    separately; `CacheCount` and `MaxCacheSize` count both.

  A middleware of your own still needs `IStreamingAgentMiddleware` to run on streaming calls, and
  registering one without it now traces a warning naming the middleware — on an agent, in a
  `CompositeMiddleware` pack, or in `OrchestratorOptions.AgentMiddlewares`. Such a middleware is
  skipped on streaming calls, and the call succeeds, so nothing in the result shows that the
  protection it was configured for was absent.
- `ToolResultBudgetMiddleware` (`IronHive.Core.Services`) bounds the total tool-result text one
  `IMessageService` call sends to the model. A per-result cap (`ToolOptions.OnAfterInvoke` with
  `TextCompactor`) cannot stop results from adding up across rounds, and `OnAfterInvoke` runs in
  parallel, so it cannot keep a running total either. Registered with `AddMessageMiddleware`, the
  middleware shares the budget out in call order before every turn: a result longer than what is left
  is cut to fit, a result with nothing left is replaced by a notice, and from the turn the budget runs
  out the request asks for `ToolChoice.None` so the model answers with what it has. Text is counted;
  other content is sent as is. Applies to buffered and streaming calls.

### Fixed

- A streaming `IMessageService` call no longer swallows a tool that throws. The streaming tool loop
  completed its progress channel without the tools' exception, so the failure went unobserved, the
  tool was left without a result, and the loop carried on; the buffered call fails with the same
  `InvalidOperationException` it always did, and the streaming call now does too.
- The `IChatClient` bridge (`ChatClientAdapter`) no longer flattens every tool result to a string. A
  `FunctionResultContent` whose result is `AIContent` or a list of it now keeps its structure —
  text as text, `image/*` data as an image block — so providers that carry image tool results
  (Anthropic, Google AI) receive the image. A block the bridge cannot carry is named in text instead
  of being dropped. A result that records an exception is now sent as a failed tool output, so the
  providers' error flag is set; the text is the invoker's `Result`. Other values keep their string
  form.
- `BulkheadMiddleware` could corrupt its queue accounting. When the wrapped call itself failed, the
  handler meant for a failure while waiting also ran, and — if another request was queued —
  decremented that request's count and released the queue slot a second time, letting more requests
  queue than `MaxQueueSize` allows.
- `FallbackMiddleware` no longer reports the caller's own cancellation of the fallback call as
  `FallbackFailedException`; the cancellation propagates.

## 0.25.0 — 2026-09-11

### Changed

- Orchestrators no longer turn a cancelled run into a failed agent step. The shared agent-execution
  path caught the `OperationCanceledException` raised when the orchestration's own token was
  cancelled and recorded it as `Agent '<name>' failed: A task was canceled.` — so an orchestration
  timeout was never reported as one, and a caller cancelling a run got a failed result back instead
  of the exception. An orchestration timeout now returns `Orchestration timed out after <n>s` from
  every orchestrator, and cancelling the token you passed throws `OperationCanceledException`.
- `MessageService.GenerateStreamingMessageAsync` now ends the call when a turn fails. A provider error
  frame (`StreamingMessageErrorResponse`) is still yielded, and is then followed by an
  `InvalidOperationException` — the same outcome as `GenerateMessageAsync`, whose generator throws.
  Before, a turn that ended with an error frame had no done reason, which the tool loop read as
  "continue": it sent the same request again, up to `MaxTurns` (50 by default), and then finished
  with a done frame as if the call had completed. A generator stream that ends without a done frame
  now throws for the same reason.

### Fixed

- `MessageService.GenerateMessageAsync` with `Suggestions` now extracts suggestion blocks from each
  turn as it arrives, as the streaming call does. It used to scan the whole message after the tool
  loop, which re-parsed text carried in from an earlier call when resuming — returning that call's
  suggestions again and editing the caller's input message — and sent the next turn of a tool loop
  the unstripped block as history.

- `SequentialOrchestrator` and `GraphOrchestrator` now report the same `OrchestrationResult` from
  `ExecuteStreamingAsync` as from `ExecuteAsync`. The streaming halves rebuilt each step on their
  own and disagreed with the buffered halves in several ways:
  - A step's message was reassembled from text deltas alone, so reasoning and other non-text
    content was dropped from the step, from the final output and from the input passed to the next
    agent. The message the agent puts on its done frame is now used.
  - With `StopOnAgentFailure` (the default), a failed agent ended the stream with `Completed` and a
    successful result whenever an earlier agent had succeeded. It now ends with `Failed`, like the
    buffered call.
  - `ContextScope` and `ResultDistiller` were not applied when streaming.
  - `GraphOrchestrator` stopped in the middle of a topological level on a failure, where the
    buffered call finishes the level first; it also reported an orchestration timeout as an agent
    timeout. Timeout messages now match the buffered call's.

## 0.24.1 — 2026-09-09

### Fixed

- MCP health checks now follow the negotiated protocol revision. The 2026-07-28 revision removed the
  `ping` utility and made `server/discover` mandatory, and the SDK negotiates that revision by
  default, but `McpSession.HealthAsync` still judged liveness by ping — so a server implementing
  exactly the current specification was reported unhealthy and moved to `Errored` by the very call
  that asked whether it was alive. A session negotiated at 2026-07-28 or later is now checked with
  `server/discover`; an initialize-era session keeps ping.

### Added

- `McpSession.NegotiatedProtocolVersion` exposes the revision the SDK agreed on for the session,
  or `null` before connecting.

### Removed

- The `NU5104` suppression in `IronHive.Plugins.MCP`, which predated the MCP SDK's stable release and
  was only hiding future prerelease dependencies.


## 0.24.0 — 2026-09-08

### Added — `GpuStackConfig.ToOpenAICompatible()` is public

0.23.0 removed `GpuStackMessageGenerator` in favour of
`new OpenAICompatibleMessageGenerator(config.ToOpenAICompatible())`, but left the converter
`internal`, so only the registry's `AddGpuStackProviders` could take that path — a consumer that
builds generators directly (a gateway adapter registering GPUStack as a candidate, for example) had
no public way left to construct a GPUStack chat generator and had to re-derive the conversion by
hand. The converter is now public, mirroring the already-public `ToOpenAI()`, and a test pins that it
carries every setting (`/v1-openai/` path, static base URL/API key fallbacks, both resolvers by
reference, `ConnectTimeout`, `TokenLimitParameter`).

## 0.23.0 — 2026-09-08

### Changed — every provider now defaults to a short connect timeout and an unbounded request timeout

`OpenAIConfig.Timeout` (renamed from `TimeOut` — every other provider already spelled it this way),
`AnthropicConfig.Timeout`, `GoogleAIConfig.Timeout`, and `VertexAIConfig.Timeout` are now non-nullable
`TimeSpan`, defaulting to `System.Threading.Timeout.InfiniteTimeSpan` instead of a fixed ceiling (10
minutes) — the same sentinel `HttpClient.Timeout` itself uses for "no timeout", so a request has no
ceiling of its own unless one is set explicitly. This replaces the earlier `TimeSpan?`-with-`null`-means-
infinite design: a nullable field bought nothing here since every consumer only ever needed a single
"unset" state, and non-nullable keeps parity with `ConnectTimeout` and the underlying `HttpClient`/
`SocketsHttpHandler` properties it feeds. Each config gains a `ConnectTimeout` (default 5 seconds;
unchanged at 2 seconds for the LAN-oriented `OpenAICompatibleConfig`/`GpuStackConfig`) that bounds only
TCP connection establishment via `SocketsHttpHandler.ConnectTimeout`, so an unreachable host still fails
fast instead of hanging for the whole request window.

When no `HttpClient`/`HttpClientFactory` is injected, every factory now builds its own default client
carrying `ConnectTimeout` with `HttpClient.Timeout` disabled (`Timeout.InfiniteTimeSpan`) — previously only
`OpenAICompatibleConfig`/`GpuStackConfig` did this; `OpenAIClientFactory`, `AnthropicClientFactory`, and
`GoogleAIClientFactory` fell through to the SDK's own default transport, which for GoogleAI meant silently
inheriting a bare `HttpClient`'s 100-second default once the adapter stopped imposing its own ceiling.
`GoogleAIDefaults` (which existed only to hold that 10-minute fallback) is removed — `GoogleAIConfig`/
`VertexAIConfig` now inline their `ConnectTimeout` default like every other provider config does.

**Behaviour change.** A deployment that relied on the previous fixed default (10 minutes for OpenAI/GoogleAI/
VertexAI, whatever the vendor SDK defaulted to for Anthropic) to bound a stalled request will now wait
indefinitely instead — set the provider's `Timeout` explicitly to restore a ceiling. `OpenAIConfig.TimeOut`
is renamed to `Timeout`, a source-breaking change for any caller that set it by name.

### Added — `IDocumentReranker`/`IRerankService`, `CohereDocumentReranker`

New capability area mirroring `Embeddings`: `IronHive.Abstractions.Reranking.IDocumentReranker`
(provider-level) and `IRerankService` (app-level, exposed as `IHiveService.Rerank` and its
`Rerankers` provider dictionary — not `Generators`, since the provider interface isn't named
`*Generator`), wired through `IHiveServiceBuilder.AddDocumentReranker`.
`CohereDocumentReranker` (`IronHive.Providers.OpenAI.Compatible.Reranking`, payloads in
`CohereRerankPayloads.cs`) talks to a Cohere-shaped `POST /rerank`
(<https://docs.cohere.com/reference/rerank>) — the shape self-hosted servers such as Infinity
(explicitly a "locally deployed Cohere rerank API"), vLLM's `/rerank`/`/v1/rerank`/`/v2/rerank`
(documented Cohere/Jina-compatible), and GPUStack's `/v1/rerank` (Jina-compatible, a superset of
Cohere's shape) implement, named for the wire protocol it actually speaks rather than
`OpenAICompatible*` (reusing an already-resolved `OpenAIConfig` only for connection settings,
matching the naming precedent set by `OpenAICompatible*` itself: named for the shape spoken, not
the package it lives in). Not every self-hosted rerank server follows this shape — HuggingFace's
Text Embeddings Inference (TEI) uses its own distinct request fields (`texts`/`raw_scores`/
`return_text` rather than `documents`/`model`/`top_n`) and is not compatible with this client.
`OpenAICompatibleServiceType` gains a `Rerank` flag (included in `All`) so
`AddOpenAICompatibleProviders` registers it alongside the other services.

`CohereDocumentReranker` takes an already-resolved `OpenAIConfig` (mirroring
`ChatCompletionMessageGenerator`) rather than `OpenAICompatibleConfig` directly, because the rerank
path is not always the chat/embeddings path on the same server: GPUStack serves chat/embeddings/
models at `/v1-openai/` but rerank at `/v1/` (llama-box backend only). `GpuStackServiceType` gains
a matching `Rerank` flag (included in `All`); `GpuStackConfig.ToRerankConfig()` (internal) builds
the `/v1/`-targeting `OpenAIConfig` alongside the existing `ToOpenAI()` for `/v1-openai/`.

### Removed — `GpuStackMessageGenerator` (merged into `OpenAICompatibleMessageGenerator`)

`GpuStackMessageGenerator` and `OpenAICompatibleMessageGenerator` were identical modulo the config
type they held (`GpuStackConfig` vs `OpenAICompatibleConfig`) — same dynamic-resolution wrapper,
same signature-based inner-generator swap, same `TokenLimitParameter` hand-off. That duplication had
already caused a real defect (0.19.0: a `TokenLimitParameter` fix landed on the OpenAICompatible side
and not the GPUStack one, because there was no single place to fix it). `GpuStackConfig` gains
`ToOpenAICompatible()` (internal), converting itself to an `OpenAICompatibleConfig` (`Path` set to
GPUStack's `/v1-openai/`, resolvers/`TokenLimitParameter`/`ConnectTimeout` carried over as-is —
dynamic base-URL/API-key rotation via `BaseUrlResolver`/`ApiKeyResolver` still works after conversion
since the same delegate is reused, not re-wrapped), mirroring the existing `ToOpenAI()`/
`ToRerankConfig()` converters on the same class. `GpuStackMessageGenerator.cs` is deleted;
`AddGpuStackProviders` now registers `new OpenAICompatibleMessageGenerator(config.ToOpenAICompatible())`.
No behavior change for either provider — this is a duplication removal, not a feature change.

### Added — `Images`/`Audio` flags on `OpenAICompatibleServiceType`/`GpuStackServiceType`

`OpenAIImageGenerator` and `OpenAIAudioProcessor` (`IronHive.Providers.OpenAI`) already take a plain
`OpenAIConfig`, same as `OpenAIEmbeddingGenerator`/`OpenAIModelFinder` — nothing OpenAI-specific stops
them from working against any OpenAI-compatible endpoint. `OpenAICompatibleServiceType` and
`GpuStackServiceType` each gain `Images`/`Audio` flags (both included in `All`), and
`AddOpenAICompatibleProviders`/`AddGpuStackProviders` register the two generators via `config.ToOpenAI()`
the same way embeddings and models already were. Support in practice depends on what the target
server/deployed models actually implement (e.g. GPUStack routes image requests to its SGLang backend
and audio to VoxBox) — registering the flag does not itself guarantee the server understands the call.

### Added — `SpeechToTextRequest.Diarized`

`OpenAIAudioProcessor.TranscribeAsync` selected the diarized transcription endpoint by checking whether
`Model` contained the substring `"diarize"` — a request that happened to name a diarization-capable model
without that substring silently got the plain (non-diarized) response shape. `SpeechToTextRequest` now
carries an explicit `Diarized` flag instead (named to match the vendor SDK's own
`AudioTranscriptionFormat.Diarized` and the adjective/state-flag convention used elsewhere in this
codebase, e.g. `Done`, `IsSuccess` — a bare verb read as "record in a diary", not "perform diarization").

`GoogleAIAudioProcessor.TranscribeAsync` previously ignored the request entirely and always forced a
structured JSON response (segments, speakers, timestamps) regardless of whether diarization was wanted,
paying for that constraint on every plain-transcript call. It now branches on `Diarized`: a plain-text
prompt with no response schema when `false`, the existing structured-output request when `true`.

### Changed — `GoogleAIEmbeddingGenerator.EmbedBatchAsync` chunks and parallelizes above 100 inputs

Google AI's `embedContent` accepts at most 100 inputs per call; a batch larger than that previously failed
against the vendor API with no indication of the limit. `EmbedBatchAsync` now splits the input into
chunks of at most 100, issues one request per chunk in parallel, and reassembles the results in original
input order.

### Added — per-tool `JsonSerializerOptions`

`FunctionTool.JsonOptions` (nullable, per-instance) now controls both argument deserialization and
result serialization, falling back to a new `JsonDefaultOptions.FunctionOptions` when unset —
mirroring the existing `Timeout` pattern (global default + per-tool override). `FunctionOptions`
starts from the same settings as the general-purpose `Options`, except `WriteIndented` is off: tool
results are consumed by the model as text, not read by a human, so indentation only costs tokens.
`FunctionToolFactory.DelegateDescriptor` carries `JsonOptions` through the delegate-registration path.

### Fixed — MCP tool content mapped to its CLR type name instead of real content

`McpTool.InvokeAsync` mapped every MCP `ContentBlock` through `ContentBlock.ToString()`, but only
`TextContentBlock` overrides it — image/audio/resource content was serialized as its own CLR type
name. Now maps text/image/audio blocks (and embedded text/blob resources) onto their matching
`MessageContent` types, with an unrecognized-MIME-type or unsupported-block-type text placeholder as
fallback, so `ToolOutput.Content` carries real content instead of garbage.

### Fixed — `OpenApiTool` JSON request body serialized with no `JsonSerializerOptions`

`OpenApiTool`'s JSON request body was serialized with zero `JsonSerializerOptions` (not even the
general-purpose default), so enums went out as raw numbers and property casing/escaping didn't match
what most external APIs expect. Now uses `JsonDefaultOptions.Options`.

### Added — `Generators`/`Finders`/`Processors` on every service, `IReadOnlyDictionary.GetOrFirstValue`

`IHiveService.GetMessageGenerator`/`GetEmbeddingGenerator` (0.22.0) special-cased raw provider access
for two of the six services. `Models`, `Messages`, `Embeddings`, `Images`, `Videos`, `Audio` now all
expose their registered-provider dictionary directly — `IModelService.Finders`, `IAudioService.Processors`,
and `Generators` on the other four — instead of each service growing its own `Get{}` accessor.

`IReadOnlyDictionary<string, TValue>.GetOrFirstValue(key)` (new extension, next to the existing
`IDictionary<string, object?>.TryGetValue<T>` in the same `System.Collections.Generic`-namespaced
`DictionaryExtensions`, so no new `using` is needed) replaces `GeneratorLookup`: pass a key to look it up,
or omit it to auto-select the sole registered entry and throw if none or more than one are registered.
Error messages name the entry by `typeof(TValue).Name` rather than a caller-supplied label. One shared
extension now backs all six services' provider lookup instead of a `Core`-internal helper only
`MessageService` and `HiveService` used.

`IHiveService.GetMessageGenerator`/`GetEmbeddingGenerator` are `[Obsolete]` — still work (they delegate to
`Messages.Generators.GetOrFirstValue`/`Embeddings.Generators.GetOrFirstValue`) but will be removed in a
future release. Use `Messages.Generators`/`Embeddings.Generators` directly instead.

### Fixed — provider network timeouts surfaced as a cancel, not a timeout

An SDK's or `HttpClient`'s own internal timeout cancels the in-flight request through a
`CancellationTokenSource` the caller never sees, which throws a bare `OperationCanceledException` —
indistinguishable in shape from the caller's own token being canceled. `RetryMiddleware`'s
`catch (OperationCanceledException) { throw; }` then silently treated a real timeout the same as a
user cancellation: no retry, and no way for a consumer to tell the two apart.

`AnthropicExceptionMapper`, `OpenAIExceptionMapper`, `GoogleAIExceptionMapper`, and the raw
`ChatCompletionHttpClient` (OpenAI-compatible / self-hosted servers) now check whether the caller's own
`cancellationToken` actually requested the cancellation; when it did not, the exception is rethrown as
`System.TimeoutException` instead, matching the convention `TimeoutMiddleware` already used.

**Behaviour change**: code that caught `OperationCanceledException` to detect a provider-side timeout
will no longer see it there — it now arrives as `TimeoutException`. A caller's own cancellation is
unaffected and still surfaces as `OperationCanceledException`.

### Added — `AudioMessageContent`

`MessageContent` gains an `"audio"` variant — `AudioMessageContent { Format, Base64 }`, shaped like
`ImageMessageContent`, with `AudioFormat` covering `Wav`/`Mp3`/`Flac`/`Aac`/`Ogg`/`Aiff`.

Wired into the two generators whose vendor API actually accepts audio input: GoogleAI (any of the 6
formats, via `Part.InlineData` — the same path images already use) and OpenAI-Compatible Chat
Completions (a new `input_audio` content part, `Wav`/`Mp3` only — the only two formats that API
accepts). Anthropic's Messages API and OpenAI's Responses API have no audio-input content block at
all, so both continue to reject `AudioMessageContent` via their existing "unsupported type" exception.
`ChatClientAdapter` maps Microsoft.Extensions.AI's `audio/*` `DataContent` the same way it already
mapped `image/*`.

### Breaking — `ToolOutput.Result` (string) replaced by `Content` (`MessageContent[]`)

A tool's result could only ever be a single opaque string, even when the underlying call produced
richer content (an image, a mix of text and media). `ToolOutput.Result` is gone; `ToolOutput.Content`
is an `IReadOnlyList<MessageContent>` instead. `Success`/`Failure` keep their `string?` overloads
(wrapping into a single `TextMessageContent`) and gain a `Success(IEnumerable<MessageContent>)`
overload for structured results.

`FunctionTool` now inspects the invoked method's return value: a `MessageContent` or
`IEnumerable<MessageContent>` (covariance covers `List<TextMessageContent>`, `MessageContent[]`, ...)
is carried through as-is; every other return type is still JSON-serialized into a single
`TextMessageContent`, unchanged from before.

All four `IMessageGenerator`s now translate `ToolOutput.Content` into their provider's native
tool-result wire shape where one exists: Anthropic's `tool_result` content blocks (text + image;
anything else degrades to a descriptive text block, since Anthropic has no other block types here)
and Gemini's `functionResponse` (text joined into the structured `Response`, image/audio content
added as `Parts[].InlineData` — genuinely multimodal). OpenAI's Responses API SDK exposes
`function_call_output.output` as a plain string only (confirmed on both 2.12.0 and 2.13.0 — the
only structured, multi-part tool-output item type is `ComputerCallOutputResponseItem`, specific to
the built-in computer-use tool, not general custom function tools), and this project's own
OpenAI-compatible Chat Completions wire DTO does the same for the `tool` role — both flatten
`Content` to text, joining `TextMessageContent`s and replacing anything else with a short
placeholder describing what was omitted.

### Breaking — `MessageToolChoice` renamed to `ToolChoice`, now a class hierarchy with multi-function support

`MessageToolChoice` (a sealed class wrapping a `MessageToolChoiceMode` enum) is now `ToolChoice`, an
abstract base with `AutoToolChoice`/`NoneToolChoice`/`RequiredToolChoice`/`FunctionToolChoice`
subclasses. `MessageGenerationRequest.ToolChoice`'s type follows. `ToolChoice.Function(...)` now takes
`params string[]` — `FunctionToolChoice.Names` can hold more than one name, forcing the model to one
of a named set rather than only ever a single specific function.

`ToolChoice` now reaches the wire on all four generators (previously only
`IronHive.Providers.OpenAI.Compatible` read it at all). Anthropic's `tool_choice`
(`ToolChoiceAuto`/`ToolChoiceNone`/`ToolChoiceAny`/`ToolChoiceTool`), GoogleAI's
`toolConfig.functionCallingConfig` (`AUTO`/`NONE`/`ANY`), and OpenAI's Responses API
`ResponseToolChoice` (`CreateAutoChoice`/`CreateNoneChoice`/`CreateRequiredChoice`) all map cleanly
for `Auto`/`None`/`Required`. A single-name `FunctionToolChoice` maps to Anthropic's `ToolChoiceTool`
and OpenAI's `ResponseToolChoice.CreateFunctionChoice`; OpenAI-Compatible sends
`{"type":"function","function":{"name":...}}` as before. Multi-name `FunctionToolChoice` is where the
wires diverge: GoogleAI's `functionCallingConfig.allowedFunctionNames` natively accepts a set of
names, so no extra work is needed there, but none of Anthropic's, OpenAI's, or OpenAI-Compatible's
`tool_choice` has a "one of these N" value — all three degrade to "require any tool"
(`ToolChoiceAny` / `CreateRequiredChoice()` / `tool_choice:"required"`) combined with the outgoing
tool catalog filtered down to just the named subset, the closest available approximation on those
wires.

(OpenAI's Responses API itself has an exact-match `tool_choice:"allowed_tools"` for this, but the
.NET SDK doesn't model it yet — see the comment in `OpenAIMessageGenerator.BuildOptions`.)

### Fixed — `FunctionTool` defaulted to a hard 60-second timeout on every call

`ToolOptions.Timeout` (the request-level, per-call timeout) already defaulted to unlimited, but
`FunctionTool.Timeout`/`FunctionToolAttribute.Timeout`/`FunctionToolFactory.DelegateDescriptor.Timeout`
independently defaulted to `60` (seconds) — so every `.NET`-method-backed tool silently failed after a
minute regardless of `ToolOptions.Timeout`. All three now default to `0`, meaning unlimited; `0` or
below skips the timeout race entirely. Set `Timeout` explicitly (on the tool, the attribute, or the
delegate descriptor) to restore a bound for a specific tool.

## 0.22.2 — 2026-09-08

### Fixed — `IronHive.Plugins.MCP`

- `McpSession.ConnectAsync`/`ReconnectAsync` no longer send a `ping` right after the
  `initialize` handshake. `McpClient.CreateAsync` already completes the handshake before
  returning, so the extra ping proved nothing about liveness; its only distinct effect was
  that a server which does not implement the `ping` utility moved the session to `Errored`,
  and because `McpClientManager` registers tools only from the `Connected` event, that
  server's entire toolset silently vanished with a state flag as the only signal.
  `HealthAsync` still pings — that is the explicit spec-level liveness check.
- `McpSession.ListToolsAsync` now moves the session to `Errored` (raising the `Errored`
  event) before rethrowing when the server fails `tools/list`. Previously the manager
  swallowed the fault and the session stayed `Connected` with zero tools and no event.

### Behaviour change

A server that rejects `ping` but completes `initialize` now **connects, and its tools are
registered** — previously that session went to `Errored` and the server contributed no tools.
Consumers that relied on the strict handshake check should call `HealthAsync`, which still
pings and still reports a non-compliant server.

## 0.22.1 — 2026-08-28

### Changed — `ModelContextProtocol` dependency

Updated `ModelContextProtocol` to 2.2.0 (previously 1.4.1). No public API changes — this
package's MCP integration is a client only (`McpClient.CreateAsync`/`ListToolsAsync`) with no
client capabilities configured, so it is unaffected by the capabilities the 2.0 protocol
revision deprecated (roots, sampling, logging).

## 0.22.0 — 2026-08-27

### Added — `IHiveService.GetMessageGenerator`/`GetEmbeddingGenerator`

A consumer registering providers through `HiveServiceBuilder` had no way to get back the raw,
provider-bound `IMessageGenerator`/`IEmbeddingGenerator` that `ChatClientAdapter`/
`EmbeddingGeneratorAdapter` (the M.E.AI bridge) require — `IHiveService.Messages`/`.Embeddings`
(`IMessageService`/`IEmbeddingService`) route by provider name per call and never hand out the
underlying instance. The only way to use the M.E.AI bridge was to construct a second, disconnected
provider instance by hand, duplicating whatever configuration the builder already held.

`IHiveService` now exposes:

```csharp
IMessageGenerator GetMessageGenerator(string? provider = null);
IEmbeddingGenerator GetEmbeddingGenerator(string? provider = null);
```

Both auto-select the sole registered provider when `provider` is omitted and exactly one is
registered, and throw `InvalidOperationException` when none or more than one is registered without
disambiguation — the same routing rule `IMessageService`'s internal lookup already used, now shared
via `IronHive.Core.Utilities.GeneratorLookup` so the two call sites cannot drift. An unregistered
`provider` throws `KeyNotFoundException`. `docs/ARCHITECTURE.md` and
`skills/ironhive/references/SERVICES.md`'s M.E.AI examples now use these instead of constructing a
second provider instance.

## 0.21.0 — 2026-08-26

### Added — `ChatOptions.ToolMode` now reaches the wire (`MessageGenerationRequest.ToolChoice`)

`Microsoft.Extensions.AI.ChatOptions.ToolMode` had no effect anywhere in the M.E.AI bridge:
`ChatClientAdapter.ConvertToRequest` never read it, and `MessageGenerationRequest` had no field
to carry it even if it had. A caller setting `ChatToolMode.None` to force a text-only response, or
`RequireAny`/`RequireSpecific` to force a tool call, produced the identical request as one that
left `ToolMode` unset — the model stayed free to decide either way, silently.
`MessageGenerationRequest` now exposes `ToolChoice: MessageToolChoice?` (new type,
`IronHive.Abstractions.Messages`), populated from `ChatOptions.ToolMode` in `ChatClientAdapter`.
`IronHive.Providers.OpenAI.Compatible` translates it into the OpenAI-compatible wire's
`tool_choice` field (`"none"`/`"required"`/`{"type":"function","function":{"name":...}}`); `None`
additionally omits the `tools` array from the request entirely, since some self-hosted backends
only partially honor `tool_choice` as a soft hint and still expose the tool grammar/schema
regardless of the field. Other providers do not yet read `ToolChoice` — unchanged (silently
ignored) behavior for those paths.

## 0.20.0 — 2026-08-24

### Added — `AgentInvokeOptions.Tools` (per-invoke tool override)

`IAgent.Tools` is set once at agent construction and, in consumers that cache/share `IAgent`
instances by name (e.g. Ironbees's `AgentRegistry`), mutating it directly to vary the tool set
per request would race across concurrent invocations of the same agent. `AgentInvokeOptions` now
exposes `Tools: IToolCollection?`, following the same per-request overlay convention as
`MaxTokens`/`Temperature`/etc.: when set, it overrides `IAgent.Tools` for that call only and
leaves the shared agent instance untouched; null keeps the agent's configured tools. `BasicAgent`
honors it in `CreateRequest`.

### Changed — `AIToolAdapter` (`IronHive.Core.Microsoft`) is now public and executes AIFunctions

Previously `internal` and declaration-only (`InvokeAsync` always threw `NotSupportedException`),
documented as existing solely so `ChatClientAdapter` could pass `ChatOptions.Tools` through to
`MessageGenerationRequest.Tools` for M.E.AI's own `FunctionInvokingChatClient` to execute.
Consumers integrating an M.E.AI `AIFunction`-shaped tool source (e.g. an MCP client's
`McpClientTool`) into IronHive's own tool-calling pipeline (`IHiveService.Messages`, or an
`AgentInvokeOptions.Tools` override) had no public path and had to hand-roll an equivalent
adapter. `AIToolAdapter` is now `public`, and `InvokeAsync` executes the wrapped `AIFunction`
(via `AIFunction.InvokeAsync`) when one is present, returning `ToolOutput.Failure` for a
declaration-only `AITool` or a thrown exception rather than leaking either as an unhandled
exception. `Parameters` now also recognizes any `AIFunctionDeclaration`, not just `AIFunction`.

## 0.19.1 — 2026-08-18

### Fixed — `AgentConfig.Tools`/`ToolOptions` were parsed but silently dropped when building an agent

`AgentService.CreateAgentFromConfig` (reached from `CreateAgentFromYaml`/`CreateAgentFromJson`/
`CreateAgentFromToml`/`CreateAgent(Action<AgentConfig>)`) never read `AgentConfig.Tools` or
`AgentConfig.ToolOptions` when constructing the resulting `BasicAgent` — a caller declaring
`tools: [...]` in YAML/JSON/TOML got a successfully-created agent whose `IAgent.Tools` was `null`,
so the declared tool silently never executed. No exception, no log.

`AgentConfigExtensions.Validate()` (already the single validation choke point for all four entry
points) now throws `NotSupportedException` when either field is populated, naming the fields and
pointing at the supported alternative (set `IAgent.Tools` directly on the constructed agent, or use
a framework with its own name-to-tool resolution, e.g. Ironbees's `AgentConfig.Tools` +
`IronhiveOptions.Tools`). `IronHive.Core` has no name-to-`ITool` registry of its own, so resolving
the names instead of rejecting them would require introducing one — out of scope for this fix and
not requested by any current consumer.

## 0.19.0 — 2026-08-11

### Fixed — GPUStack ignored `TokenLimitParameter`, always sending `max_completion_tokens`

`TokenLimitParameter` was added to `OpenAICompatibleConfig` and threaded through
`OpenAICompatibleMessageGenerator` to the inner Chat Completions generator, but `GpuStackConfig` never
gained the matching property and `GpuStackMessageGenerator` constructed its inner generator without
setting it — on both the initial construction and the resolver-driven swap in `GetOrUpdateInner()`. GPUStack
callers were pinned to the default `max_completion_tokens` with no way to switch to `max_tokens` for a
server that predates the rename, silently dropping the parameter server-side.

`GpuStackConfig.TokenLimitParameter` now exists with the same default (`MaxCompletionTokens`, no
regression) and reaches the inner generator at both construction sites, mirroring the sibling class.
`GpuStackMessageGenerator.EffectiveTokenLimitParameter` is added for the same observability reason as
the sibling's.

### Fixed — a keyless OpenAI-compatible provider no longer aborts service registration

`OpenAICompatibleConfig` documents the API key as optional, and the servers the package exists to
support — Ollama, LM Studio, vLLM, llama.cpp server — require no credential by default. Registering
one without a key nevertheless threw `ArgumentException: Value cannot be an empty string (Parameter
'key')` from `System.ClientModel`, because the model finder and the embedding generator are both
constructed eagerly by the registration helpers and reach `OpenAIClientFactory` with an empty key.
The documented registration examples were themselves affected, as was any caller reading a credential
from the environment with an empty-string fallback: the failure happened during `Build()`, before any
request, and named neither the provider nor the field.

An absent key is now carried as a placeholder credential. This is sent rather than dropped — requests
include an `Authorization` header with that value — so a server that rejects an unexpected credential
answers with an error that names it, instead of the previous crash before startup completed. A key
that is present is untouched, and the hand-rolled chat-completions path is unaffected because it never
used this factory.

`OpenAIConfig.Validate()` is unchanged but now documents what it actually answers: whether a
credential is present, not whether the configuration is usable. It is not a registration gate — a
keyless local server and a gateway that supplies the credential upstream are both valid without one.

### Fixed — MCP configuration equality was incomplete in both directions

`McpHttpClientConfig.Equals` omitted `OAuth`, so two configurations differing only in their credentials
compared equal. Both configurations also compared their collections — headers, arguments, environment — with
the default comparer, which is reference equality, so the same settings deserialised twice compared unequal.
The two mistakes point opposite ways and come from the same cause, and `GetHashCode` inherited both.

Equality now compares content: the OAuth redirect URI, client id, secret and scopes participate; headers and
environment compare by key and value; arguments compare in order, because command-line arguments are
position-sensitive. `GetHashCode` uses only the content-stable parts, so it cannot disagree with `Equals`.

Nothing gates on this today — the client manager reconnects unconditionally on update — so no behaviour
changes. An incomplete equality override is a trap for whatever starts to.

### Added — MCP transport mappings are asserted

The stdio transport takes a server name, a command and a working directory as three adjacent strings, so a
swap compiles and launches the wrong process, or the right one from the wrong place. The OAuth options take a
client id and a secret the same way. Both mappings are now built by named internal methods the tests assert,
with the transport construction doing nothing configuration-dependent, and both were verified by injecting
those swaps. The tests also pin that the transport mode is adapter policy rather than a vendor default, and
that absent optional fields stay absent instead of becoming empty collections.

### Changed — exception messages are English

Sixty-six exception messages across six assemblies were Korean. They are operator-facing at runtime and
part of what the packages ship, so a non-Korean-speaking operator could not triage a failure whose message
they could not read — and the reasoning that keeps Korean out of log pipelines applies equally to a message
pasted into an issue or a search: a Latin-boundary tokenizer indexes a Korean phrase as one opaque token,
and `grep` needs UTF-8-aware matching to find it.

Two of them named properties that do not exist. The Azure storage validation reported `ContainerName` and
`ShareName` as required; the property is `StorageName`, and the message now says which of the two roles it
plays. Several others now name the configuration property they are about (`AmazonS3Config.AccessKey`
rather than "an access key is required"), so the message points at the setting to change.

`ExceptionMessageLanguageConventionTests` keeps it that way. `LogLanguageConventionTests` already covers
`[LoggerMessage]` attributes by reflection, but exception messages are literals inside method bodies that
reflection cannot reach, so this reads the sources. Turning it on caught one more that a text search had
missed — a construction split across lines — and injecting Korean back into a message confirms the check
names the file, line and text. XML documentation is deliberately out of scope: it is Korean throughout by
established practice and is read at development time rather than emitted at runtime.

### Added — documentation drift is a test failure

The repository already required a document to be updated in the same commit as the surface it
describes. That rule was held by convention alone and was not kept: a single sweep found twenty-five
documented properties that no longer existed, across five guides, every one where a refactor had passed
through. `DocumentationConventionTests` now enforces it — each `new T { P = … }` in an example, and each
property in a documented type declaration, is resolved against the compiled assemblies by reflection.

Reflection rather than text matching is deliberate: a scratch regex pass over the same documents missed
two real failures on `ApiKeyCredential`, whose only member is a positional record parameter and so never
appears as a `{ get; }` declaration. The check also ignores string literals, which can contain what
looks exactly like an assignment (`"DefaultEndpointsProtocol=https;…"`), and lambda parameters (`ex =>`).

A third test asserts the check has documents and types to inspect at all. Both ways this guard could
fail silently — finding no documents, or resolving no type names — would otherwise read as a pass.

Known limits, deliberately not papered over: an initialiser containing a nested initialiser is skipped
rather than mis-parsed; a `new X { … }` naming a type that does not exist anywhere is skipped, because
that is indistinguishable from a vendor or framework type; and a type name that is ambiguous across
assemblies is skipped rather than checked against the wrong one.

### Documented — the OpenAPI plugin section described a different API

`docs/PLUGINS.md` had `OpenApiClientManager` constructed with no arguments and `AddOrUpdate` taking a
name and an options object carrying a spec URL, base URLs and a single credential. The manager takes an
`IToolCollection` and `AddOrUpdate` takes a constructed `OpenApiClient`; the options carry a *dictionary*
of credentials keyed by the spec's security-scheme name, default headers and a timeout, and nothing
else. Callers parse the specification themselves, and the request base URL comes from the spec's
`servers`.

The credential examples set `HeaderName` and `ApiKey` on `ApiKeyCredential`, which is a positional record
whose only member is `Value`, and named two types that do not exist (`BearerTokenCredential`,
`BasicAuthCredential`) in place of `HttpBearerCredential` and `HttpBasicCredential`. Where a key travels
— header, query, path or cookie — is decided by the specification, not the credential, which the section
now says. The tool-listing example called methods with the wrong names (`GetClient`, `GetToolsAsync`) and
an operation filter that does not exist.

### Documented — the service guide described a provider argument that lives elsewhere

`docs/SERVICES.md` showed `Provider` being set on the image, video and audio request objects. No such
property exists on any of them: the provider is a separate first argument to the service method. The
documented interfaces had the wrong method names too — `GenerateAsync`/`EditAsync`,
`TextToSpeechAsync`/`SpeechToTextAsync` instead of `GenerateImageAsync`/`EditImageAsync`,
`GenerateSpeechAsync`/`TranscribeAsync` — and omitted that parameter, so the signatures and the calls
were consistently wrong together and neither corrected the other.

The response shapes were wrong in the same direction: generated images and videos carry bytes, not a
`Url`, and a video response holds a single `Video` rather than a collection. Speech-to-text takes a
`GeneratedAudio`, not raw bytes with a `Language`, and its response exposes optional per-segment
transcription that was undocumented. The image message content example set `MediaType` and `Data`
where the type has `Format` (an `ImageFormat`) and `Base64`, and the search example repeated the
`TopK`/`Hits`/`Content` mistakes already corrected in the memory guide.

`docs/ORCHESTRATION.md` set `HandoffTarget.Condition`, which does not exist. The field is
`Description`, and it is not merely a label — the orchestrator renders it into the prompt as the basis
on which the model chooses a handoff target, so the guide now says that.

With this, every `new T { … }` example across `docs/` and `README.md` names only properties the types
actually have.

### Documented — storage configuration examples set properties the types do not have

Every registration example in `docs/STORAGES.md` and `docs/SETUP.md` that constructs a storage
configuration named at least one property that does not exist, so none of them compiled as printed:
`AmazonS3Config` takes `AccessKey` and `RegionCode`, not `AccessKeyId` and `Region`; `RabbitMQConfig`
takes `Host`, not `HostName`; `LocalVectorConfig` takes `DatabasePath` and `LocalQueueConfig`
`DirectoryPath`, not `Path`; and the Azure examples set `ContainerName` and `ShareName`, neither of
which exists — the container or share is `AzureStorageConfig.StorageName`.

That last one deserves the note it now carries: the registration call's first argument is also called a
storage name, but it is the logical name of the storage, while `StorageName` on the configuration is the
container or share. Swapping them creates the wrong container rather than failing. The Azure section
also now lists the `AuthType` alternatives, which were undocumented.

The queue example built a `MemoryContext` from the pre-refactor shape (`StorageName`,
`CollectionName`, `FilePath` as flat properties) and now constructs the real `Source`/`Target` pair.

### Documented — the memory-pipeline guide described an API that no longer exists

`docs/MEMORY.md` documented `MemoryContext` with nine flat properties — `SourceId`, `StorageName`,
`CollectionName`, `EmbeddingProvider`, `EmbeddingModel`, `FilePath`, `Text`, `Vectors`, `Metadata` — none
of which the type has. Those settings moved onto `IMemorySource` and `IMemoryTarget` implementations,
and the per-step data onto `Payload`. The custom-pipeline example was worse than incomplete: it declared
`Task<MemoryContext> ExecuteAsync(...)` returning the context, where the interface returns
`TaskStepResult`, and assigned `context.Text`, so the example as written did not compile.

The guide now describes the real shape — `Source`, `Target`, `Payload`, the polymorphic source and
target types, the documented target-narrowing step, and the `Payload` keys the built-in pipelines
exchange (`text`, `chunks`, `vectors`), each read from the pipeline that writes it. The example itself is
now a compiled test, so an interface change breaks the build rather than quietly outdating the prose.

`AnthropicConfig` in `docs/PROVIDERS.md` also misstated `ExtraHeaders` as `Dictionary` rather than
`IDictionary` and omitted `HttpClient`.

### Documented — `BaseUrl` means something different in the two OpenAI configurations

`OpenAIConfig.BaseUrl` is the complete endpoint: it reaches the vendor SDK verbatim and the adapter
adds no version segment, so `https://gateway.example.com` sends requests to `/responses` and `/models`
and every one of them 404s. `OpenAICompatibleConfig.BaseUrl` is the opposite — a server address, with
`Path` (default `/v1`) appended by the adapter. Two properties of the same name in the same provider
family with opposite contracts, and nothing but a 404 to tell them apart.

Neither is changed. A version segment is deliberately not synthesised for the plain configuration
because the rule differs per compatible service — GPUStack serves `/v1-openai` — and appending one
would corrupt paths the sibling configurations already build correctly. What was missing was the
statement of the contract, which is now in the XML documentation, in `docs/PROVIDERS.md`, and pinned at
the wire by `BaseUrlPathContractTests` rather than described only in prose.

The `OpenAIConfig` listing in `docs/PROVIDERS.md` also no longer describes members that do not exist
(`MaxRetries`), misstates the nullability of `BaseUrl`, or omits `Organization` and `Project`.

### Added — the `OpenAIConfig` to client mapping is asserted

`IronHive.Providers.OpenAI` had no test covering how its configuration reaches the vendor client, which
is the same blind spot that let a one-line base-URL misroute survive several releases in the Anthropic
adapter (0.18.0). Every field is now pinned to its own slot — endpoint, organization, project, network
timeout, transport — so a field routed elsewhere fails the build rather than a request.

### Fixed — samples no longer require every credential, and no longer name a private host

The console sample registered all four providers unconditionally with an empty-string fallback, so a
reader holding one API key could not run it: a provider that genuinely requires a credential rejects
an empty one during `Build()`. Providers are now registered only when their key is present. Both
samples also pointed their OpenAI-compatible provider at a private host; they now default to
`http://localhost:8080` and read `LOCAL_BASE_URL` instead.

## 0.18.0 — 2026-08-07

### Added — `GoogleAIConfig.Timeout` and `VertexAIConfig.Timeout`, and an explicit adapter default

Both configurations exposed the request timeout only through the vendor's `HttpOptions`, in
milliseconds, so a caller reading either one reasonably concluded the setting did not exist. They now
carry `TimeSpan? Timeout` alongside it, matching `AnthropicConfig`.

More importantly, the adapter now always supplies a value. Given none, the vendor SDK constructs a
bare `HttpClient` and keeps its 100-second default — which bounds the entire call for a non-streaming
request, and the wait for the first byte of a streaming one. Neither limit is announced anywhere, and
the resulting cancellation names no setting. `GoogleAIDefaults.Timeout` (ten minutes) is applied when
the configuration specifies nothing, so that default is never inherited silently.

Setting both `Timeout` and `HttpOptions.Timeout` throws rather than resolving by precedence. A
configuration that specifies the same thing twice in two units has no obvious winner, and a setting
that loses silently is the defect this change exists to remove. `HttpOptions` remains available for
everything else it carries, and a caller already using it for the timeout alone is unaffected.

**Behaviour change.** A Google AI or Vertex AI caller that configured no timeout previously had 100
seconds and now has ten minutes. Set `Timeout` explicitly to choose otherwise.

`docs/PROVIDERS.md` documents the new property; its Vertex example also had a `ProjectId` field that
does not exist on `VertexAIConfig`, now corrected to `Project`.

## 0.17.1 — 2026-08-07

### Fixed — `AnthropicConfig.BaseUrl` was assigned to the client's API key

`AnthropicClientFactory` wrote `BaseUrl` into `ClientOptions.ApiKey` and never assigned
`ClientOptions.BaseUrl`. Two consequences followed from that single line. A configuration pointing
at a proxy, gateway, or regional endpoint silently reached the vendor's default host, and the call
succeeded there with nothing in the configuration or the response indicating the substitution. And
because the key assignment is last-write-wins only when a key is present, a configuration that
authenticates with a bearer token — or relies on the vendor SDK's environment-variable fallback —
sent the base URL itself as the credential. The resulting authentication failure named neither
`BaseUrl` nor the factory.

**Regression teeth.** `AnthropicClientFactoryTests` asserts the config→client mapping directly
rather than compiling against it: a `BaseUrl`-only configuration must reach `BaseUrl` and must not
land in the credential slot. Nothing previously asserted that mapping, which is why a one-line
misroute survived several releases.

### Fixed — an injected `HttpClient` reintroduced a 100-second ceiling on time-to-first-byte

`OpenAICompatibleConfig.ToOpenAI()` and `GpuStackConfig.ToOpenAI()` construct an `HttpClient` in
order to set a connect timeout, and left `HttpClient.Timeout` at its 100-second default. That
default is applied ahead of the SDK's per-read network budget and wins, so `OpenAIConfig.TimeOut`
appeared to be ignored: a request whose first byte had not arrived within 100 seconds was cancelled
regardless of the configured value. Locally hosted OpenAI-compatible servers routinely exceed that
while loading a model or prefilling a long prompt, and the cancellation names neither the handler
nor the configured timeout. Both factories now disable the client-level timeout, matching what the
SDK's own default transport already does, which leaves `OpenAIConfig.TimeOut` as the single
effective ceiling. The connect timeout is unaffected, so an unreachable host still fails fast.

**Behaviour change, and only this one.** An endpoint that has not returned response headers within
100 seconds is now waited on until `OpenAIConfig.TimeOut` (ten minutes by default) instead of being
cancelled. Behaviour once headers have arrived is unchanged: the client-level timeout never bounded
the response body, which remains governed by the SDK's per-read network budget. Set `TimeOut`
explicitly to restore a shorter bound on time-to-first-byte.

`OpenAIConfig.HttpClient` now documents the same hazard for callers who supply their own instance.

## 0.17.0 — 2026-08-06

### Fixed — the `IChatClient` bridge dropped four of the five sampling parameters

`ChatClientAdapter` now forwards `ChatOptions.Temperature`, `.TopP`, `.TopK` and `.StopSequences`
to `MessageGenerationRequest`, on both the buffered and the streaming path. Previously only
`MaxOutputTokens` was mapped.

**What happened.** 0.15.0 restored the five sampling parameters to the request types and wired
them along the *agent* path (`AgentConfig.Parameters` → `BasicAgent` → providers). The
`Microsoft.Extensions.AI` bridge was not part of that change and kept mapping a single field, so
callers reaching IronHive through `IChatClient` — the standard MEAI entry point — had their
temperature silently ignored and sampled at the provider default. Same symptom as the 0.15.0
regression, one layer up: no error, no warning, only a response that does not honour the request.

**Behaviour change.** A caller that already sets `ChatOptions.Temperature` (or `TopP`/`TopK`/
`StopSequences`) will now see it applied where it was previously discarded. Provider coverage is
unchanged and still deliberately non-uniform — a provider that cannot accept one of these drops it
at its own adapter (Anthropic rejects `temperature`/`top_p`/`top_k` on models after Claude Opus
4.6), which is where that judgment belongs.

**Regression teeth.** Two structural tests replace the single-knob coverage that let this through:
`EveryDeclaredOptionKnob_ReachesItsRequestSink` drives every declared knob→sink pair through the
adapter one at a time, and `NoRequestSink_IsLeftUnmapped` fails when `MessageGenerationRequest`
gains a field `ChatOptions` already carries under the same name. `StopSequences` is copied rather
than aliased, so a caller mutating its own list after the call cannot change the request.

### Fixed — the embedding bridge reported the input count as a token count

`EmbeddingGeneratorAdapter` filled `GeneratedEmbeddings.Usage` with `InputTokenCount` =
`TotalTokenCount` = *the number of input strings*. Embedding two documents reported two tokens.
`EmbeddingResult` carries no usage information, so `Usage` is now left unset: a consumer feeding it
into cost or budget arithmetic is better served by "unknown" than by a confident wrong number.

### Fixed — a partial embedding batch was returned as if it were complete

`EmbeddingGeneratorAdapter` dropped results whose vector was null and returned the survivors.
`GeneratedEmbeddings` is positional — the caller matches `result[i]` to `input[i]` — so losing the
second of three embeddings did not return two of three results, it returned the *third* text's
vector under the second text's index, and every later pair shifted with it. A short list is still a
valid list, so nothing reported the mismatch; the visible outcome was a store quietly populated with
vectors attached to the wrong text.

`GenerateAsync` now throws `InvalidOperationException` when the provider returns a different number
of embeddings than there were inputs, naming both counts. A caller that needs per-input failure
detail should batch at a granularity where a failure is attributable.

### Fixed — `EmbeddingGenerationOptions.Dimensions` was accepted and never checked

A caller could request 512 dimensions, receive the model's native 1536, and be told nothing — after
which the vectors are silently incompatible with a store provisioned for the requested size.

`Dimensions` is documented as honoured *if supported*, so it is deliberately **not** rejected up
front: the request may well be satisfied by how the model or deployment is configured, and refusing
it a priori would break callers whose configuration already matches. Instead the request is now
compared against the vectors actually produced, and a mismatch throws `InvalidOperationException`
naming the requested and the actual size. Leaving `Dimensions` unset accepts the model's native size
exactly as before.

**Behaviour change.** A caller that sets `Dimensions` to a value this provider never honoured now
receives an error where it previously received differently-sized vectors in silence.

Callers reading `Usage` must now handle `null`. The previous value was not a coarse estimate of the
right quantity — it was a different quantity.

## 0.16.0 — 2026-08-06

### Added — the compatible provider chooses its output-length parameter

`OpenAICompatibleConfig.TokenLimitParameter` (and the same property on
`ChatCompletionMessageGenerator`) selects whether the request carries `max_completion_tokens` or
the pre-rename `max_tokens`. Default is `MaxCompletionTokens`, so existing behaviour is unchanged.

A server that predates OpenAI's rename does not reject the newer name — it ignores it. The limit
is therefore dropped in silence and the only symptom is a response longer than asked for. No
single name works everywhere, and the package cannot infer which one an arbitrary endpoint wants,
so this is a setting rather than a guess. `OpenAICompatibleMessageGenerator.EffectiveTokenLimitParameter`
exposes what the generator will actually send.

## 0.15.0 — 2026-07-28

### Fixed — sampling parameters reach the provider again (regression from 0.11.0)

`Temperature`, `TopP`, `TopK` and `StopSequences` are restored to `MessageRequest`,
`MessageGenerationRequest` and `AgentInvokeOptions`, and are wired from `AgentConfig.Parameters`
through `BasicAgent` to the providers.

**What happened.** Commit `1b38998` ("refactor(messages): simplify request parameter model",
2026-06-30) removed the `MessageGenerationParameters` base class and folded **only `MaxTokens`**
onto the request types. The other four were dropped. `AgentParametersConfig` kept exposing all
five and the TOML parser kept reading them, so an agent configured with `temperature = 0.2`
silently sampled at the provider default — a no-op with no error and no warning. Separately,
`IronHive.Flux`'s adapters stopped compiling against the removed properties, which is how the
regression surfaced.

**Provider coverage** — deliberately not uniform:

| Provider | Temperature / TopP | TopK | StopSequences |
|---|---|---|---|
| OpenAI | ✅ | — (not in the Responses API) | — |
| OpenAI.Compatible | ✅ | ✅ (`top_k`, ignored by servers that don't know it) | ✅ (`stop`) |
| GoogleAI | ✅ | ✅ | ✅ |
| Anthropic | **intentionally not forwarded** | **not forwarded** | ✅ |

Anthropic deprecated `temperature`/`top_p`/`top_k`; models released after Claude Opus 4.6 reject
any value with a 400. Forwarding them would convert a silent no-op into a hard request failure,
so they are dropped at the Anthropic adapter with a comment explaining why.

Regression guard: `tests/IronHive.Tests/Agent/SamplingParameterFlowTests.cs`.

## 0.14.0 — 2026-07-22

`MessageRequest` kept growing per-request options (`Suggestions`,
`ThinkingEffort`, `OutputFormat`, …) that agent-abstraction consumers could
never reach — `IAgent.InvokeAsync` had no options parameter, so wrappers like
Ironbees had nothing to pass through. This release opens that channel.

### Added

- **`AgentInvokeOptions`** (`IronHive.Abstractions.Agent`) — per-request
  options for `IAgent.InvokeAsync/InvokeStreamingAsync`: `PreviousId`,
  `ThinkingEffort`, `MaxTokens` (overrides the agent default), `ToolOptions`,
  `OutputFormat`, `Suggestions`, `MaxTurns`, and `Items`. `BasicAgent`
  overlays them on top of its agent-fixed defaults; null fields keep defaults.
- **Field-symmetry regression test** — every writable `MessageRequest`
  property must be classified as agent-fixed (`Provider`/`Model`/`System`/
  `Messages`/`Tools`) or exposed on `AgentInvokeOptions`, so future
  per-request options cannot silently become unreachable again.

### Changed (breaking, 0.x)

- **`IAgent` invoke methods** gain an optional `AgentInvokeOptions?` parameter
  before the cancellation token. Source-compatible for callers using named or
  omitted arguments; implementers must add the parameter. Positional
  `InvokeAsync(msgs, ct)` call sites need `InvokeAsync(msgs, null, ct)`.
- **`IAgentMiddleware`/`IStreamingAgentMiddleware`** receive `options` and a
  two-argument `next(messages, options)` — middleware must forward options.
- **`CachingMiddleware`** includes options in its cache key: identical
  messages with different per-request options no longer share a cache entry.
- **`OrchestratorAgentAdapter`** throws `NotSupportedException` when passed
  non-null options (fail-loud instead of silently ignoring them) — configure
  member agents or orchestrator options instead.

## 0.13.0 — 2026-07-14

`MessageService`'s ad-hoc turn state (four scattered locals threaded through a
do-while loop) became the blocker for giving `IMessageMiddleware` real turn
visibility, so it's now a proper `MessageContext`. Alongside: a naming
collision fix on structured-output config, and rate-limit errors join
context-overflow in the normalized exception taxonomy.

### Added

- **`RateLimitException`** (`IronHive.Abstractions.Exceptions`) — HTTP 429 /
  rate-limit errors from OpenAI, Anthropic, Google AI, and OpenAI-compatible
  backends (vLLM/GPUStack/llama.cpp) now normalize to this type instead of
  leaking as raw provider exceptions, mirroring `ContextOverflowException`'s
  per-provider mapping. Carries an optional `RetryAfter` when the provider
  exposes one (`retry-after` header, Gemini `RetryInfo.retryDelay`).
  `IronHive.Providers.OpenAI.Compatible`'s `ContextOverflowDetector` is
  renamed `ChatCompletionExceptionDetector` and split into a response-driven
  `DetectAsync` and a message-only `Detect`, since rate-limit detection needs
  the HTTP status code, not just the error body.
- **`MessageContext`** (`IronHive.Abstractions.Messages`) — the per-call state
  `MessageService`'s turn loop and `IMessageMiddleware` chain now share:
  `Request`, `MaxTurns`, `CurrentTurn`, `TrackedId`, `TurnReason`,
  `TokenUsage`, `CurrentMessage`, `Elapsed`, and `Items`. Construction and turn
  bookkeeping (`BeginTurn`/turn-state setters) stay internal to `MessageService`
  so middleware can read turn state but can't forge it to bypass loop control.
- **`MessageContextItems`** (`IronHive.Abstractions.Messages`) — a
  `Dictionary<string, object?>`-backed data bag flowing
  `MessageRequest.Items` → `MessageContext.Items` →
  `MessageResponse.Items`/`StreamingMessageDoneResponse.Items`, for middleware
  to pass data across turns or back out to the caller.

### Changed

- **`IMessageMiddleware.GenerateAsync`/`GenerateStreamingAsync` now take
  `MessageContext` instead of `MessageGenerationRequest`.** Existing
  middleware needs no request-shape changes (`context.Request` is the same
  `MessageGenerationRequest` as before) but gains access to turn state
  (`CurrentTurn`, `TokenUsage`, etc.) without a `MessageRequest` reference.
- **`MessageRequest.MaxLoopCount` renamed to `MaxTurns`.**
- **`OutputOptions` renamed to `OutputFormat`**, collapsing its mutually
  exclusive `Type`/`Schema(string)` fields into a single `JsonNode Schema`
  computed once via `For<T>()`/`For(string)`/`For(JsonNode)`/`For(JsonElement)`.
  `OutputOptions` collided in name/meaning with
  `ToolOutput`/`ToolMessageContent.Output` elsewhere in the codebase. This
  removes duplicated Type-vs-Schema branching from all four provider
  generators; Anthropic no longer delegates to the Anthropic SDK's
  reflection-based `StructuredOutput.CreateJsonFormat<T>()` and instead
  applies `AnthropicHelper.ToAnthropicCompatibleSchema`'s compatibility
  transform (`additionalProperties: false`, nullable-union flattening)
  directly to the shared `JsonNode` schema.
- **`MessageResponse.Model`** no longer gets `{provider}/{model}` formatting —
  it's now just `request.Model` as supplied by the caller.
- **Tool execution split** in `MessageService` into `ExecuteToolAsync` (shared
  per-tool logic), `ExecuteToolsAsync` (non-streaming), and
  `ExecuteStreamingToolsAsync` (streaming, progress events) — was one method
  handling both paths.

### Removed

- **`LimitedCounter`** (`IronHive.Core.Utilities`) — unused after the turn
  loop moved to `MessageContext.CurrentTurn`/`MaxTurns`.

## 0.12.0 — 2026-07-13

Follow-up to 0.11.0's `IMessageMiddleware`: a real consumer (vault-ai's
context-compaction middleware) needed to signal its own out-of-band events
mid-stream, which `MessageService` couldn't carry. Also dropped an `ITool`
JSON-polymorphism mechanism that turned out to have no actual caller.

### Changed

- **`MessageService.GenerateStreamingMessageAsync` now passes through
  unrecognized `StreamingMessageResponse` types instead of throwing.**
  Previously any `IMessageMiddleware` that yielded a response type outside
  the fixed set (`Begin`/`Error`/`ContentAdded`/`ContentDelta`/
  `ContentUpdated`/`ContentCompleted`/`Done`) crashed the pipeline with
  `InvalidOperationException("Unexpected response type.")`. `StreamingMessageResponse`
  is a plain (non-sealed) `abstract class`, so a middleware can now define
  its own subtype and `yield return` it directly as a real-time signal to
  the caller — e.g. a compaction middleware emitting "compacting
  started"/"compacted" events at the moment they happen, rather than
  smuggling them through an out-of-band callback that the caller has to
  poll for on every subsequent chunk. All previously-known types are
  handled exactly as before; this only changes behavior for types that used
  to throw.

### Removed

- **`PolymorphicJsonConverter<T>`, `JsonPolymorphicNameAttribute`,
  `JsonPolymorphicValueAttribute`** (`IronHive.Abstractions.Json`) — a
  hand-rolled polymorphic-JSON mechanism used only by `ITool` (via
  `FunctionTool`/`McpTool`). No call site anywhere in IronHive or its
  consumers ever actually serializes an `ITool`-typed value through
  `JsonSerializer`; the attributes were a declared-but-unexercised
  contract. `ITool`, `FunctionTool`, `McpTool` no longer carry the
  `[JsonConverter]`/`[JsonPolymorphicName]`/`[JsonPolymorphicValue]`
  attributes.

## 0.11.0 — 2026-07-10

Two threads: a generic interception point for the message-generation loop
(requested to control tool-invoke input/output and wrap generator calls for
retry/compaction/error-handling), and a correctness pass on the 0.9.0
context-overflow exception mapping after verifying each provider's actual
SDK/error shape via reflection instead of assumption.

### Added

- **`IMessageMiddleware`** (`IronHive.Abstractions.Messages`) — a next-chain
  middleware wrapping `MessageService`'s generator calls, both streaming and
  non-streaming (default-interface pass-through, so a middleware can opt into
  just one). Register globally via `HiveServiceBuilder.AddMessageMiddleware()`;
  `MessageService` composes the chain once per call and drives every
  loop iteration through it, so a middleware can inspect/mutate the request
  before each round (e.g. compact history) or wrap `next()` in try/catch for
  retry/error-handling.
- **`ToolOptions.OnBeforeInvoke` / `OnAfterInvoke`** — delegates receiving the
  full `ToolMessageContent` (not just name/output) around each tool
  invocation. `OnBeforeInvoke` can short-circuit the real tool call by
  pre-filling `Output`. Replaces `OutputTransform`.
- **`GoogleAIExceptionMapper`** — Gemini API context-window overflow
  (`ClientError { StatusCode: 400, Status: "INVALID_ARGUMENT" }`, message
  "input token count (X) exceeds the maximum number of tokens allowed (Y)")
  was previously undetected entirely; now maps to `ContextOverflowException`
  like the other three providers.

### Changed

- **Provider exception mappers hardened to real SDK types**, found via
  reflecting the pinned `Anthropic`/`OpenAI`/`Google.GenAI` package
  assemblies rather than assuming `.Message` shape:
  - Anthropic now gates on `AnthropicApiException { ErrorType:
    ErrorType.InvalidRequestError }` (was the base `AnthropicException`
    catch-all) and reads `ResponseBody` instead of `.Message` (which the SDK
    prefixes with `"Status Code: {code}"`).
  - OpenAI detects via `ClientResultException.Message`, which the SDK embeds
    `error.code`/`error.message` into for the Responses API. The Responses
    API's real overflow text carries no token counts, so
    `ContextOverflowException.ContextWindow` is expected to be null there;
    the legacy Chat-Completions-style numeric regex is kept only as a
    best-effort fallback.
- **Renames**, all under `IronHive.Abstractions`/provider assemblies:
  - `ContextWindowExceededException` → `ContextOverflowException`.
  - `ExceptionMappingExtensions` → `ExceptionExtensions`;
    `MapExceptions` → `MapException` (singular — it maps the one exception a
    call threw).
  - Each provider's `ContextWindowErrorMapper` → `{Provider}ExceptionMapper`
    with a single `Map(Exception)` entry point (collapsed the previous
    `Map`/`Detect`/`ExtractErrorCode` split), structured so a matched error
    returns from inside an `if` and the method always falls through to
    `return null` — meant to make adding another error category later just
    another sibling `if` block.
  - `IronHive.Providers.OpenAI.Compatible`'s `ContextOverflowDetector` →
    `ChatCompletionExceptionDetector`, now returns the `HiveException` base
    type (not `ContextOverflowException` specifically) for the same
    future-extensibility reason.
- **`ToolOutputFilter`** moved out of `IronHive.Core.Tools` into
  `IronHive.Core.Utilities.TextCompactor` — it was designed to attach to
  `ToolOptions.OutputTransform` specifically; now that hook is gone, its
  JSON→CSV/whitespace/truncation algorithms are exposed as plain
  `string`-in/`string`-out utility functions decoupled from `ToolOutput`, for
  callers to wire into `OnAfterInvoke` (or anywhere else) themselves.

### Breaking

- **`ContextPolicy` / `IMessageCompactor` / `MessageRequest.ContextPolicy`
  removed** (shipped 0.9.0–0.10.0). The merge that landed 0.10.0 had a
  conflict in `MessageService.cs` that was resolved in favor of
  `IMessageMiddleware`, silently dropping `ContextPolicy`'s actual
  enforcement code — the `Abstractions` types and `MessageRequest` property
  survived the merge unused. Rather than re-wire a parallel hook mechanism,
  proactive budget/compaction is now just another `IMessageMiddleware`:
  inspect `MessageGenerationRequest`/token usage before calling `next()`.
- **`ContextOverflowException.PromptTokens` and `IsPreflightRejection`
  removed.** `PromptTokens` is dropped entirely — `ContextWindow` is the only
  field now (OpenAI's Responses API never reports prompt token counts on
  overflow, so the field was unreliable across providers anyway).
  `IsPreflightRejection` was only ever set by `ContextPolicy`'s preflight
  check, above.
- **`ContextOverflowException.ContextWindow`** is a plain settable property
  (was `init`-only).
- **`ToolOptions.OutputTransform` removed**, replaced by
  `OnBeforeInvoke`/`OnAfterInvoke` above. Callers wiring `ToolOutputFilter`
  should call `TextCompactor.Compact(...)` from within `OnAfterInvoke` instead
  (see `docs/TOOLS.md`).

## 0.10.0 — 2026-07-07

Pipeline-state completion of the 0.9.0 `ContextPolicy` surface (vault-ai
dogfooding — a persistence-aware `IMessageCompactor` cannot compute its
store-relative summary boundary without knowing which messages the pipeline
added mid-loop, nor merge correctly when compaction fires twice in one request).

### Added

- **`MessageCompactionContext.OriginalMessageCount`** (required) — the number of
  messages present at request start. On the first compaction, everything past
  this index in `Messages` was appended by the tool loop and is not yet in the
  consumer's store.
- **`MessageCompactionContext.PreviousCompactedMessages`** — the message list
  returned by the immediately preceding `CompactAsync` in the same request
  (null on first compaction), so a second compaction can build on the first
  instead of re-summarizing its own output or merging against a stale baseline.
- **Documented pipeline invariant** — the pipeline only appends after the
  baseline list (original or previously compacted messages); it never reorders,
  clones, or mutates existing messages. Both new properties derive their
  boundary semantics from this now-explicit contract. Also documented: the
  compactor may fire multiple times per request (sequentially), so per-request
  state must come from the context, not compactor instance fields.

### Breaking

- Constructing `MessageCompactionContext` now requires `OriginalMessageCount`.
  Only affects code that builds the context manually (e.g. compactor unit
  tests); the pipeline always supplies it. A defaultable property was rejected
  because a silent `0` reproduces the exact wrong-boundary defect class this
  release removes.

## 0.9.0 — 2026-07-05

First slice of the domain exception taxonomy: context-window overflow errors are
now typed instead of leaking as raw provider strings (vault-ai dogfooding — a 32k
local model receiving a 42k-token request permanently wedged the session because
consumers had no way to detect the overflow without string parsing).

### Added

- **`IronHive.Abstractions.Exceptions`** — new `HiveException` base type and
  `ContextWindowExceededException` (`PromptTokens`, `ContextWindow`,
  `IsPreflightRejection`). Providers normalize their vendor-specific overflow
  errors to this type; consumers can `catch` it and compact/truncate/re-route.
- **Provider mapping** across all three built-in surfaces, non-streaming and
  streaming (including GPUStack mid-stream `error:` lines):
  - `IronHive.Providers.OpenAI.Compatible` — llama.cpp/GPUStack
    `exceed_context_size_error` (with `n_prompt_tokens`/`n_ctx` extraction) and
    vLLM/OpenAI-compatible `context_length_exceeded`.
  - `IronHive.Providers.OpenAI` — SDK `ClientResultException` with
    `context_length_exceeded` / "maximum context length".
  - `IronHive.Providers.Anthropic` — SDK errors with
    "prompt is too long: X tokens > Y maximum".
- **`ExceptionMappingExtensions`** (`IronHive.Abstractions.Extensions`) —
  provider-neutral `Task<T>.MapExceptions(...)` / `IAsyncEnumerable<T>.MapExceptions(...)`
  helpers that providers use to translate SDK exceptions at the call boundary.
- **`MessageRequest.ContextPolicy`** (opt-in) — proactive input-token budget
  enforcement before every provider call, including each tool-loop iteration.
  `MaxInputTokens` is consumer-supplied (model metadata lookup stays an app-layer
  concern, e.g. TokenMeter); estimation uses the provider's `CountTokensAsync`
  first and an opt-in `FallbackEstimator` for providers that don't support
  counting (an active policy with no estimation path is an explicit
  configuration error, never a silent no-op). `OnOverflow`:
  - `Fail` (default) — throws `ContextWindowExceededException`
    (`IsPreflightRejection = true`) before any network call.
  - `Compact` — delegates to a consumer-supplied `IMessageCompactor`
    (summarize/persist strategy is app domain; no default implementation),
    re-checks, and fails if still over budget.
  Message truncation is deliberately not provided: naive oldest-first dropping
  can break tool_use/tool_result pairing invariants and no consumer demands it.

### Breaking

- For the overflow case only, `IronHive.Providers.OpenAI.Compatible` now throws
  `ContextWindowExceededException` where it previously threw
  `HttpRequestException`; other errors are unchanged. Consumers catching
  `HttpRequestException` specifically to detect overflow should catch the new
  type instead.

## 0.8.3 — 2026-07-03

Reverts the 0.8.2 split-surface design in favor of dedicated generators per
package, then moves the Chat Completions generator off the OpenAI SDK onto a
raw HTTP/JSON client so vendor reasoning fields are actually reachable.

### Breaking

- **`OpenAIApiSurface` enum and `OpenAIConfig.Api` removed.** No more surface
  selection: `IronHive.Providers.OpenAI` implements the Responses API only
  (`OpenAIMessageGenerator`); `IronHive.Providers.OpenAI.Compatible`
  (including GPUStack) owns Chat Completions outright via
  `ChatCompletionMessageGenerator`, since each package is now scoped to a
  single wire protocol.
- **`OpenAIResponseMessageGenerator`/`OpenAIChatMessageGenerator` removed.**
  `OpenAIMessageGenerator` is a direct Responses API implementation again
  (not a dispatcher), and the Chat Completions implementation lives at
  `IronHive.Providers.OpenAI.Compatible/ChatCompletion/ChatCompletionMessageGenerator`.

### Changed

- **`ChatCompletionMessageGenerator` moved off the OpenAI SDK onto a raw
  HTTP/JSON client** (`ChatCompletion/ChatCompletionHttpClient.cs`). The
  SDK's typed models have no slot for `reasoning_content` (emitted by
  Ollama/vLLM-style reasoning models) and expose no raw-JSON escape hatch
  during streaming ([openai-dotnet#813](https://github.com/openai/openai-dotnet/issues/813)),
  so those vendor fields were unreachable.
  - **`ExtraBodyJsonConverter`/`ExtraBodyJsonConverterFactory`** deep-merge
    vendor request extensions (`thinking_token_budget`,
    `chat_template_kwargs`) and recover unknown response fields such as
    `reasoning_content`.
  - **`ThinkingEffort`** maps to `reasoning_effort` plus vendor-specific
    overrides for Qwen/DeepSeek/Granite-style hybrid reasoning models.
  - **GPUStack's bare `error: <message>` SSE line** (not an HTTP error) is
    now handled during streaming.
- **`MessageService`** copies `request.Messages` into a new `List<Message>`
  before mutating it in the send/stream loops, instead of mutating the
  caller's list in place.
- Bumped `Anthropic` 12.34.0 → 12.35.1, `OpenAI` 2.11.0 → 2.12.0, `AWSSDK.S3`
  4.0.100 → 4.0.100.2, `Tomlyn` 2.9.0 → 2.10.1.

### Notes

- `README.md`/`docs/PROVIDERS.md` updated: `IronHive.Providers.OpenAI` is
  documented as Responses-API-only; Chat Completions coverage moved to
  `IronHive.Providers.OpenAI.Compatible`.
- ConsoleApp sample updated to exercise the compatible provider alongside
  OpenAI/Anthropic/GoogleAI.

## 0.8.2 — 2026-07-03

Fixes a silent runtime break introduced in 0.7.9 where the entire OpenAI provider
family routed chat through the OpenAI-proprietary **Responses API**
(`POST /v1/responses`), returning `404 Not Found` on every Chat-Completions-only
endpoint (self-hosted / OpenAI-compatible servers). Reported by Filer while
consuming iron-prow 0.1.1. The switch was intended for first-party OpenAI only;
`OpenAICompatible`/GPUStack delegating to the Responses generator was the defect.
Still present in 0.8.0/0.8.1 — the structural refactor in those releases did not
touch the OpenAI provider files.

### Added

- **`OpenAIApiSurface` enum** and **`OpenAIConfig.Api`** selector — choose between
  `ChatCompletions` (`/v1/chat/completions`) and `Responses` (`/v1/responses`).
- **`OpenAIChatMessageGenerator`** — Chat Completions implementation over the
  official OpenAI SDK (`GetChatClient`). Covers text, tools, images, streaming,
  and local token estimation.
- **`OpenAIResponseMessageGenerator`** — the prior Responses implementation,
  extracted from `OpenAIMessageGenerator` (unchanged behavior).

### Changed

- **`OpenAIMessageGenerator` is now a dispatcher** that routes to the surface
  selected by `OpenAIConfig.Api`. Constructors are unchanged, so existing
  registrations keep working: first-party OpenAI defaults to `Responses`.
- **`AddOpenAICompatibleProviders` and the GPUStack provider now default to Chat
  Completions.** Their `ToOpenAI()` sets `Api = ChatCompletions`, so they target
  `/v1/chat/completions` — the surface Ollama, LM Studio, vLLM, llama.cpp server,
  and GPUStack implement. This is the fix for the 404 regression.

### Notes

- The Chat Completions surface has no reasoning-input block: prior assistant
  `thinking` content is not replayed, and `ThinkingEffort` is not mapped there
  (compatible servers generally reject `reasoning_effort`). Use the `Responses`
  surface for reasoning.
- Per-provider API surfaces are documented in `docs/PROVIDERS.md` and `README.md`.

## 0.8.1 — 2026-07-02

Follow-up hardening to the `Files` parsers introduced in 0.8.0.

### Breaking

- **`IFileParser.CanParse` drops the unused `mimeType` parameter** —
  `CanParse(fileName)`. `FileParserService` never passed it.
- **`ExcelParser`/`WordParser`/`PowerPointParser` now throw
  `InvalidOperationException`** on missing required parts (workbook, document
  body, slide list) instead of silently returning an empty block list.
- **Parsers no longer swallow exceptions.** The blanket try/catch around each
  `ParseAsync` is removed; malformed documents now propagate exceptions
  instead of failing silently. Per-image extraction still fails individually.

### Added

- **`WordParser`/`PowerPointParser` extract embedded images** as `ImageBlock`,
  matching `PdfParser`.

## 0.8.0 — 2026-07-01

This release is a sweeping structural refactoring across tools, services, models,
MCP, and files. Almost every public API surface has changed. **Every consumer of
IronHive must update call sites before upgrading.**

### Breaking

#### Tools pipeline

- **`ToolItem` removed.** `MessageRequest.Tools` and `IAgent.Tools` are now
  `IToolCollection?` instead of `IEnumerable<ToolItem>?`. Remove all `ToolItem`
  construction; pass an `IToolCollection` directly.
- **`FromOptionsAttribute` removed.** Parameter binding from an options object is
  no longer supported by `FunctionToolFactory`.
- **`ToolInput.Options` removed.**
- **`IHiveServiceBuilder` registration helpers removed:** `AddTool`,
  `AddWorkflowStep`, `AddToolInitializer`, `AddFunctionTool<T>` (and the MCP /
  OpenAPI builder extensions that relied on them). Use
  `ToolCollectionExtensions.AddFunctionTool<T>` on `IToolCollection` at call site.
- **`ToolInput.Services` removed.** `ToolInput` is now a pure data object.
  `FunctionTool` receives `IServiceProvider` at construction via
  `FunctionToolFactory`.
- **`IToolOutputFilter` interface deleted.** Use `ToolOutputFilter` (the
  standalone utility class) directly, or supply the new
  `ToolOptions.OutputTransform` delegate.
- **`ToolOptions.OutputFilter` renamed to `OutputTransform`** (`Func<string,
  ToolOutput, ToolOutput>?`).
- **`MessageService` constructor simplified** — accepts only generators; holds no
  external dependencies.

#### Service container

- **`IHiveService.Workflows` removed.** `WorkflowFactory` is no longer managed by
  `HiveService`.
- **`HiveServiceBuilder.Build()` no longer creates an internal
  `ServiceCollection`.** `CompositeServiceProvider` is deleted; callers must
  supply `IServiceProvider` explicitly where needed (e.g., `CreateMemoryWorker`).

#### Models

- **`ModelSpec` family renamed to `ModelCard`.**
  `IModelSpec` → `IModelCard`, `ModelSpecList` → `ModelCardList`,
  `ChatModelSpec` → `LanguageModelCard`, `EmbeddingModelSpec` →
  `EmbeddingModelCard`, `GenericModelSpec` → `ModelCard`.
- **Compatibility adapters removed:** `ChatClientAdapter`,
  `EmbeddingGeneratorAdapter`, `AIToolAdapter` and their tests are deleted.
  The `Microsoft/` integration layer is preserved under `IronHive.Core`.
- **`IronHiveTelemetry` renamed to `HiveTelemetry`** (moved to
  `IronHive.Core/Utilities/`).
- **`AssemblyExtensions` and `HttpMessageExtensions` removed.**

#### MCP

- **`McpSseClientConfig` renamed to `McpHttpClientConfig`.** Update all config
  construction and DI registration.

#### Files

- **`IFileDecoder<T>`, `IFileExtractionService<T>`, `IFileMediaTypeDetector`
  removed.** Replace with `IFileParser` / `IFileParserService`.
- **`FileExtractionService` removed.** Use `FileParserService`.
- All `Decoder` and `Detector` implementations under `Files/Decoders/` and
  `Files/Detectors/` are deleted.

#### Resilience / Streaming

- **`Resilience` namespace removed:** `ResilienceOptions`, `ResiliencePipelineFactory`,
  `ResilientMessageGenerator` deleted.
- **`Streaming` namespace removed:** `IStreamState`, `IStreamStateManager`,
  `StreamStateOptions`, `InMemoryStreamStateManager`, `ResumableStreamingGenerator`,
  `StreamState` deleted.
- **`DelegatingMessageGenerator` removed** from `IronHive.Abstractions`.

### Added

- **`ToolCollectionExtensions`** (`IronHive.Core.Extensions`): `AddFunctionTool<T>`,
  `AddFunctionTool(instance)`, `AddFunctionTool(delegate)` — replaces the old
  builder-level helpers; attach tools to an `IToolCollection` at the call site.
- **`FunctionTool` DI injection.** `FunctionToolFactory` now accepts
  `IServiceProvider?` on all `CreateFrom` overloads; the provider is captured by
  the tool instance rather than flowing through `ToolInput`.
- **`IFileParser` / `IFileParserService`** with five built-in parsers:
  `PdfParser`, `WordParser`, `ExcelParser` (new), `PowerPointParser`,
  `ImageParser`. Unsupported files are classified as text or binary via a
  null-byte heuristic.
- **`FileBlock`** (`TextFileBlock`, `ImageFileBlock`, `BinaryFileBlock`) with
  `JsonPolymorphic` / `JsonDerivedType` attributes for serialization.
- **`McpHttpClientConfig`** adds OAuth 2.0 support for MCP HTTP streams.
- **`HiveTelemetry`** replaces the old `IronHiveTelemetry`.

## 0.7.9 — 2026-07-01

This release first publishes a series of message-contract refactorings that had
landed on `main` after 0.7.8 (`f88b366`) but were never version-bumped, so they
were absent from nuget. **It is a breaking release for any consumer of the
message/tool abstractions** (ironhive-agent, iron-prow, ironhive-host,
ironhive-flux, ironbees).

### Breaking

- **Message type hierarchy flattened.** `Roles/AssistantMessage` and
  `Roles/UserMessage` subclasses are removed; their data is folded into
  `Message`. Code that pattern-matches on those subtypes must switch on
  `Message` and its `Role`.
- **Request parameter model simplified.** `MessageGenerationParameters` is
  removed and replaced by focused option records: `OutputOptions`,
  `ToolOptions`, and `SuggestionOptions` on the request.
- **Tool-limit validation removed.** `ToolLimitBehavior`, `ToolLimitValidation`,
  and `ToolLimitValidationExtensions` are deleted.
- **Tool output filter renamed and promoted to Abstractions.**
  `IToolResultFilter` → `IToolOutputFilter` (now in `IronHive.Abstractions.Tools`);
  `ToolResultFilter` → `ToolOutputFilter`; `ToolResultFilterOptions` →
  `ToolOutputFilterOptions`.
- **New interface members.** `CountTokensAsync` is added to `IMessageGenerator`
  and `IMessageService`; custom implementers must implement it.

### Added

- **Suggestion extraction.** `Suggestion`, `SuggestionOptions`, and
  `SuggestionCollector` support extracting follow-up suggestions from a
  generation, opt-in via `SuggestionOptions` on the request.
- **`CountTokensAsync`** across all built-in providers (OpenAI, Anthropic,
  GoogleAI, GPUStack).
- **Generic OpenAI-compatible provider** (`IronHive.Providers.OpenAI.Compatible`):
  `AddOpenAICompatibleProviders(name, config, serviceType)` targets any
  OpenAI-compatible HTTP endpoint (Ollama `:11434`, LM Studio `:1234`,
  vLLM `:8000`, llama.cpp server) by host:port with a shared `/v1` path.
  `OpenAICompatibleConfig` is key-optional (`IsUsable` vs `IsConfigured`) for
  LAN services and idempotently appends the path. GPUStack keeps its dedicated
  provider for its `/v1-openai/` quirk.

### CI

- CI now also triggers on `tests/**` and `.github/workflows/ci.yml` changes, and
  supports `workflow_dispatch`. Previously the `src/**`-only push filter let a
  test-only fix land without re-running CI, producing a stale red badge over a
  green tree.
