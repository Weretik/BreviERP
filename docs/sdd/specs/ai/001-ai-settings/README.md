# AI-001 «Налаштування ШІ»

**Status:** implemented 2026-11-05. **Module:** Ai (new, schema `ai`). **Consumer:** brevi-web «Адміністрування → Налаштування ШІ»; Purchasing stage 3 ([PUR-001](../../purchasing/001-purchase-invoices/README.md)).

## Goal

One place where the admin connects Brevi to OpenAI and keeps its cost under control, without a secret in Cloud Run or in chat (user decision 2026-11-05: OpenAI, key entered in the admin).

## Decisions

| ID | Decision |
| --- | --- |
| D1 | New module Ai: settings, use and extra rules in schema `ai`. Other modules reach it only through ports implemented in the host (`ExtractStructuredDataCommand`); no module references Ai. |
| D2 | The API key is write-only: stored encrypted with ASP.NET Data Protection (keys persisted in `identity.DataProtectionKeys`, as the Gmail token of КП), returned only as `hasKey` + last four characters, never logged. Removing it switches the AI off. |
| D3 | Two models: fast (default `gpt-6-luna`, $0.10/$0.50 per 1M) tried first, strong (default `gpt-6.1-terra`, $1/$5) for one retry when the fast answer fails the caller's checks. Names and prices are editable because OpenAI changes them; prices checked 2026-11-05 in public price lists. |
| D4 | Cost = tokens from OpenAI's `usage` × the prices in settings, added per month (UTC). When the month reaches the limit, requests stop with «Ліміт витрат на ШІ цього місяця вичерпано». Default limit $5. |
| D5 | OpenAI Responses API with a strict `json_schema` answer and `store: false`; a PDF as `input_file`, a picture as `input_image` (base64). No redirects; 120 s timeout. |
| D6 | The admin may add extra rules per request kind (`purchase-invoices` = «Умови розпізнавання рахунків», up to 4000 characters). They are appended after the module's fixed instruction, which keeps the answer format; the schema cannot be changed from the admin. |
| D7 | «Перевірити» sends a tiny request to the fast or strong model, works while the AI is off, and is counted like any request. |
| D8 | Access: policy `RequireAdminRole` (Admin; the Director holds Admin). |

## Contract and tests

[OpenAPI](../../../contracts/ai/ai-settings.openapi.yaml): `getAiSettings`, `updateAiSettings`, `setAiKey`, `removeAiKey`, `updateAiPrompt`, `testAiConnection`. Tests: `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Ai"` and `tests/ArchitectureTests/AiDependencyTests.cs`.

## Deployment

Migration `InitialAi` runs with the normal deploy. Then the admin opens «Налаштування ШІ», enters the OpenAI key (platform.openai.com → API keys), presses «Перевірити» for both models, sets the limit and switches the AI on. Nothing is sent to OpenAI before that.
