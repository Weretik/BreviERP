# 001 — Product page

**Module:** Catalog\
**Status:** complete\
**Owner:** Catalog\
**Created:** 2026-07-27

Create and evolve products of two types: Sewing and PPE. This specification describes shared Product data, type-specific rules, the storage model, and delivery history.

Shared terms are defined in the [product glossary](../../../../product/glossary.md).

## Requirements

1. [Overview, scope, and open questions](requirements/overview.md)
2. [Product descriptive data](requirements/product-content.md)
3. [Sewing: operations, fabrics, and accessories](requirements/sewing.md)
4. [PPE: supplier and coefficient](requirements/ppe.md)
5. [Pricing](requirements/pricing.md)

## Technical design

1. [Domain: model, calculations, and Reference integration](design/domain.md)
2. [Infrastructure: persistence, EF Core, and migrations](design/infrastructure.md)
3. [Current technical baseline](design/implementation-baseline.md)
4. [Data model](data-model.md)
5. [Admin list query performance](design/admin-list-query-performance.md)
6. [Narrow Reference pricing reader for the admin list](design/admin-list-pricing-reference-reader.md)

## Delivery

1. [API contract](contracts/api-contract.md)
2. [Checklist: specification readiness](checklist/spec-readiness.md)
3. [Checklist: delivery readiness](checklist/delivery-readiness.md)

## Frontend handoff

- [Product administration scenario: screens, sections, actions, and API](../../../frontend-scenario/catalog/001-product-page/admin-product-scenario.md)

## AI implementation tasks

Full status and navigation: [tasks/README.md](tasks/README.md). The main phases `00–05` only create and order subphases. AI performs one exact `NN.N-*.md` subphase file at a time, marks a completed task `[x]`, and does not start another file without an explicit instruction. `[P]` means the task can run in parallel with other `[P]` tasks in the same subphase after its dependencies are complete.

| Phase | Input | Result |
| --- | --- | --- |
| [00 — Readiness](tasks/00-readiness.md) | requirements, open questions | `00.N` subphases for scope and tools are created and completed |
| [01 — Domain](tasks/01-domain.md) | 00 | `01.N` Product Domain subphases are created and completed |
| [02 — Infrastructure](tasks/02-infrastructure.md) | 01 | `02.N` persistence/migration subphases are created and completed |
| [03 — Application](tasks/03-application.md) | 01, 02 | `03.N` CQRS slices, including enriched list/detail Media URLs and prices for the admin frontend |
| [04 — API](tasks/04-api.md) | 03 | `04.N` controllers, HTTP behavior, OpenAPI, and API tests — complete |
| [05 — Verification](tasks/05-verification.md) | 00–04 | `05.N` build, test, and delivery evidence subphases — complete |

## Decision log

- 2026-07-27 — Recorded the Domain and persistence baseline; the specification remains open for expansion.
- 2026-08-07 — For the 480-minute calculation, `piecesPerShift` is passed as the full fractional value without rounding; no separate completed-pieces metric is produced. A draft without operations and the current operation duration without a snapshot remain valid.
- 2026-08-10 — Sewing price tiers 1–10, 11–39, and 40+ units use `AdditionalReference` keys `profit_10`, `profit_10_40`, and `profit_40`, respectively; `coefficient_ciz_*` keys belong to PPE and do not apply to Sewing.
- 2026-07-27 — Defined Description, Information, Characteristics, fabrics, accessories, `MetersPerProduct`, PPE supplier, and coefficient.
- 2026-07-27 — Recorded three Sewing price tiers and two PPE tiers; formulas were moved to a separate decision.
- 2026-07-27 — Adopted the composition persistence model: Products plus separate Sewing/PPE details and Sewing links.
- 2026-08-19 — Changed the admin frontend read contract: the list returns only the main photo with its URL and `minimumWholesalePrice`; detail returns URLs for all photos and all prices. Added corrective subphase `03.8` and dependent API/OpenAPI/test tasks.
