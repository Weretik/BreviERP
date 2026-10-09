---
name: brevi-erp-docs-sync
description: "Use when checking or repairing drift between BreviERP code and canonical architecture, standards, contracts, operations, or feature documentation."
---

# BreviERP Documentation Sync

Знайди й виправ лише підтверджений drift між repository truth та `docs/sdd/`.

## Workflow

1. Визнач scope через request, `git status` і scoped diff. Прочитай `AGENTS.md` та relevant indexes.
2. Визнач canonical source: project/config for architecture, runtime/tests for behavior, EF migrations for schema lineage, OpenAPI for public contracts, feature artifacts for accepted intent.
3. Порівняй source з `docs/sdd/architecture/`, `standards/`, `contracts/`, `operations/`, `frontend-scenario/` і relevant `specs/`.
4. Класифікуй drift як stale, missing, contradictory або historical-only. Audit-only scope повертає findings; repair scope вносить мінімальні зміни.
5. Не документуй planned behavior як implemented і не переписуй historical evidence. Contract drift маршрутизуй через `$brevi-erp-api-contract-sync`.
6. Перевір links, paths, skill references і `git diff --check`; для behavior claims запусти відповідні checks.

## Guardrails

- `docs/new/` не є canonical BreviERP documentation і не використовується для синхронізації.
- Не роби broad rewrite для локального drift.
- Зберігай stable IDs та unrelated changes.

## Example

`Use $brevi-erp-docs-sync to compare Host.Seed behavior with operations docs and repair confirmed drift.`
