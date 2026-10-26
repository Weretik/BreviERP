# TS-007 scoped boundary review

| ID | Severity / owner | Evidence and impact | Resolution / verification |
| --- | --- | --- | --- |
| AF-R01 | Medium / Catalog reads | ProductAdminDetailMapper calculated sale prices from mutable legacy percentages and old sewing divisions, separate from approved versioned inputs. | Remove old arithmetic/types and split metadata mapping by Sewing/PPE. Public mapper Red observed retail 150 instead of retired placeholder 0; Green passed. |
| AF-R02 | Medium / Product list | List SQL hydrated manufacturing details and fetched pricing references to run the retired formula. | Metadata-only projection and no price-reference bridge/DI registration. Unit and SQL projection regression verify metadata, media, filters and pagination. |
| AF-R03 | Medium / Admin current price selection | Confirmed non-enrollment revived an old amount. | Pure selector Red observed 999 instead of 0; blocked/onboarding state replaces fallback. Remove legacy price UI/bands and archive percent controls. |

API controllers retain binding/auth/result mapping; Domain owns the two current engines. Application metadata mappers contain no sale-price formula. Jobs call read ports/current Domain code and never version writers/publication APIs; runtime modules do not reference jobs. Each new production type has its own file and feature/topic folder. PPE diagnostic comparison, report DTOs and orchestration are separate; no generic helper/service module was added.

No schema change, destructive data conversion or automatic approval is introduced. Legacy CLR/JSON price fields remain deprecated shape-compatible placeholders; archived input records are not deleted. The release changes their semantics, so frontend current-price consumers must precede backend activation in a production rollout. No deployment is claimed. Existing source metadata editing gaps belong to the Product/profile feature, not a hidden fallback calculation.

See [reconciliation](reconciliation.md) and [evidence](evidence.md).

## Structural follow-up — 2026-10-25

AF-R04 (Low; owner: CatalogImport; TS-007; verified): `src/Jobs/CatalogImport/PpePricing/Verification/PpePricingVerificationJob.cs:19` combined source/EF reading, SKU/input lookup and diagnostic calculation. The former SQL/JSON read block now belongs to `Reading/PpePricingVerificationReader.cs`; its internal result has its own `PpePricingVerificationData.cs`. `Evaluation/PpePricingDiagnosticBuilder.cs` owns the pure per-row strict/diagnostic evaluation and comparison. The public job only orchestrates these parts and assembles report provenance. Current Domain arithmetic and Application evaluator remain the owners of price/readiness rules.

Keep decisions: comparison and diagnostic report types already have separate files in topic directories. Type-specific Product metadata mappers each map one aggregate view. The command runner owns CLI/file output and stays separate from database access. No additional project, interface or generic helper directory is warranted. Existing HTTP contracts, formula versions, source classification, batch size and no-write behavior remain unchanged. This is a structural refactor, so existing focused regression replaces a new behavior Red test. No migration or business-data write is needed.

[Structural verification](structure-verification.md).
