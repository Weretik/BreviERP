---
name: brevi-erp-pr-handoff
description: "Use when completed BreviERP work needs review-ready commit grouping, branch naming, PR text, verification evidence, and residual-risk handoff."
---

# BreviERP PR Handoff

Підготуй review-ready handoff. Не commit, push, create або merge PR без прямого запиту.

## Workflow

1. Переглянь `git status`, scoped diff, diff stat і recent log; відокрем unrelated changes.
2. Звір accepted scope, feature status, audit findings і fresh verification.
3. Якщо commits дозволені, запропонуй batches за `git-commit-batching.md`; один commit — один reviewable intent, branch після кожного commit buildable. Inseparable model/configuration/migration тримай разом.
4. Запропонуй branch `codex/...`, imperative PR title і description: Summary, Scope, Database/API impact, Verification, Documentation, Residual risks.
5. Наведи exact results, rollout/rollback notes та reviewer focus.
6. GitHub integration використовуй лише на явний запит створити або оновити PR.

## Guardrails

- Не включай secrets, local absolute paths або unverified claims.
- Не включай `docs/new/` випадково до scope.
- Не включай unrelated user changes у commits або PR.

## Example

`Use $brevi-erp-pr-handoff to prepare the handoff for the completed Accounting feature; do not create the PR.`
