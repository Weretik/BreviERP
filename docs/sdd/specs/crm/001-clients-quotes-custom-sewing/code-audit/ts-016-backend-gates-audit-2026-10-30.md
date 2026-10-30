# P5 backend gates functional and responsibility audit — 2026-10-30

Scope: only the latest P5 items 1–3: CRM migration/persistence verification, introduction apply replay and canonical TS-016 OpenAPI. Existing frontend, source import, prices and delivery implementation were not changed by this audit.

## Finding

**AF-P5-016-07 — Low, verified refactor.** Application `Quotes/Introductions/IntroductionQuoteCreator.cs` combined orchestration of replay/template/quote creation with deterministic per-slot price request keys and construction of price-basis snapshots. Extracted `IntroductionPriceBasisIssuer.cs` beside it. Creator still owns operation order and failure propagation; issuer owns only child identity and price-basis issuance. It uses the same IQuotePriceBasisStore and identical identity byte algorithm. No DI, wire contract, transaction, schema or price formula change.

## Boundaries and functional checks

- `IntroductionQuoteApplicationIntent` retains stable versioned input; live prices/metadata are excluded. Null internal intent is excluded from ordinary CreateQuote serialization.
- `IntroductionQuoteReplayReader` adapts the existing journal; it creates no second receipt store. `QuoteMutationJournal` checks current ownership before hash comparison and returns stored output only for the same actor/operation/request key.
- Existing CreateQuote lock/transaction remains the commit boundary, including concurrency replay. QuoteVersion pin validation checks saved identities and offered prices before commit. A new application still validates current selections, while an already successful application replays before current template/catalog reads.
- Store read/write/transaction files and HTTP request/mapping files are already separated. Journal and replay adapter are small cohesive types; no further split justified.
- Canonical module OpenAPI and aggregate use six shared template/settings paths and nine operations. Requests have string layouts; current JSON responses have numeric enums. Contract covers pagination, ownership, 400/404/409 and replay behavior. No schema change was needed for replay.
- The two additive CRM migrations were applied in order to verified local brevidb after a verified backup. Earlier final 8/8 PostgreSQL tests (zero skipped) cover empty migration/reapply, existing-db upgrade, persistence, immutable rows, jsonb history and receipt/access/concurrency; this audit did not change mappings, migrations or journal behavior and did not repeat database writes.

## Fresh verification after extraction

- Focused unit/contract/public-projection selection: **28 passed, 0 failed/skipped**.
- Host.Api build: **0 warnings, 0 errors**.
- `git -c core.safecrlf=false diff --check`: passed.
- Unit command reports the existing NU1903 advisory for Microsoft.OpenApi.YamlReader 2.7.5; dependency remediation is outside this slice.

No new blocking defect confirmed in this scoped review. This is not full P5 acceptance: immutable consumer handoff, FE-011 integration and actual ordinary/seasonal PDF visual/privacy acceptance remain open. [Backend gate evidence](../evidence/ts-016-backend-gates-2026-10-30.md) preserves the migration/backup and PostgreSQL results. Old local receipts from unfinished TS-016 cannot be safely reinterpreted and retain the documented conflict boundary.
