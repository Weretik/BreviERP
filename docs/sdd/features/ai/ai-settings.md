# AI settings «Налаштування ШІ»

The OpenAI account of Brevi for «Адміністрування → Налаштування ШІ»: the key (encrypted, write-only, shown as the last four characters), fast and strong models with prices per 1M tokens, on/off, monthly limit and this month's use, «Перевірити», and «Умови розпізнавання рахунків» added to the invoice recognition, shown under the fixed base instruction (read-only). Admin only. [SDD](../../specs/ai/001-ai-settings/README.md), [contract](../../contracts/ai/ai-settings.openapi.yaml), [tables](../../architecture/data-model/ai.md).

## Code structure

```text
src/Modules/Ai/
  Ai.Domain/Ai.Domain/
    Settings/          # AiSettings (key, models, prices, limit, version), AiModelPrice, AiModelTier
    Usage/             # AiUsageMonth
    Prompts/           # AiPrompt (extra rules per request kind)
  Ai.Application/Ai.Application/
    Abstractions/      # IAiDbContext, IAiKeyProtector, IOpenAiClient, IAiBasePrompt (fixed instruction a module shows)
    Features/Calls/Shared/   # AiCaller: key, limit, extra rules, call, usage — the only sender to OpenAI
    Features/Settings/       # Queries/GetAiSettings; Commands/UpdateAiSettings (version), SetAiKey, RemoveAiKey, UpdateAiPrompt, TestAiConnection («Перевірити»)
    Features/Extraction/Commands/ExtractStructuredData/  # for other modules (through `Ai.Contracts`)
  Ai.Infrastructure/Ai.Infrastructure/
    Persistence/       # AiDbContext (schema ai), configurations, migrations
    Integrations/OpenAi/ # AiKeyProtector (Data Protection)
    Integrations/OpenAi/ # OpenAiResponsesClient (Responses API, strict json_schema)
  Ai.Api/Ai.Api/Endpoints/Settings/AiSettingsEndpoints.cs   # /api/ai/settings, policy RequireAdminRole; <UseCase>/ per endpoint
  Ai.Contracts/Ai.Contracts/
    Extraction/IStructuredDataExtractor.cs  # structured answer from text and files, for other modules
    Prompts/{IAiBasePrompt,AiPromptKeys}.cs # base instructions other modules register
  Ai.Infrastructure/Ai.Infrastructure/Contracts/Extraction/StructuredDataExtractor.cs
src/Modules/Purchasing/Purchasing.Infrastructure/Purchasing.Infrastructure/Adapters/Ai/
  PurchaseDocumentRecognizer.cs    # Purchasing → Ai.Contracts
  PurchaseInvoiceBasePrompt.cs     # invoice base instruction shown read-only
```

## Tests

`tests/UnitTests/Ai/`: key encryption and hint, version, switching on without a key, limit and monthly use, extra rules and the read-only base instruction, «Перевірити», Responses request and answer. Command: `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Ai"`. Module references and schema: `tests/ArchitectureTests/AiDependencyTests.cs`.
