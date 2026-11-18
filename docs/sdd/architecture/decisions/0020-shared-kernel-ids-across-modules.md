# ADR-0020: Ids referenced across modules live in the shared kernel

**Status:** accepted · **Date:** 2026-11-18 · **Owner:** Виталий (project owner) · **Extends:** [ADR-0006](0006-module-contracts.md), [ADR-0019](0019-module-graph-without-cycles.md)

## Context

Since 2026-11-17 every aggregate root has its own id type and every reference inside its module uses it. References to another module's aggregate were still plain numbers: about sixty fields such as `Quote.AuthorUserId`, `ProductionOrder.QuoteId`, `ProductionOrder.CustomerId`, `QuoteItem.ProductId`, media file ids in Catalog and Content. The compiler did not stop a user id going where a customer id belongs. The module rules listed this as a known deviation.

A module's `Domain` may reference only `BuildingBlocks.Domain`; it cannot reference another module's `Contracts`, so the owning module cannot hand its id type to the others from there.

## Options

1. **Keep plain numbers for references across modules.** Rejected: the owner's rule is to fix violations, not to keep them.
2. **Each module wraps the other module's number in its own type** (Catalog had `FabricReferenceId`, Crm had `CrmUserId`). Rejected: the same id gets a different type in every module, and a value passed between modules is unwrapped and wrapped again.
3. **The id type lives in the shared kernel `BuildingBlocks.Domain` (chosen).** One type per id, used by the owning module and by every module that references it.

## Decision

- An id that another module's domain stores goes to `BuildingBlocks.Domain/Identifiers/<Group>/` (namespace `BuildingBlocks.Domain.Identifiers.<Group>`: `People`, `ReferenceData`, …). It is the same hand-written `readonly record struct <Name>Id(<type> Value)` as any id, with `IComparable<T>`, `FromNullable(...)` for optional values coming from contracts, and `ToString()` printing the bare value. The owning module uses this type for its own key too; the type has no rules of its own, so the kernel stays free of any module's concepts.
- An id that no other module's domain stores stays beside its root in the module.
- `TypedIdConventions.AddTypedIds(<domain assembly>)` maps the kernel ids as well, so no module writes a converter for them; the columns stay the same numbers, with no schema change.
- `Contracts`, commands, queries, DTOs and HTTP keep plain numbers. Application wraps a number on the way in (`new UserId(command.UserId)`, `UserId.FromNullable(...)`), queries compare typed values (`x.OwnerUserId == new UserId(userId)`), and `.Value` is read only in projections.
- The kernel folder is split by meaning (people, reference data, products and media, orders), as every folder past six files is.

The move is done in stages, one pull request each: people (`UserId`, `CustomerId`), reference data (fabrics, suppliers, accessories, operations, sections; the Vogen types and Catalog's wrappers go), products and media (`ProductId`, `ArticleId`, `MediaFileId`), quotes and production orders (`QuoteId`, `ProductionOrderId` and their item ids). The [module rules](../../standards/module-rules.md#domain-model-ddd) keep the list of what is left as a known deviation until the last stage merges.

## Consequences

- Stage 1 (2026-11-18): `UserId` (a staff account of Identity) and `CustomerId` (moved from Crm) are kernel types; Crm's `CrmUserId` is gone; user and customer references in Crm, Production, Warehouse and Reminders use them.
- Stage 2 (2026-11-18): `FabricId`, `SupplierId`, `GarmentAccessoryId`, `GarmentPartOperationId` and `ProductSectionId` left Vogen and Reference for `Identifiers/ReferenceData/`; Catalog's `FabricReferenceId`, `AccessoryReferenceId`, `OperationReferenceId` and `SupplierReferenceId` are gone; fabric and supplier references in Catalog, Crm, Production, Purchasing and Warehouse use them. Catalog and Reference now map ids with `AddTypedIds` too, so their hand-written converters for these ids are gone.
- Stage 3 (2026-11-18): `ProductId`, `ArticleId` (Catalog) and `MediaFileId` (Media) live in `Identifiers/Products/`; Catalog's own copy of `MediaFileId` is gone; product, article and photo references in Content, Crm and Production use them. A not yet saved article still gets a temporary negative `ArticleId` from Catalog's value generator.
- Stage 4 (2026-11-18): `QuoteId`, `QuoteItemId`, `QuoteOptionId` (Crm) and `ProductionOrderId`, `ProductionItemId` (Production) live in `Identifiers/Orders/`; Crm's and Production's own copies are gone; the quote references in Production and the order and item references in Warehouse use them. The known deviation in the module rules is closed.
- Behaviour, HTTP API, OpenAPI and data: unchanged. `ModuleDataIsolationTests.Each_module_model_matches_its_latest_migration_snapshot` proves the schema did not change.
- A kernel id type is never deleted while a module stores it; renaming one changes every module that uses it in one pull request.
