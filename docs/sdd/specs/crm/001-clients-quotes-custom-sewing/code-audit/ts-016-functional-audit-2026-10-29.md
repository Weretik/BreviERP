# TS-016 implemented-slice functional and responsibility audit — 2026-10-29

**Scope:** only the newly implemented first-contact Domain/Application core, Host Catalog/Reference/price adapter, Reference fabric presentation metadata, additive EF migration and frozen-source import update. Existing TS-009/010 audits cover ready/PDF and are not repeated here. TS-016 is partial; Manager template/API, versioned featured configuration, quote creation and introduction PDF remain future work.

## Boundary and split/keep decision

- `Crm.Domain/Quotes/Introductions/` contains small typed layout/slot/selection/mapping/snapshot invariants. Its seven production files each own one value or aggregate rule; `IntroductionSelectionSnapshot` validates complete 4/8-slot configuration rather than performing persistence or price lookup.
- `Crm.Application/Quotes/BuildIntroduction/` separates the Catalog reader port, current-offer data, builder and prepared result. `IntroductionDraftBuilder` resolves current price each time and blocks missing metadata/URL/large tier; the earlier null-metadata exception was fixed and covered by a regression test.
- `Host.Api/Crm/IntroductionCatalogReader.cs` is one cross-module adapter. It reads current Catalog product/article, exactly one fabric from Reference and published quote price behind one Application port. Keeping these three reads together preserves a single current-offer contract; moving EF queries into Crm Domain/Application would violate module ownership. No extra interface or module is justified at this size.
- `Reference.Domain/GarmentAccessories/Entities/Fabric.cs` owns optional public composition/density alongside fabric identity, cost and supplier. EF configuration/migration remain Infrastructure; `FabricCatalogImporter` reads the two frozen-sheet columns under the existing import transaction. The import method remains one resource importer and did not gain a second scenario.
- No Catalog/Reference database entity or price formula was copied into Crm. Featured IDs are proven through `imports.CatalogSourceMappings` for the local smoke check, not hardcoded into runtime code.

## Findings and limits

No confirmed open structural or functional defect was found in the implemented TS-016 slice after the null-metadata regression repair. Full TS-016 acceptance is blocked by its planned versioned featured configuration, Manager-owned templates, quote creation, public renderer and API. Sending belongs to the later delivery phase. These missing capabilities are task scope, not evidence that the present core is complete.

Focused evidence: [TS-016 core and local migration](../evidence/ts-016-core-2026-10-29.md) records 83/83 source-hash/mapping-verified fabrics, 11/11 focused tests, Host.Api build and read-only real-price smoke for the two featured articles. This audit adds no migration or database write.
