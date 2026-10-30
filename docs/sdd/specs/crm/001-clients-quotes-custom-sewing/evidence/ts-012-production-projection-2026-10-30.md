# TS-012 production package: projection and persistence evidence

Date: 2026-10-30. Scope: R-S07 / SC-S06, production package v1; no workshop order creation, stock reservation, or release to an external system.

## Module boundaries

- Application `CustomSewing/Production/Contracts`: explicit versioned manufacturing allowlist. `Projection`: pure compiler, operation norm resolution and production-file projection. No external calls or current reference-master reads.
- Infrastructure `Persistence/Documents/Production`: exact issued-version source reader; transactional export journal/artifact store; authorized historical reads. `Configurations/Production`: mapping only.
- Root integration owns archive construction and byte validation, file provider, HTTP handlers, dependency registration and OpenAPI.

The manifest exposes quote/version and custom-version provenance, variant IDs, physical material quantities and units, saved operation norms and effective manual overrides, application location/artwork reference, and the approved wage basis (total minutes, minute rate, seamstress/cutting/master/bonus). It does not dump price-basis JSON, recipient/contact data, private comments, supplier source values, sales prices, manager/overhead/delivery/packaging costs or internal totals. Fabric/accessory names are not invented where immutable snapshots supply only reference IDs.

Only included items and explicitly selected visible custom-sewing variants are accepted. Missing/ambiguous saved operation norm, mismatched effective-time sum, incomplete composition and missing production-approved artwork block export. Canonical file names use UUIDs; only PDF/PNG/JPEG are allowed. Limit: 30 distinct production files across the package, 20 MiB per immutable file; root ZIP adapter enforces manifest/package aggregate byte bounds.

## Persistence and consistency

`ProductionPackageArtifacts` uses an internal bigint identity and a separate public UUID. Quote/version composite FK prevents cross-quote attachment. Unique quote/version/sorted-selection hash provides one immutable artifact; ZIP bytes, SHA-256 and recorded length are stored together. Private content is bounded to 64 MiB and integrity checked before reads.

`ProductionPackageRequests` is an append-only actor/request journal with intent hash and same-quote artifact FK. Current owner/Admin authorization precedes replay payload comparison. Changed request payload conflicts; a new request for the same artifact records a new audit receipt without opening source files or rebuilding. Export uses PostgreSQL transaction advisory locks and quote row locking. It does not extend the quote lifecycle-event whitelist.

Source compilation requires `QuoteIssue` for the exact requested quote version and compares the saved quote custom snapshot to the pinned immutable custom version. Historical artifact reads do not recalculate or use current draft/master values. Current authorization still applies after owner transfer.

Migration: `20261030150923_AddProductionPackages` adds only the two production tables, constraints/indexes, and UPDATE/DELETE rejection triggers. Down explicitly drops those triggers/function and then the tables. Apply after `20261030150013_AddEmailTemplateRegistry`, before enabling the production API. No live database was migrated by this agent. Down removes historical export evidence; use only before production data exists, otherwise retain/restore data under the project's operational procedure.

## Verification handoff

- Infrastructure project build succeeded with 0 errors and 0 warnings before migration generation.
- Six focused pure projection tests passed before the final operation-total and aggregate-file guards; their rerun belongs to root's serialized verification to avoid shared DLL compilation locks.
- Six focused PostgreSQL tests cover cached replay/history/privacy; payload conflict and authorization after owner transfer; unissued/mismatched snapshot failures with no writes; concurrent export deduplication; database append-only guards; and archived-quote rejection of new requests while existing replay/read remain available.
- Initial integration compilation found assertion-constructor/analyzer issues in the new tests; these are corrected. Root owns the final serialized build and local disposable PostgreSQL test results.
- A parallel unit/integration build attempt hit a shared compiler DLL lock; it is not represented as a passing final run.

No commit, push, remote publication or live migration was performed. No product decisions remain within this projection/store scope; final API/archive and database verification belongs to the integrating agent.


Coordinator results and actual local database apply are authoritative in [unified checkpoint](p6-backend-checkpoint-2026-10-30.md).
