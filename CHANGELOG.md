# Changelog

All notable changes to IronHive are documented here. Pre-1.0 (0.x): breaking
changes are expected and used freely for structural correctness (see
`docs/CONSTITUTION.md`).

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
