# TS-006 — versioned PPE pricing

Authorized scope: R-007/008/010, SC-006; backend pricing, additive persistence and explicit local source bootstrap, followed by Admin consumer integration. No quote rewrite or live XML lookup. The workbook is a read-only arithmetic/data source, not a runtime service.

## Verified source and decisions

Read 2026-10-25: workbook `1BMl9jZLprY-15TfErBi9pbv9e4TVq8DUdjlZ4HvCxRE`, `Налаштування цін!J1:M11` (ten rules), `ЗІЗ!A1:R150` (119 identities). Formula evidence remains in the imported frozen catalogue. Rule names match exactly; no category guessing. Generic `Одяг ЗІЗ` and `Захист голови` are absent from the rule table, but their source Products have both external prices and need no fallback. Twenty-six offers have both Insight prices. Preserve source review/date separately from Brevi approval.

Each tier selects its source independently. Positive approved external retail wins retail; when both external values are positive, wholesale is their mean rounded away from zero to two decimals; positive external wholesale wins large wholesale. Every remaining tier requires a positive supplier cost and exactly one approved scoped rule. Rule absence/ambiguity does not block tiers fully supplied by external data. Unapproved external values are ignored, so an approved fallback can still work. Invalid/negative inputs, overflow, inactive Product or incomplete required fallback return typed blockers and no price object. Zero is never an error sentinel. External endpoints preserve given amounts; only mean/fallback apply ROUND to two decimals. Fractions stay fractions; no additional VAT.

## Data model and ownership

Catalog owns two append-only Infrastructure persistence records: `PpePricingRuleVersions` and `PpePricingOfferVersions`. Internal sequence keys are long; public version identities and idempotency request keys are UUIDs. Persistence records never escape Infrastructure. Domain owns pure amount/markup inputs, validation and arithmetic, without storage or HTTP. Product ownership uses an FK to existing Catalog Products. Rule code is an exact business key, not an inferred category.

Each version stores its typed input values in a jsonb snapshot; indexed scope, identity, request hash and approval metadata are relational columns. This preserves exact approved input values without a second mutable cost table. Rules retain code, three markup fractions, source, source date, approval actor/time and creation time. Offers retain Product, rule code, supplier cost, two external values, source/source-review evidence and Brevi approval actor/time. Imported inputs are drafts; the source-reviewed flag never fabricates a Brevi approval. A manager appends an approved version with an expected current version UUID and request UUID. A replay with the same actor and payload returns the original version; changed payload or outdated expected version returns conflict. Writes serialize within a Catalog transaction. Queries read latest versions in a bounded Product batch and compute on the server. Calculation responses include the exact selected version IDs; no saved quotation/publication is fabricated.

## API and compatibility

Add separate `/api/v1/pricing/ppe` operations for Product pricing reads, bounded batch reads, current rule reads, rule version creation and Product offer version creation. Reads require CatalogRead; writes require CanManageProducts and a server-derived user ID. Import never creates legacy two-percent details. Legacy Product contract is unchanged; enrolled PPE consumers select the new result, including blocked state, without falling back to old amounts. Legacy products without versioned input remain compatible until explicit enrollment.

## Tasks and tests

- TS-006A: pure formula/precedence, missing/ambiguous rule, rounding and overflow — unit.
- TS-006B: two version tables, append/replay/conflict, bounded reads — PostgreSQL integration.
- TS-006C: authenticated typed API and canonical OpenAPI — API/contract.
- TS-006D: local-only repeatable bootstrap of frozen source as draft inputs, exact identity reconciliation, rollback-safe transaction — integration. No implicit approval.
- TS-006E: clean backend checkpoint, generated frontend contract, Admin current three-tier/blocker/review UI and Product projection — focused client tests.

Deployment: migrate with Host.Seed before API; import is an explicit job mode after migration, not API startup. Rollback selects previous versions by appending a corrected version; preserve history. A database backup is required before local real-data execution. Do not drop the additive tables after approvals/edits. Verify empty and upgrade migration paths in isolated test databases. Record exact execution and remaining work in ppe-evidence.md.

[Structural follow-up](structure/ppe/audit.md): contracts are grouped by Rules/Offers/Products; the import keeps orchestration separate from identity/replay/write adapters. No accepted business behavior changes.
