# TS-008C — frozen-source input review

Specified before implementation, 2026-10-26. Authorized scope: verify existing imported drafts and identify concrete corrections before confirmation. A source match is technical review evidence, not an authenticated manager approval. TS-008D still owns confirmation/save/publication. Missing values and inactive states are retained; no blanket approval or invented cost is allowed.

## Rules and acceptance

- R-C01 / SC-C01: compare every imported material/operation line to the immutable source through original SourceIdMappings and existing import mappers. Check quantity, unit, reference, required flag, scope, ordering and cardinality; retain Product composition version. Extra/missing/changed lines must prevent a clean result. Review metadata is intentionally not compared because later approval owns it.
- R-C02 / SC-C02: compare current PPE offer/rule values to the frozen PPE source, including its hash/date/source-reviewed state. Preserve exact version IDs, current approval state and per-subject differences; numeric equivalence must not depend on JSON decimal formatting. Do not create fallback rules for valid external-price paths.
- R-C03 / SC-C03: verify current effective settings/reference versions against the existing frozen baseline using existing builders and current effective read ports, and compare all imported Sewing outputs through the existing diagnostic engine. A baseline difference is a blocked review, never an automatic overwrite.
- R-C04 / SC-C04: local-only review mode is mutually exclusive with other Host.Seed modes, requires both frozen source paths and report output, writes only reports and retains source provenance. Missing source data (724) and inactive status (931) remain distinct unresolved decisions. Unchanged reports do not mutate approvals/history.

## Planned responsibility map

Existing `CatalogImportPlanner`, composition mappers, pricing source mappers and baseline logic remain unchanged. Add `Jobs/SewingPricing/Review/` with orchestration, separate composition/PPE/baseline comparison readers and one type per report/check file; Host.Seed owns explicit configuration/output. No new module/project, schema, external API or frontend change is required.

Use a repeatable-read Catalog/Reference/provenance snapshot; PPE latest records are read inside that snapshot. Report source hashes, selected version IDs and composition versions permit later revalidation. The report never supplies an actor, write request or publication. A mismatch records a correction requirement; rewriting a changed version to match old data needs an accepted decision and the existing versioned API, not this reviewer.

## Tasks and evidence

TS-008C1: implement per-field pure checks and composition/PPE/reference comparison; test numeric equality and changed-field detection, changed/missing composition and no-write behavior on isolated PostgreSQL. TS-008C2: run against the local target, run existing Sewing diagnostic for all imported SKUs, classify actual differences and record review decisions. TS-008C3: create the ordered handoff for TS-008D; unresolved source/activation decisions remain explicit. Existing focused regression is sufficient for reused code; no artificial Red is required for unchanged mappers.

[Execution evidence](review-evidence.md). Backups/migrations are not needed for this read-only phase; later state-changing activation follows the existing checklist and backup procedure. No price, quantity, approval or active state is changed by this mode.

## Current acceptance

SC-C01..04 verified by [review evidence](review-evidence.md). DR-C01 accepts the retained manual operation and corrected outputs for 452/453; [decisions and recovery](review-decisions.md) preserve its scope. Input review requires no numeric correction. SKU 724 and inactive 931 are explicit exclusions. Confirmation/publication remains TS-008D.

## Structural follow-up

The source-review readers are grouped by Composition, Ppe and Reference. Material/operation field comparers are separate pure files in Comparison/Composition; Catalog settings selection is separate from Reference input review. These are behavior-preserving ownership changes under TS-008C. [Audit](review-structure-audit.md) · [Verification](review-structure-verification.md).
