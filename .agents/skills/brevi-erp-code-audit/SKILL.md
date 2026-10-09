---
name: brevi-erp-code-audit
description: "Use when performing a BreviERP architecture or code audit, or when feature delivery requires baseline or final audit phases."
---

# BreviERP Code Audit

Проводь evidence-based audit; audit-only запит не дозволяє змінювати runtime code.

## Workflow

1. Визнач scope і прочитай `AGENTS.md`, feature, architecture, standards та contracts.
2. Прочитай `docs/sdd/specs/_templates/code-audit/README.md`. Для delivery виконай required baseline phases, після implementation — full audit.
3. Перевір compiled/source dependencies, module isolation, Domain/Application/Infrastructure/API responsibilities, security, data access, tests і docs.
4. Для reads звір SQL projection/specification pattern і paging; для writes — invariants, transaction/idempotency; для API — DTO/errors/authorization/OpenAPI.
5. Створи або онови feature-local audit artifact. Findings мають stable `AF-*`, severity, status, exact path, evidence, impact, owner, target, task, dependencies і verification.
6. Systemic findings маршрутизуй у `$brevi-erp-remediation-plan`; open in-scope findings є blockers.

## Guardrails

- Не роби висновок лише з назв файлів або diagrams.
- Не використовуй `docs/new/` як audit standard.
- Не закривай `AF-*` без fresh evidence.
- Не змішуй audit і fix без окремого запиту.

## Example

`Use $brevi-erp-code-audit to run the full audit for the Catalog product-page feature and record findings only.`
