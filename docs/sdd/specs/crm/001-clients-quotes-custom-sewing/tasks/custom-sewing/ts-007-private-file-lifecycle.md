# TS-007 — Private file lifecycle

**Status:** planned. **Covers:** SC-S03, SC-S05. **Depends on:** EN-003, TS-006.

## Paths and one responsibility

Planned roots/areas: `Crm.Domain/Crm.Domain/CustomSewing/Attachments/; Crm.Application/Crm.Application/Files/; Crm.Infrastructure/Crm.Infrastructure/Files/; Crm.Api/Crm.Api/Files/`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Implement staged upload, bytes/MIME/size/hash validation, finalize/link with root revision and quote/production flags, ACL read and version retention. Cleanup removes only unattached expired temporary uploads, never referenced historic bytes; no hard delete endpoint for history.

## Verification and evidence

Test level: Focused Domain/Application, PostgreSQL/API integration and architecture checks as required by this task; no blanket full suite.

Spoofed MIME, >20 MiB, foreign/unfinalized IDs, 30-file boundary, concurrent linkage and customer projection leakage tests; runtime byte round-trip.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/ts-007.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

## Checkpoint

File availability reflects actual bytes and proper version/owner, not metadata row existence.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.
