# Changelog

All notable changes to IronHive are documented here. Pre-1.0 (0.x): breaking
changes are expected and used freely for structural correctness (see
`docs/CONSTITUTION.md`).

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
