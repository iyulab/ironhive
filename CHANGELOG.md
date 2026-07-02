# Changelog

All notable changes to IronHive are documented here. Pre-1.0 (0.x): breaking
changes are expected and used freely for structural correctness (see
`docs/CONSTITUTION.md`).

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
