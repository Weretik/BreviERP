# TS-009 functional and responsibility audit — 2026-10-29

**Scope:** P5 EN-005/TS-009 changes in Crm Domain, Application, Infrastructure, API, EF migration, canonical OpenAPI, tests, SDD and feature map. Reviewed against SC-D01, R-D01/R-D06, the TS-009 task, backend layer/database/API rules and adjacent Crm request conventions. Ready/PDF (TS-010) and sending (P6) were outside this audit.

## Findings

| ID | Severity / status | Evidence and impact | Owner / resolution | Verification |
| --- | --- | --- | --- | --- |
| AF-CRM-P5-001 | Medium / fixed | The new outer request records and nested `IssuerSettingsInput`/`DocumentTemplateInput` accepted unknown JSON fields although `issuer-settings.openapi.yaml` declares `additionalProperties: false`. A client could send extra fields and receive success instead of a contract error. | Crm API/Application: added `JsonUnmappedMemberHandling.Disallow` to all four records, matching adjacent Crm request behavior. | Focused test was Red (expected `JsonException`, got none), then Green; `IssuerSettingsRulesTests` 5/5 and DB-free `IssuerSettingsRequestContractTests` 1/1 pass. |
| AF-CRM-P5-002 | High / verified fixed | Initially `IssuerSettingsPersistenceTests` compiled but skipped 5/5 without a local PostgreSQL connection. The first live run passed 4/5: the history-guard test expected `P0001` although the migration explicitly raises `55000`. | Backend verification: run the five scenarios in an isolated temporary PostgreSQL database; correct the test expectation to `55000` for both UPDATE and DELETE. The trigger and migration were unchanged. | Final focused run passed 5/5 with zero skipped. CRM-only migration applied to local `brevidb` with zero pending afterward. |

## Responsibility and boundary review

The earlier broad settings store was split before this audit into `Persistence/Documents/Writing`, `Reading`, `Mapping`, `Idempotency` and `Numbering`. Files now have one primary role. Domain holds typed issuer/template versions and active pointer; Application holds separate GET/create/activate handlers, contracts and ports; Infrastructure owns EF mapping, transaction/replay and allocator; API holds a thin role-protected controller and independent request records. Shared Crm DI, DbContext and aggregate OpenAPI only register or reference the slice. No new module boundary is warranted: Crm owns quote documents.

The generated migration adds five tables and restrictive active-version FKs; the coordinator added PostgreSQL UPDATE/DELETE guards for immutable version rows. `dotnet ef migrations has-pending-model-changes` reported no model drift, and the generated SQL contains only additive table creation and the two history triggers. It ran in the isolated test database and then through CRM-only Host.Seed against local `brevidb` after a CRM-schema backup. Number allocation requires the caller's transaction, serializes a UTC month with an advisory lock and considers numbers already present in both old and new separator formats. Unknown future archive numbers remain a P7 import reconciliation dependency.

## Checks and limits

- `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~Issuer_and_template_inputs_reject_unmapped_json_fields --no-restore --nologo`: Red, 1 failed as expected because unknown fields were accepted.
- `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~IssuerSettingsRulesTests --no-restore --nologo`: Green/regression, 5 passed.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~IssuerSettingsRequestContractTests --no-restore --nologo`: 1 passed without a database.
- Earlier TS-009 checks remain applicable: Crm API/Infrastructure builds, aggregate OpenAPI 1/1, Crm architecture 1/1, EF no-pending-model check. The expanded persistence test command compiled but skipped 5/5 after adding imported-number and concurrency scenarios.
- `git diff --check` and whitespace review of new files are the final documentation/code hygiene checks; no full suite or production build was requested.

Follow-up 2026-10-29: the five focused persistence scenarios passed against the locally started PostgreSQL, including replay/access, append-only history, legacy-number skip and concurrent numbering. The CRM-only Host.Seed plan listed nine pending Crm migrations; a verified CRM-schema backup preceded apply, and apply reported zero pending afterward. No source import, ready/PDF generation, send, commit or push was performed. EN-005 policy is confirmed; TS-009's local verification gate is closed. No further file or module split is justified by the current responsibility map.
