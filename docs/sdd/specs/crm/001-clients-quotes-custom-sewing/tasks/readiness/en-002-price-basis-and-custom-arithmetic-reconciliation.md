# EN-002 — Price-basis and custom arithmetic reconciliation

**Status:** planned. **Enables:** SC-Q01–04, SC-Q07, SC-S01–02. **Depends on:** EN-001.

## Paths and one responsibility

Planned roots/areas: `docs/sdd/specs/catalog/003-sheet-pricing-parity/; Catalog Application pricing contracts; Crm.Application/Crm.Application/Abstractions/Pricing/; tests/Fixtures/Crm/`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Compare existing engine signatures/units and current approved Sheet-derived formulas with v211 custom cost/application rules. Freeze UAH/current monetary and tax basis, semantic tier ranges, decimal precision, coherent source-version resolution and basis token scope/expiry. Confirm custom labour/overhead/assembly use and application sale addition once; no hardcoded defaults. Extend human contract design only if needed before code.

## Verification and evidence

Test level: Prerequisite analysis / adapter proof; no business test required for documentation-only decisions.

Dated comparison, synthetic golden fixture input versions/expected tiers and rounding, catalog blocker matrix and selected adapter contract. Unknown price/tax decision remains an explicit blocked dependent case.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/en-002.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

## Checkpoint

Price rules have reproducible fixtures and one engine owner; no extra VAT, fallback constants or threshold guessing.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.
