# Draft quote price-basis protocol (P2 design checkpoint)

**Status:** design for EN-002/TS-003/004; no endpoint or table is implemented by this document. The quote contract must encode these decisions before TS-003 transport work.

## Server ownership

Crm accepts typed selection intent for one or more ordered variants. The request contains sellable Sewing Product and Article IDs together, or a PPE Product ID. A one-off PPE item carries an explicit positive sale amount and reason. A catalog PPE override carries tier, positive amount and nonblank reason. Sewing totals, costs, coefficients, source version IDs, actor and document numbers are never client authority. Unsupported custom and migrated kinds cannot be priced by the P2 preview; their later adapters are TS-006 and TS-013.

A Host adapter resolves Catalog's public Application contracts. For Sewing, it reads the current approved publication **and its saved calculation snapshot**, then checks Product/Article identity, active selection and the calculation's composition/settings/reference versions. Reading tier values from `PublishedSewingPrice` alone loses provenance. For PPE, it resolves approved offer and rule versions and uses the existing `PpeProductPricingEvaluator`. No Crm-owned arithmetic or internal HTTP loopback is allowed. The adapter returns typed semantic tiers, blockers and source IDs; a failed or missing source is a blocker, never zero.

## Basis and token

The preview operation stores an immutable `CalculationBasis` in Crm: opaque UUID, schema/formula versions, ordered normalized input hash, actor ID, quote ID if editing, resolved source version IDs, server monetary results and blockers, created/expiry instants. The token is the opaque UUID; the server record is its authority. The server fixes a short validity window in the contract (proposed 15 minutes), verifies actor, quote scope, exact normalized intent hash and expiry on save, and compares the current publication/approved offer/rule identities with those used by the preview. A newer unpublished Reference cost does not silently override a published Sewing price. A requestId scopes preview replay; identical actor/operation/key/payload returns the prior basis, while changed payload with the same key conflicts. The later immutable QuoteVersion copies the complete basis and prices so expiry never changes historical reads.

Text-only edits to a saved draft copy the old monetary basis and do not call pricing. A changed price input requires a fresh basis. Explicit reprice after a source change belongs to TS-005; opening, reading and copying saved content never call this adapter. Server-side save validates the whole graph before inserting a version and atomically moving the root pointer.

## Precision and readiness

All money uses decimal and the existing Catalog two-place AwayFromZero displayed prices; source breakdown retains its Catalog precision. Sewing tier codes have source quantity ranges 1–9, 10–40, 41+. PPE has three semantic codes without an inferred quantity cutoff. The quote draft stores currency and tax provenance without adding VAT. Customer-ready tax wording, issuer configuration and numbering remain EN-005/TS-009/010 gates. A draft may retain blockers and missing prices as null; a ready quote may not.

## Contract and verification checkpoints

Freeze the preview request/response, basis token, expiry, blocker paths and 409 replay/stale-source semantics in canonical Crm OpenAPI. Focused tests must prove the 9/10/40/41 sewing boundaries, approved PPE source precedence, 452/453 ordinary composition, 724/931 blockers, actor/input/quote-scope forgery rejection, replay and expiry, source drift and text-only no-reprice. A clean backend commit is needed for the frontend snapshot under the existing contract workflow; this document does not authorize one.
