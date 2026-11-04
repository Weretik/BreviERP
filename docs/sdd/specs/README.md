# Feature specifications

This is an index of project decisions and delivery; the [implemented feature map](../features/README.md) points to the architecture of the current code. Check each item's README and code for its status.

| Module   | Specification                                                                                            | Status                                                                                                             |
| -------- | -------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| Catalog  | [001 product and product page](catalog/001-product-page/README.md)                                       | complete                                                                                                           |
| Catalog  | [002 Sewing Articles and calculation composition](catalog/002-sewing-articles-and-composition/README.md) | implemented; local catalogue import verified; dedicated cloud import locally verified 2026-11-02, not deployed                                             |
| Catalog  | [003 Brevi-owned product pricing (sheet parity)](catalog/003-sheet-pricing-parity/README.md)             | Sewing and PPE pricing implemented and verified locally 2026-10-25                                                 |
| Catalog  | [004 Sewing composition structure](catalog/004-sewing-composition-structure/README.md)                   | structural remediation complete 2026-10-25                                                                         |
| Catalog  | [005 Import and pricing responsibility decomposition](catalog/005-import-pricing-structure/README.md)    | structural remediation verified                                                                                    |
| Catalog  | [006 «Товари» page from the Calculator sheet](catalog/006-products-page/README.md) | backend implemented 2026-11-03 |
| Catalog  | [007 Sewing product card](catalog/007-sewing-product-card/README.md) | backend implemented 2026-11-03 |
| Catalog  | [008 PPE product card](catalog/008-ppe-product-card/README.md) | backend implemented 2026-11-03 |
| Catalog  | [009 Product creation from the Admin pages](catalog/009-product-create-pages/README.md) | backend implemented 2026-11-03 |
| Catalog  | [010 Product import from the Calculator sheet](catalog/010-sheet-product-import/README.md) | backend implemented 2026-11-03, not applied |
| Catalog  | [011 Prices on the «Товари» list and recalculation](catalog/011-product-list-prices/README.md) | backend implemented 2026-11-03; recalculation superseded by 012 |
| Catalog  | [012 Automatic Sewing price publication](catalog/012-sewing-price-auto-publication/README.md) | backend implemented 2026-11-03 |
| Catalog  | [013 «Товари» list in one server request](catalog/013-product-list-page/README.md) | backend implemented 2026-11-03 |
| Catalog  | [014 No review or check marks](catalog/014-remove-review-marks/README.md) | backend implemented 2026-11-03 |
| Catalog  | [015 Fabric variants](catalog/015-fabric-variants/README.md) | backend implemented 2026-11-03 |
| Catalog  | [016 Sewing card in one request](catalog/016-sewing-card-one-request/README.md) | backend implemented 2026-11-04 |
| Content  | [001 «Контент сайту» site content](content/001-site-content/README.md) | planned only (draft) |
| Quotes   | [001 «КП» commercial proposals](quotes/001-quote-calculation/README.md) | specification 2026-11-04; stage 1 next |
| Catalog  | [Admin product category list](catalog/admin-product-list/README.md)                                      | verified 2026-07-27                                                                                                |
| Catalog  | [Media files](catalog/media-files/README.md)                                                             | history; moved to Media MED-001                                                                                    |
| Catalog  | [Early product snapshot](catalog/product-page/README.md)                                                 | history; superseded by 001                                                                                         |
| Crm      | [Clients, quotes and custom sewing](crm/001-clients-quotes-custom-sewing/README.md)                      | P1–P5 locally verified; P6 Gmail deferred; P7 offline tooling verified, historical source/gap/owner readiness remains gated |
| Crm      | [002 Clients from KeepinCRM](crm/002-keepincrm-clients/README.md) | backend implemented 2026-11-02 |
| Reference | [001 Suppliers from the Calculator and KeepinCRM](reference/001-suppliers-keepincrm/README.md) | implemented 2026-11-02; live KeepinCRM run pending secret |
| Reference | [002 Fabrics and accessories from the Calculator sheet](reference/002-fabrics-accessories-sheet/README.md) | implemented 2026-11-02 |
| Reference | [003 Operations from the Calculator sheet](reference/003-operations-sheet/README.md) | implemented 2026-11-02 |
| Reference | [004 Pricing parameters from the Calculator sheet](reference/004-pricing-parameters-sheet/README.md) | backend implemented 2026-11-02 |
| Reference | [005 Product sections from the Calculator sheet](reference/005-product-sections-sheet/README.md) | backend implemented 2026-11-03 |
| Media | [001 «Фото» photo library with folders](media/001-media-library/README.md) | stage 1 backend implemented 2026-11-03 |
| Reminders | [001 Telegram deadline reminders](reminders/001-telegram-reminders/README.md) | implemented and locally verified 2026-11-01; not deployed |
| Identity | [001 Cookie sign-in like Kedr](identity/001-cookie-sign-in/README.md) | implemented 2026-11-03; not deployed |
| Platform | [001 module schemas](platform/001-module-schemas/README.md)                                              | implementation                                                                                                     |

When a specification's status changes, update its row here in the same change.

Every non-trivial change starts here before implementation.

For a new or changed specification, use `$backend-feature-spec` and the [delivery rules](../standards/delivery-rules.md). The structure below defines where BreviERP project specifications belong; add specific documents according to the feature's content.

For shared business terms Product, Sewing, and PPE, use the [glossary](../../product/glossary.md) instead of creating new definitions in each specification.

## Structure

```text
docs/sdd/specs/
  <module>/
    <NNN>-<feature-slug>/
      README.md                    orchestration feature
      requirements/                behavior and rules
      design/                      backend decisions
      data-model.md                data model
      contracts/                   API/integration contracts
      traceability.md              scenario, task, and evidence links
      tasks/                       AI phases and atomic tasks
      checklist/                   quality gates
```

Use the module folder, for example `catalog`, `reference`, `identity`, `accounting`, `crm`, `reminders`, `platform`, or `cross-module`. Feature and phase names must be lowercase kebab-case.

Gradually migrate existing specifications to this structure: preserve completed task IDs and verification history unless the user requests a complete feature redesign. For data or schema migrations, explicitly describe the baseline and rollback in `design/`; put rollout and verification in `tasks/`. Keep machine-readable public API contracts in [`docs/sdd/contracts/`](../contracts/README.md). Link to architecture and standards instead of copying them.

## Lifecycle

Draft → accepted → in progress → verified → complete.

Implementation must not silently diverge from an accepted specification. If that happens, add a dated change entry with the reason, boundary impact, and new verification.

- [Platform: temporary anonymous API access](platform/002-temporary-anonymous-api/README.md) — history; removed by [identity 001](identity/001-cookie-sign-in/README.md).
