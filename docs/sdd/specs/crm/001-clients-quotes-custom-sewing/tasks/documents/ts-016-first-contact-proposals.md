# TS-016 — First-contact proposal layouts and owned selections

**Status:** planned. **Covers:** SC-I01–03. **Depends on:** TS-008, TS-010; TS-011 for actual send acceptance.

## Paths and responsibility

Planned: `src/Modules/Crm/Crm.Domain/Crm.Domain/Quotes/Introductions/`, Application `Quotes/BuildIntroduction/`, `Templates/`, Infrastructure `Documents/Introductions/`, and `tests/{UnitTests,IntegrationTests}/Crm/Introductions/`. Split layout configuration, slot validator, selection template, builder, public projection and renderer into individual files/types; reuse quote/version/outbox ports.

## Work

Implement R-I01–03 with 4/8 typed slots, exact approved featured model mappings, season/one-fabric/metadata/product-URL/large-tier validation. Preserve owned Manager template lifecycle and typed current-price application into a new quote draft. Extend existing template contract with presentation kind/layout/version/selection references and archive operation, not an independent quote API/store. Custom-configuration templates and first-contact Catalog selection templates have distinct discriminated content. PDF/email uses shared pinned document/delivery pipeline; no hardcoded aliases or stale template prices.

## Test level and Evidence

Focused Domain slot/identity validation, Application current-price/ownership tests, persistence template revision/archive tests, API contract checks and renderer integration. Record Red → Green → Refactor → Regression command/test outcomes in dated `evidence/ts-016.md` when run; currently **not run**. Required cases: general vs seasonal counts, duplicates, wrong season, missing featured/metadata/price, own/foreign template, source change between reuse and historical read, one/two-page rendered customer projection, explicit-send idempotency.

## Checkpoint

Manager can save/reuse their own first-contact selection; correct 4/8-model document uses current prices for a new version and immutable prices for history, without bypassing quote/privacy/send rules. [Sequence](../implementation-plan.md).
