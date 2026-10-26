# TS-003 — Server quote price preview adapter

**Status:** planned. **Covers:** SC-Q01–04, SC-Q07. **Depends on:** EN-002.

## Paths and one responsibility

Planned roots/areas: `Crm.Application/Crm.Application/Quotes/PreviewQuotePrices/; Abstractions/Pricing/; Host Crm pricing adapter; Catalog pure-engine adapter if required; tests/UnitTests/Crm/Pricing/`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Implement typed Article/PPE/custom price intent and immutable server basis storage/token, input hash/version coherence, blockers and PPE-only reasoned override. Reuse common backend engine and current published/source contracts without publishing private custom configuration as a Product. Freeze preview schema and operation before frontend use.

## Verification and evidence

Test level: Focused Domain/Application, PostgreSQL/API integration and architecture checks as required by this task; no blanket full suite.

Golden calculation tests, forged totals/cross-owner/expired basis rejection, stale source conflict, normal 452/453 and blocked 724/931 regression; targeted architecture tests.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/ts-003.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

## Checkpoint

Preview never mutates Catalog masters, exposes no arbitrary formula execution and can be saved reproducibly.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.
