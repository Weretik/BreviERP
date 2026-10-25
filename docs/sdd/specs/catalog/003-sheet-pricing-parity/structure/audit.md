# Pricing publication structure audit

Scope: the implemented EN-004 / TS-004 backend, its calculation dependencies, API entry points, Reference revision adapters and directly affected tests. User authorization includes both audit and structural fixes. Existing dirty import/composition work is preserved. No scenario, route, JSON shape, formula, schema or approval policy is changed.

## Findings and remediation

| ID | Severity / owner | Verified starting evidence | Target change | Status |
| --- | --- | --- | --- | --- |
| AF-PS01 | Medium / Catalog Infrastructure | `Pricing/History/Writing/SewingPricingHistoryStore.cs` (105 lines) owned transaction locks, saved calculations, publications and snapshot serialization. Its Application interface exposed all eight operations to read-only handlers. | Split into `History/Calculations/SewingCalculationStore.cs`, `History/Publications/SewingPublicationStore.cs`, `History/Transactions/SewingPricingTransaction.cs` and the calculation snapshot serializer. Application contracts now follow these owners. Persisted records/configurations moved with their owner; scoped DbContext remains shared. | Verified |
| AF-PS02 | Medium / Catalog Application | `Features/Pricing/CalculateSewingArticle/DTOs/SewingArticlePricePreview.cs` and `Inputs/SewingPricingInputResolution.cs` were used by save/publish/evaluation outside that query. Seven sibling scenario directories mixed settings, history and previews without a Sewing boundary. | Shared snapshot now belongs to `Contracts/Pricing/SewingCalculation`; input resolution belongs to `Features/Pricing/Sewing/Evaluation`. Sewing scenarios are grouped into Calculations, Publications, Settings and Preview. Non-persisted namespaces and consumers updated. | Verified |
| AF-PS03 | Medium / Reference Infrastructure | `Pricing/InputVersions/Writing/ReferencePricingChangeAppender.cs` detected EF changes, extracted three legacy source values, selected version history and appended revisions. The same Writing area contained the separate OtherCost command adapter. | `Pricing/InputVersions/Changes/TrackedReferencePricingChanges.cs` discovers values; `ReferencePricingTrackedChange.cs` retains original/current values and the typed owner predicate; `ReferencePricingChangeAppender.cs` handles version append. Explicit cost revision moved to `Pricing/OtherCosts/Writing`. | Verified |
| AF-PS04 | Low / API | Sewing pricing controllers and request DTOs sat in broad Controllers/Contracts folders, apart from their Application scenario structure. | Controllers now belong to `Controllers/Pricing/Sewing`; requests belong to `Contracts/Pricing/Sewing/{Calculations,Settings}`. Routes, policies and serialization remain intact. | Verified |

The evaluator, settings writer, OtherCost writer, controllers and Domain calculator each have one coherent purpose; their length alone is not a reason to split methods into tiny services. No new runtime project or business module is needed: Catalog owns prices/history, Reference owns input versions, and the Host adapter owns cross-module enlistment.

## Verification plan

Use existing tests as before/after characterization: calculation parity and contracts, saved/blocked/stale publication, retries/history, concurrent Reference edits, rollback, precision and HTTP authorization. Run the narrow architecture filters and confirm Catalog/Reference EF models have no pending change. Do not create or apply migrations, rerun import/bootstrap, publish real prices, commit, or change frontend consumers in this structural task. See [verification](verification.md) for fresh results.

All four scoped findings are resolved with [fresh evidence](verification.md). No unresolved finding remains within this structural scope. The [Catalog feature map](../../../../features/catalog/sewing-pricing.md) and [Reference feature map](../../../../features/reference/pricing-input-versions.md) provide current full source trees.
