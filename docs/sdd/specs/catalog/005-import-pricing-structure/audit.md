# Scoped structural audit — 2026-10-25

Scope: handwritten sewing composition, import, pricing and Host.Seed integration from SDD002/003. Existing unrelated working changes are preserved. Generated migrations/snapshots are excluded from size/type splitting.

| ID | Severity / status | Initial evidence and responsibility issue | Owner / remediation |
| --- | --- | --- | --- |
| AF-S01 | P2 / verified | src/Jobs/CatalogImport/Persistence/CatalogProductWriter.cs:14, 138 lines; category lookup, name/profile translation, identity persistence, composition translation and source mappings in one writer. | Jobs / RM-001: category resolver, product factory/identity writer, composition mapper/writer and a coordinator. |
| AF-S02 | P2 / verified | src/Jobs/CatalogImport/Persistence/ contains provenance records/context, catalogue writes and Reference writes at one level; CatalogImportDbContext.cs configures two entity mappings inline. | Jobs / RM-001: Catalog, Reference and Provenance owner folders; individual EF mapping files. |
| AF-S03 | P2 / verified | src/Jobs/CatalogImport/Planning/CatalogImportPlanner.cs:7, 133 lines; source identity extraction, four validation areas and source-key conventions combined. | Jobs / RM-002: dataset validators and Source identity/reference conventions. |
| AF-S04 | P2 / verified | src/Bootstrapper/Host.Seed/Program.cs:1; host lifecycle mixed with arguments, three job-specific report formats and error decisions. | Host.Seed / RM-003: dispatcher and scenario runners. |
| AF-S05 | P2 / verified | src/Jobs/CatalogImport/Pricing/ contains distinct pricing bootstrap and verification operations inside the catalogue import assembly. | Jobs / RM-004: dedicated SewingPricing project, retaining a one-way provenance dependency. |
| AF-S06 | P2 / verified | Pricing/Verification/SewingPricingVerificationJob.cs:20 mixes database setup, Article diagnostic reads, arithmetic acknowledgement and source-field comparison; Bootstrap/PricingBootstrapJob.cs:16 mixes transaction flow and immutable-version reconciliation. | Jobs / RM-004: separate diagnostic reader, parity comparer and baseline reconciler. |
| AF-S07 | P2 / verified | Catalog.Domain/Pricing/Sewing has 13 flat settings/input/result/calculation types; Application Contracts/Pricing and Infrastructure pricing owners also mix topics. Product.SewingComposition.cs includes identity and read policy alongside composition writes. | Catalog/Reference / RM-005: semantic folders and aggregate partials, stable CLR namespaces. |

Small cohesive handlers/adapters are not split merely to lower line counts. Domain and Application already respect the module dependency direction; public wire contracts and database structure require no change. Transaction scope remains explicit in coordinators rather than being hidden in a new generic unit-of-work abstraction.

All AF-S01–07 findings are verified through the mapped remediation tasks. See [fresh verification and final paths](verification.md). No formula, public contract or persistent schema/data change was introduced.
