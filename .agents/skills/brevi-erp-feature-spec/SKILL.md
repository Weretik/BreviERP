---
name: brevi-erp-feature-spec
description: "Use when creating, extending, or incrementally migrating a BreviERP backend feature specification before implementation."
---

# BreviERP Feature Specification

Створюй behavior-first SDD-специфікацію до зміни runtime code. Цей skill не реалізує feature.

## Workflow

1. Прочитай кореневий `AGENTS.md`, `docs/sdd/architecture/README.md`, релевантні standards і `docs/sdd/specs/_templates/README.md`.
2. Визнач owning module: `identity`, `catalog`, `reference`, `accounting`, `crm`, `platform` або інший наявний module. Не створюй нову module boundary без architecture decision.
3. Створи або доповни `docs/sdd/specs/<module>/<NNN>-<feature-slug>/` з observable goal, in/out scope, rules `R-*`, scenarios `SC-*`, design, risks, tasks і traceability.
4. Для persistent model до коду зафіксуй identifier strategy, read-model approach, integrity, migration baseline, rollout і rollback. Прочитай `identifier-strategy.md` та `database-rules.md`.
5. Для HTTP behavior узгодь route, method, operationId, DTO, paging metadata, errors, authorization, idempotency і compatibility; зв'яжи OpenAPI у `docs/sdd/contracts/`.
6. Розбий delivery на dependency-ready `TS-*` і `EN-*`; behavior tasks визначають Red, Green, Refactor, Regression та evidence.
7. Перевір readiness checklist і unresolved `[NEEDS CLARIFICATION: ...]`. Зупинись після specification artifacts.

## Guardrails

- Використовуй тільки `docs/sdd/`; `docs/new/` не є BreviERP source of truth.
- Не вигадуй business rules, database state, public contracts або external integrations.
- Не копіюй stable standards у feature і не переписуй historical evidence.
- Не запускай migrations або seeders у planning scope.

## Example

`Use $brevi-erp-feature-spec to prepare accounting feature 003-wallet-transfer. Goal: authorized users can transfer funds between wallets.`
