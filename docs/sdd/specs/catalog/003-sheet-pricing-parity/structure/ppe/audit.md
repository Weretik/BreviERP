# PPE structural audit — 2026-10-25

User-authorized responsibility audit and refactoring of TS-006. Scope: PPE Domain/Application/API/Infrastructure, local bootstrap and their integration boundaries. Rules: docs/sdd/standards/backend-rules.md (layers, CQRS, cohesive types and file organization). No pricing/scenario/API/schema change is intended.

| ID | Severity / owner | Original evidence / impact | Target and dependency | Status |
| --- | --- | --- | --- | --- |
| AF-PPS01 | Low / PPE import | `src/Jobs/CatalogImport/PpePricing/Bootstrap/PpePricingBootstrapJob.cs:26` owned Product identity lookup, baseline replay validation and draft record creation alongside transaction/job orchestration. Those independent adapter changes required editing the same job. | TS-006D: `Persistence/PpePricingProductResolver.cs`, `PpePricingBaselineVerifier.cs`, `PpePricingDraftWriter.cs`; job retains local-target guard, source/plan, lock and transaction lifecycle. | Verified |
| AF-PPS02 | Low / Application contracts | `src/Modules/Catalog/Catalog.Application/Catalog.Application/Contracts/Pricing/Ppe/` had nine direct DTO/port files for three capabilities. Navigation did not distinguish rule edits, offer edits and Product reads. | TS-006B/C: group into Rules, Offers and Products, three files each. Public namespaces/type names remain unchanged as in existing pricing contract organization. | Verified |

Keep decisions: Domain arithmetic and blocker validation have one calculation responsibility. Version writers remain separate by rule/offer, each owning one atomic append scenario. `PpePricingInputReader` keeps its bounded three-query projection and repeatable-read snapshot together; no formulas or commands are hidden in this reader. API DTO/controller ownership is already separate. Persistence record namespaces/EF mappings and public contracts are unchanged; no migration is needed.

All collaborators use the same CatalogDbContext and the job's existing advisory lock/transaction. Draft writer does not commit independently. Replay checks still accept the unchanged original baseline after later approved versions and reject changed/incomplete source. No new module/project or generic Common/Utils layer was introduced. [Verification](verification.md).
