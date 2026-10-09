---
name: brevi-erp-feature-orchestrator
description: "Use when an accepted BreviERP feature specification must be delivered with multiple agents and dependency-aware integration."
---

# BreviERP Feature Orchestrator

Координуй multi-agent delivery лише за явного запиту користувача та для прийнятої specification.

## Required input

- Exact feature path під `docs/sdd/specs/`.
- Accepted implementation scope.
- Явний запит на multiple agents.
- Окреме рішення щодо commits; default — no commit, push або PR.

## Workflow

1. Прочитай `AGENTS.md`, feature artifacts, AI workflow, architecture, standards і baseline audit. Підтвердь readiness.
2. Побудуй dependency graph для in-scope `SC-*`, `TS-*` і `EN-*`.
3. Сформуй work packets за `references/delegation-contract.md`. Один owner на mutable path; shared evidence coordinator-owned.
4. Паралель лише незалежні packets. Migrations, aggregate OpenAPI, host composition і shared feature records інтегруй послідовно.
5. Implementation packets застосовують `$brevi-erp-feature-delivery`; contract/database packets — відповідні specialized skills.
6. Після хвилі перевір diffs, ownership і narrow tests; coordinator оновлює shared traceability.
7. На integrated snapshot запусти `$brevi-erp-change-verification` і незалежний `$brevi-erp-code-audit`, потім `$brevi-erp-pr-handoff`.

## Stop conditions

Зупинись при неготовій specification, overlapping ownership, непогодженому contract/schema change, required check failure або open in-scope audit finding.

## Example

`Use $brevi-erp-feature-orchestrator to deliver the accepted Reference feature with multiple agents; stop before commits and PR creation.`
