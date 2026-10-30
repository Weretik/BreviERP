# TS-016 — First-contact proposal layouts and owned selections

**Status:** partially implemented and locally verified — 2026-10-29. Typed 4/8 slots, current-offer validation, Catalog/Reference adapter and local fabric metadata migration/backfill are complete in this checkpoint; owned template/config persistence, quote creation, API, public PDF and send remain. [Checkpoint evidence](../../evidence/ts-016-core-2026-10-29.md). **Covers:** SC-I01–03 when complete. **Depends on:** TS-008, TS-010; TS-011 for actual send acceptance.

## Paths and responsibility

Implemented: `src/Modules/Crm/Crm.Domain/Crm.Domain/Quotes/Introductions/`, Application `Quotes/BuildIntroduction/`, Host.Api `Crm/IntroductionCatalogReader.cs`, Reference fabric metadata/migration, and focused `tests/UnitTests/Crm/Introductions/` plus Reference metadata tests. Planned: owned `Templates/` and Infrastructure `Documents/Introductions/` integration, API/contract and renderer. Split layout configuration, slot validator, selection template, builder, public projection and renderer into individual files/types; reuse quote/version/outbox ports.

## Work

Implement R-I01–03 with 4/8 typed slots, exact approved featured model mappings, season/one-fabric/metadata/product-URL/large-tier validation. Preserve owned Manager template lifecycle and typed current-price application into a new quote draft. Extend existing template contract with presentation kind/layout/version/selection references and archive operation, not an independent quote API/store. Custom-configuration templates and first-contact Catalog selection templates have distinct discriminated content. PDF/email uses shared pinned document/delivery pipeline; no hardcoded aliases or stale template prices.

## Test level and Evidence

The [partial checkpoint](../../evidence/ts-016-core-2026-10-29.md) records 11 passing focused Domain/Application/Reference tests, successful Host.Api build, local adapter/pricing smoke, exact source mappings and the local additive migration/backfill. Remaining verification: Manager ownership, persisted template revision/archive, API contract, source change between reuse and historical read, one/two-page rendered customer projection and explicit-send idempotency. The complete TS-016 evidence remains pending.

## Checkpoint

Manager can save/reuse their own first-contact selection; correct 4/8-model document uses current prices for a new version and immutable prices for history, without bypassing quote/privacy/send rules. [Sequence](../implementation-plan.md).

## Backend gates completed — 2026-10-30

Owned template/config persistence, quote creation with pinned public snapshot, stable apply replay, canonical API contract and local CRM migrations are implemented and verified. [Evidence](../../evidence/ts-016-backend-gates-2026-10-30.md): 28 focused unit/contract tests and 8 PostgreSQL tests. Frontend handoff/integration and actual rendered PDF acceptance remain; TS-016 is still partial overall.

## Local rendered acceptance — 2026-10-30

P5 ordinary/seasonal PDFs and saved-selection/ready/artifact/reopen journey are locally verified after pagination and early capacity fixes. [Combined evidence](../../evidence/p5-local-acceptance-2026-10-30.md). P6 send/new-selection deferral and deployed/business approval remain outside this checkpoint.
