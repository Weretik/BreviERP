# EN-005 — Issuer, numbering and tax policy values

**Status:** planned. **Enables:** SC-D01–02. **Depends on:** EN-002.

## Paths and one responsibility

Planned roots/areas: `This SDD decisions-and-risks.md; Crm document/number policies; verified organizational settings`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Obtain verified seller requisites/logo, actual currency/tax wording and human number format/scope; first inspect existing authoritative organizational settings. Source five tabs have no values. Record who confirmed the values; agree number allocation point, sequence/concurrency behavior, imported number collision handling and inactive setting rules. No guessed IBAN/tax identity.

## Verification and evidence

Test level: Prerequisite analysis / adapter proof; no business test required for documentation-only decisions.

Approved policy/value provenance, redacted example PDF wording, numbering collision/concurrency acceptance examples. Keep unconfirmed fields as a precise blocker.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/en-005.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

## Checkpoint

Ready/PDF can be enabled legally and technically without fabricated company data; drafts remain usable independently.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.
