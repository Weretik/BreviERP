# TS-006 — Custom drafts, calculation and pinned versions

**Status:** planned. **Covers:** SC-S01–03. **Depends on:** TS-003, TS-004.

## Paths and one responsibility

Planned roots/areas: `Crm.Domain/Crm.Domain/CustomSewing/{Drafts,Versions,Composition}/; Crm.Application/Crm.Application/CustomSewing/{SaveDraft,Calculate,SaveVersion}/; Crm.Api/Crm.Api/CustomSewing/`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Implement private structured composition, product type/season, client/private text, applications/delivery/packaging, fabric alternatives, immutable custom versions and quote hydration from server records. Reuse EN-002 adapter; calculation requires fabric/operation and coherent approved versions. Draft can retain typed blockers; no magic operation-ID values.

## Verification and evidence

Test level: Focused Domain/Application, PostgreSQL/API integration and architecture checks as required by this task; no blanket full suite.

Custom golden costs/tiers, two fabric versions, reference blocker, server hydration and parent quote independence; concurrent draft-save and private ownership API tests.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/ts-006.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

## Checkpoint

Quote pins real custom version, not mutable browser snapshot; later draft edits do not change prices or files.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.
