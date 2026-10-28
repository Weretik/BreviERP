# EN-003 — Private storage and document adapter readiness

**Status:** implemented and locally verified — 2026-10-28. **Enables:** SC-S05, SC-D02, SC-D04. **Depends on:** EN-001. [Actual adapter proof](../../evidence/en-003-2026-10-28.md). This closes adapter readiness; TS-007 ownership/link APIs and TS-010 issuance remain separate tasks.

## Paths and one responsibility

Planned roots/areas: `Crm.Application/Crm.Application/Abstractions/{Files,Documents}/; Crm.Infrastructure/Crm.Infrastructure/{Files,Documents}/; existing storage registration/configuration`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Inspect reusable storage/rendering capabilities and licenses/runtime requirements. Confirm private ACL, upload allowlist/20 MiB cap/30-file bound, hash/byte validation, renderer escaping/fonts/images and per-version retention. Choose the smallest existing adapter; any new dependency records actual version and verification. No public-media ACL downgrade.

## Verification and evidence

Test level: Prerequisite analysis / adapter proof; no business test required for documentation-only decisions.

Private test-file upload/read/hash round-trip; foreign-ID denial; render proof with Ukrainian glyphs and multipage layout; concrete runtime/config plan without secrets.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/en-003.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

## Checkpoint

Both metadata and real bytes work under scoped access; a source URL alone is not accepted as a file.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.
