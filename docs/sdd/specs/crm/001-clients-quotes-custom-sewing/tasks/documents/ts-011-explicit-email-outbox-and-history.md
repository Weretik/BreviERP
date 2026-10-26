# TS-011 — Explicit email outbox and history

**Status:** planned. **Covers:** SC-D03. **Depends on:** TS-010, EN-004.

## Paths and one responsibility

Planned roots/areas: `Crm.Domain/Crm.Domain/Delivery/; Crm.Application/Crm.Application/Delivery/{EnqueueQuote,GetDeliveries}/; Crm.Infrastructure/Crm.Infrastructure/Delivery/; Host worker registration`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Create enqueue/audit transaction with explicit recipient/version/PDF; worker lease/attempt and provider result reconciliation. Record sent event only after confirmed success; support same-key retry and safe error status. Import sends remain inert history, not queue items.

## Verification and evidence

Test level: Focused Domain/Application, PostgreSQL/API integration and architecture checks as required by this task; no blanket full suite.

Fake-provider duplicate request/worker overlap/crash/unknown tests and controlled test-recipient manual evidence; API status/history contract checks.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/ts-011.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

## Checkpoint

Queued/downloaded is never displayed as sent, uncertain attempts never blindly resent.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.
