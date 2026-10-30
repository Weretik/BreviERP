# TS-006 — Custom drafts, calculation and pinned versions

**Status:** implemented locally — backend custom draft/version/calculation and quote hydration verified 2026-10-28. **Covers:** backend portions of SC-S01–03; actual private bytes/visibility remain TS-007. **Depends on:** completed EN-002 arithmetic, TS-003, TS-004. [Execution evidence](../../evidence/ts-006-2026-10-28.md).

## Paths and one responsibility

Actual paths under `src/Modules/Crm/`: `Crm.Domain/Crm.Domain/CustomSewing/{Composition,Drafts,Versions,Identifiers,Pricing}`; `Crm.Application/Crm.Application/CustomSewing/{CreateDraft,SaveDraft,Calculate,SaveVersion,GetDraft,GetVersion,GetVersions,ResolveQuoteVersions,Access,Contracts,Mapping,Persistence,Pricing}`; `Crm.Infrastructure/Crm.Infrastructure/Persistence/CustomSewing/{Reading,Writing,Pricing,Idempotency}` and `Configurations/CustomSewing`; `Crm.Api/Crm.Api/CustomSewing/{Requests,CustomSewingController.cs}`. The Host pricing adapter calls the existing Catalog evaluator; quote hydration uses a bounded batch query. One command/query/handler/validator/request/value type per file; SQL specifications belong to their read scenario.

## Work

Implement private structured composition, product type/season, client/private text, applications/delivery/packaging, fabric alternatives, immutable custom versions and quote hydration from server records. Reuse EN-002 adapter; calculation requires fabric/operation and coherent approved versions. Draft can retain typed blockers; no magic operation-ID values.

## Verification and evidence

Test level: Focused Domain/Application, PostgreSQL/API integration and architecture checks as required by this task; no blanket full suite.

Custom golden costs/tiers, two fabric versions, reference blocker, server hydration and parent quote independence; concurrent draft-save and private ownership API tests.

Evidence: [TS-006 execution](../../evidence/ts-006-2026-10-28.md). Domain Red: 2 failed / 2 passed, then 4 passed. Storage-limit adapter Red: 1 failed / 2 passed. Stable tier projection Red: 1 failed; the migration regression exposed the same issue. Verified: 57 focused unit tests, 40 integration/API tests, 2 architecture checks, and 17 custom integration checks after final projection/spec refactoring. Canonical OpenAPI generates successfully. No commit/push or working-database deployment.

## Checkpoint

Quote pins real custom version, not mutable browser snapshot; later draft edits do not change prices or files.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.

## Follow-up audit — 2026-10-28

[Functional audit and decomposition](../../evidence/ts-006-audit-refactor-2026-10-28.md): AF-CRM-P4-005–008 verified; source-revalidation expiry race fixed with deterministic clock-boundary tests. Host input/output mapping, Domain readiness and calculation-token persistence have explicit owners. Fresh checks: 36 unit, 17 integration/API and 2 architecture tests passed. No API/schema change, commit or push.
