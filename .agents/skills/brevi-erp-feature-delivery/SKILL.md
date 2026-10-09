---
name: brevi-erp-feature-delivery
description: "Use when implementing or continuing an accepted BreviERP backend feature specification at feature, phase, scenario, or task scope."
---

# BreviERP Feature Delivery

Реалізуй лише погоджений scope і синхронізуй specification evidence з кодом.

## Workflow

1. Прочитай `AGENTS.md`, усі artifacts feature, applicable architecture/standards та AI workflow `01`–`04`.
2. Зафіксуй authorized scope: whole feature, phase, `SC-*`, `TS-*` або `EN-*`; перевір readiness і dependencies.
3. Виконай baseline audit за `docs/sdd/specs/_templates/code-audit/` для existing code. Системні findings маршрутизуй у remediation plan.
4. Для behavior slice виконай Red → Green → Refactor → Regression на найвужчому рівні, який доводить ризик.
5. Розміщуй invariants у Domain, orchestration/validation в Application, EF/adapters в Infrastructure, HTTP у Module.Api, composition у hosts. Зберігай isolation модулів.
6. Для read models дотримуйся repository projection/specification rules; для schema/seed changes застосуй `$brevi-erp-database-change`, для HTTP — `$brevi-erp-api-contract-sync`.
7. Після task онови status, traceability, exact results, deviations і residual risks. Перед completion застосуй verification та повний code audit.

## Guardrails

- Не реалізуй незатверджену поведінку й не обходь readiness.
- Не використовуй `docs/new/` як template або evidence source.
- Не запускай state-changing operations проти непогодженої БД.
- Не commit, push або create PR без прямого запиту.

## Example

`Use $brevi-erp-feature-delivery to implement SC-004 from docs/sdd/specs/reference/002-suppliers/.`
