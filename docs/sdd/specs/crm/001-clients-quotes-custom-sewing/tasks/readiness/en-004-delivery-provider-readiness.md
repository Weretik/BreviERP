# EN-004 — Delivery provider readiness

**Status:** partial: Gmail adapter/readiness and delegated OAuth code implemented; user consent and real sandbox round-trip pending. **Enables:** SC-D03. **Depends on:** EN-003.

## Paths and one responsibility

Planned roots/areas: `Crm Application delivery ports; existing host secret/config mechanisms; approved provider capability documentation`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Select actual provider/sender credentials through configured secure mechanism; establish sandbox/test recipient, stable send key/message ID and uncertain-outcome reconciliation. Do not import old Gmail secrets. Freeze safe retry/timeout/lease behavior; sending remains disabled until configured.

## Verification and evidence

Test level: Prerequisite analysis / adapter proof; no business test required for documentation-only decisions.

Provider test round-trip/status lookup, redacted runtime configuration requirements and failure/reconciliation examples; no real customer send.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/en-004.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

[Provider evidence](../../evidence/en-004-gmail-readiness-2026-10-30.md) records the chosen provider, safe defaults, fake HTTP tests and remaining OAuth requirements. Sending stays disabled; sender/test-recipient choices are local configuration, not credentials.

## Checkpoint

Provider integration supports truthful succeeded/failed/unknown states and a controlled test path.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.

[OAuth implementation and queue integration](../../evidence/p6-outbox-oauth-checkpoint-2026-10-30.md); [setup instructions](../../../../../operations/crm-gmail-oauth.md). No OAuth client exists yet per user confirmation.
