# Feature specifications

This is an index of project decisions and delivery; the [implemented feature map](../features/README.md) points to the architecture of the current code. Check each item's README and code for its status.

| Module   | Specification                                                                                            | Status                                                                                                             |
| -------- | -------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| Catalog  | [001 product and product page](catalog/001-product-page/README.md)                                       | complete                                                                                                           |
| Catalog  | [002 Sewing Articles and calculation composition](catalog/002-sewing-articles-and-composition/README.md) | implemented; local migrations and catalogue import verified 2026-10-25                                             |
| Catalog  | [003 Brevi-owned product pricing (sheet parity)](catalog/003-sheet-pricing-parity/README.md)             | Sewing and PPE pricing implemented and verified locally 2026-10-25                                                 |
| Catalog  | [004 Sewing composition structure](catalog/004-sewing-composition-structure/README.md)                   | structural remediation complete 2026-10-25                                                                         |
| Catalog  | [005 Import and pricing responsibility decomposition](catalog/005-import-pricing-structure/README.md)    | structural remediation verified                                                                                    |
| Catalog  | [Admin product category list](catalog/admin-product-list/README.md)                                      | verified 2026-07-27                                                                                                |
| Catalog  | [Media files](catalog/media-files/README.md)                                                             | verified                                                                                                           |
| Catalog  | [Early product snapshot](catalog/product-page/README.md)                                                 | history; superseded by 001                                                                                         |
| Crm      | [Clients, quotes and custom sewing](crm/001-clients-quotes-custom-sewing/README.md)                      | P1–P3 backend/Admin; P4 TS-006/EN-003/TS-008 verified; TS-007 and FE-006/007 remain |
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

Use the module folder, for example `catalog`, `reference`, `identity`, `accounting`, `crm`, `platform`, or `cross-module`. Feature and phase names must be lowercase kebab-case.

Gradually migrate existing specifications to this structure: preserve completed task IDs and verification history unless the user requests a complete feature redesign. For data or schema migrations, explicitly describe the baseline and rollback in `design/`; put rollout and verification in `tasks/`. Keep machine-readable public API contracts in [`docs/sdd/contracts/`](../contracts/README.md). Link to architecture and standards instead of copying them.

## Lifecycle

Draft → accepted → in progress → verified → complete.

Implementation must not silently diverge from an accepted specification. If that happens, add a dated change entry with the reason, boundary impact, and new verification.
