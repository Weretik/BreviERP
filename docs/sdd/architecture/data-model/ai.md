# Ai schema

Code model reviewed 2026-11-05. `Ai.Infrastructure.Persistence.AiDbContext` owns schema `ai`; migration history is `ai.__EFMigrationsHistory`. Migration `InitialAi` creates the tables ([AI-001](../../specs/ai/001-ai-settings/README.md)).

| Table | Identity / relationships / purpose |
| --- | --- |
| Settings | PK Id (`openai`, one row). ProtectedApiKey (Data Protection purpose `Ai.OpenAi.ApiKey.v1`; the key is never stored in clear text), KeyHint (last 4), Enabled, FastModel + FastInputPrice/FastOutputPrice, StrongModel + StrongInputPrice/StrongOutputPrice (USD per 1M tokens), MonthlyLimitUsd, Version, UpdatedBy, UpdatedAt |
| UsageMonths | PK Id (`yyyy-MM`, UTC). Requests, InputTokens, OutputTokens, CostUsd; increased atomically after each answer |
| Prompts | PK Id (request kind, e.g. `purchase-invoices`). Text (up to 4000), UpdatedBy, UpdatedAt |

The Data Protection keys themselves live in `identity.DataProtectionKeys`; losing them makes the stored key unreadable, and the admin enters it again.
