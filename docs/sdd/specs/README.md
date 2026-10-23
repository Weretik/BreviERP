# Feature specifications

This is an index of project decisions and delivery; the [implemented feature map](../features/README.md) points to the architecture of the current code. Check each item's README and code for its status.

| Module | Specifications |
| --- | --- |
| Catalog | [product and product page — current SDD](catalog/001-product-page/README.md), [early historical product snapshot](catalog/product-page/README.md), [admin product category list](catalog/admin-product-list/README.md), [media files](catalog/media-files/README.md) |
| Platform | [module schemas](platform/001-module-schemas/README.md) |

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

## Planned Catalog migration slice

- [Sewing articles and calculation composition](catalog/002-sewing-articles-and-composition/README.md) — domain/schema design is ready; data import awaits database access.
