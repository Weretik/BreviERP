---
name: brevi-erp-change-verification
description: "Use when BreviERP code, tests, contracts, migrations, documentation, or build configuration changed and fresh completion evidence is required."
---

# BreviERP Change Verification

Обери найменший risk-adequate набір перевірок і надай свіже evidence.

## Workflow

1. Переглянь `git status`, scoped diff і diff stat; відокрем task changes від unrelated changes.
2. Зістав ризик із Unit, Integration, Architecture або API/contract test за `testing-rules.md`.
3. Спочатку запускай focused project/test filters, потім required regression. Для feature completion, де застосовно:
   - `dotnet restore BreviERP.sln`
   - `dotnet build BreviERP.sln --no-restore`
   - `dotnet test BreviERP.sln --no-build`
4. Для layer/reference changes включи `tests/ArchitectureTests`. Для EF/OpenAPI/HTTP/Host.Seed — relevant IntegrationTests та operational evidence.
5. Для migration/seed change перевір migration на disposable PostgreSQL, Host.Seed logs, idempotency, rollout і rollback; не торкайся непогодженої БД.
6. Для docs-only changes перевір links, canonical `docs/sdd` paths, skill structure і `git diff --check`.
7. Повідом exact commands, exit codes, test counts, warnings, skipped та unavailable checks.

## Guardrails

- Не називай роботу passing без required fresh evidence.
- Не виправляй unrelated failures без окремого scope.
- Не використовуй результати або команди з `docs/new/`.

## Example

`Use $brevi-erp-change-verification to verify the current Accounting wallet changes and report exact evidence.`
