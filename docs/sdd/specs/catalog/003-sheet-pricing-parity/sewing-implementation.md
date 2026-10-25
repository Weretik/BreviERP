# Sewing calculation slice — implementation scope

This page retains the initial EN-001/002/003 and TS-001/002/003 scope. The subsequent backend publication stage is implemented in [publication-implementation](publication-implementation.md), with [current evidence](publication-evidence.md); TS-005 consumer cutover and PPE remain separate.

## Scope and acceptance

The user authorized the next implementation stage after local catalogue import: EN-001/002/003 and TS-001/002/003, including settings in PostgreSQL, Reference input versions, sewing Article cost calculation and three tiers. This slice does not switch the existing HTTP consumers or publish prices; TS-004/005 and PPE TS-006 remain subsequent tasks. The strict Application calculation must return blockers for imported unreviewed lines; diagnostic arithmetic parity is separate and cannot publish.

- SC-S1: Calculate each applicable shared/Article material and each operation occurrence using decimal arithmetic, multiplicative labor rates and setting-driven schedule/assembly/overhead.
- SC-S2: Resolve approved effective settings and Reference input versions by an explicit instant; reject missing/ambiguous versions, unit mismatch, inactive inputs, unreviewed required lines and absent positive operation time. OtherCost has Reference ownership and an explicit cost version.
- SC-S3: Return rounded currency tiers 1–9, 10–40 and 41+, with boundaries 9/10/40/41 and no intermediate monetary rounding.
- SC-S4: Bootstrap one immutable settings revision and Reference input revisions from the frozen, mapped catalogue plus the approved settings snapshot. Repeat the same bootstrap without duplicates; do not mutate catalogue composition or approve its lines. Changed bootstrap source requires a separately reviewed next-version workflow.
- SC-S5: Verify the imported 246/247 inputs against the frozen displayed result and each cost component. A diagnostic report contains arithmetic plus actual review blockers; it is never a published-price result.

## Ownership and schema decisions

Catalog/Pricing owns immutable SewingPricingSettingsVersion with typed generated long ID, revision, effective date, source SHA-256, source payload, approval state and explicitly mapped decimal setting columns. Fractions and price multipliers are separate fields. Formula implementation is code-versioned as sewing-sheet-v1; MINUTE_COST is derived, not an independent setting.

Reference owns immutable ReferencePricingInputVersion with a typed generated long ID, typed optional Fabric/Accessory/Operation IDs or an OtherCost code (exactly one), value/unit, activity/approval, effective date, revision and source provenance. A missing operation default remains nullable; positive manual minutes can resolve it. Costs are UAH per stored material unit; operation values are minutes per occurrence. A record's source date is evidence, not an invented historical effective date.

These are additive migrations in the existing Catalog and Reference DbContexts. Existing mutable Reference prices/norms remain compatible; this first slice reads versioned inputs. Automatic version creation from Reference edit APIs is outside this bootstrap/calculation slice and must precede production consumer cutover. Existing Product price DTOs and legacy algorithms are not redirected until TS-005.

## Operational boundaries

Host.Seed remains the migration runner. A separate local pricing bootstrap/verification job reuses the committed catalogue batch and SourceIdMappings, validates the snapshot hash and exact mapped Reference values, then writes settings/input versions under one transaction. The source values are explicitly accepted as the initial calculation baseline by the user's instruction; composition review is not implicitly granted. EffectiveFrom is the bootstrap date supplied in the source, not the old spreadsheet update serial.

A pre-change local backup is required before applying migrations/bootstrap. Normal rollback retains additive schema and versions; no destructive Down is executed with data. Empty-schema and existing local upgrade paths, rerun behavior and actual calculation inputs are verified. No cloud write, automatic publication or browser run is in this scope.

## Precision and source corrections

Use decimal throughout, round only displayed tier prices to 2 decimals with MidpointRounding.AwayFromZero; breakdown remains unrounded. No extra VAT multiplier. Manual time overrides the standard when explicitly provided, preserving Brevi's richer composition behavior. Unknown manual-only source operations retain their supplied minutes rather than being dropped by a spreadsheet lookup. Every required line is validated; source lookup shortcuts are not copied.

The current imported MOD-025 fixture has 13 operation occurrences and total 84.84 minutes. Source material totals (including OtherCost) are 277.99/544.39; full costs are 754.6707393292684/1021.0707393292685; visible prices are 1169.74/996.17/943.34 and 1582.66/1347.81/1276.34. Earlier comparison values remain historical evidence of the 12-operation source.

## Planned file map

- Catalog.Domain/Pricing/Sewing: immutable input/value records, settings entity, formula, blockers and tier policy, one primary type per file.
- Catalog.Application/Features/Pricing/CalculateSewingArticle: one read scenario and private DTOs; Contracts/Pricing defines persistence and cross-module input boundaries.
- Catalog.Infrastructure/Pricing: settings/composition projections and EF mappings.
- Reference.Domain/Pricing and Reference.Infrastructure/Pricing: versioned inputs, mapping and effective-value projection.
- Jobs/SewingPricing: local bootstrap and diagnostic verification adapters; Host.Seed only dispatches explicit job modes.


The subsequent [structural decomposition](../005-import-pricing-structure/README.md) records current folder owners and verification without changing this behavior.
