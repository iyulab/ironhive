# Changelog

All notable changes to IronHive are documented here. Pre-1.0 (0.x): breaking
changes are expected and used freely for structural correctness (see
`docs/CONSTITUTION.md`).

## Unreleased

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
