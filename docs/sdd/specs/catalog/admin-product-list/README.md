# Admin product category list

**Module:** catalog\
**Status:** verified as of 2026-07-27\
**Related:** `ProductCategoriesController`, `Catalog.Application/Features/ProductCategory` scenarios

See the [product glossary](../../../../product/glossary.md) for Product and category terminology.

## Goal

Provide an admin API for viewing product categories, together with create, update, delete, and store-view scenarios.

## Scope

- In scope: Catalog product categories, category hierarchy/path, CRUD contracts, and admin reads.
- Out of scope: products, media, schema changes without a separate specification, and moving categories to the Reference module.

## Contract and compatibility

- `GET /api/reference/product-categories/admin` — admin list.
- `GET /api/reference/product-categories/store/{language}` — store view.
- `POST`, `PUT {id:int}`, `DELETE {id:int}` — category write scenarios.
- The route prefix historically contains `reference`, but `Catalog` owns the domain model and Application scenarios; do not change the route without a consumer compatibility plan.

## Layer implementation

- Domain: `Catalog.Domain/ProductCategories` owns the model and category errors.
- Application: focused CQRS commands/queries, validators, DTOs, and specifications in `Catalog.Application`.
- Infrastructure: `CatalogDbContext`, EF configuration, and category seeding.
- API: `ProductCategoriesController` only binds the HTTP contract, Mediator, and result mapping.

## Acceptance criteria

- [x] Categories belong to Catalog, not Reference.
- [x] The API includes admin and store reads, as well as CRUD operations.
- [ ] Any future change to routes or response fields must separately verify consumer compatibility.

## Verification

- Manual: verify authorization, hierarchy, negative cases, and response-field stability.
- Automated: `dotnet test .\BreviERP.sln`.

## Change log

- 2026-07-27 — Documentation adapted to the actual location of categories in Catalog.
