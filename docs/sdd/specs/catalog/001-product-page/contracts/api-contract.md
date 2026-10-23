# Product page — API contract

**Status:** implemented and verified by automated tests; manual Swagger/API scenarios remain open. Machine-readable source of truth: [product-catalog.openapi.yaml](../../../../contracts/catalog/product-catalog.openapi.yaml).

**Versioning:** all endpoints in this delivery use the `/api/v1` prefix.

## Purpose

This document records the contract boundaries. The machine-readable source of truth for the frontend is `docs/sdd/contracts/catalog/product-catalog.openapi.yaml`; this file records decisions about audience, versioning, and compatibility.

Swagger UI is not the contract; it is only an interface for viewing OpenAPI. The frontend can work without it by using versioned OpenAPI YAML/JSON to generate types and a client, for contract testing, and for local documentation.

## Accepted boundaries

The current HTTP contract is for the admin frontend only. The admin detail/read model must provide all data the manager can configure:

- shared Product fields, photos, and categories;
- localized `Description`, `Information`, and `Characteristics`;
- for Sewing: `MetersPerProduct`, fabrics with primary flag and order, accessories, operations, `piecesPerShift`, three prices for each fabric, and min/max for each price tier;
- for PPE: `SupplierId`, `BasePrice`, and retail and wholesale percentages, each with its mode and either an `AdditionalReferenceId` or a manual value.

The admin list is returned as `Ardalis.Result.PagedResult<IReadOnlyList<ProductListItem>>`: items are in `value`, and metadata (`pageNumber`, `pageSize`, `totalPages`, `totalRecords`) is in `pagedInfo`. Each item contains `minimumWholesalePrice`: for Sewing, the minimum of all available calculated prices across fabrics and tiers; for PPE, the calculated wholesale price; and `0` when calculation is impossible. The list supports:

- one search across ID and Ukrainian and Russian names; `Slug` is not searched. A single numeric token searches for an exact ID, while text is split on spaces: each token is a partial, case-insensitive match and must match at least one localized name;
- filtering by `ProductType`;
- filtering by one category: products in the selected category and all its child categories;
- sorting by `id`, `name`, `createdAt`, or `updatedAt` in `asc`/`desc` order.

The page parameter starts at 1. `pageSize` defaults to 20 and accepts only 10, 20, or 50. Default sorting is `sortBy=name`, `sortDirection=asc`.

The admin list remains compact and sends only `categoryIds`, `minimumWholesalePrice`, and `mainPhoto` as `null` or an object with `mediaFileId` and a ready `url`. Full admin details (`GET /{id}` and successful `POST`/`PUT`) instead expand categories as objects with `id`, `name`, `ruName`, and `slug` in one response.

A photo is sent as an object with `mediaFileId` and a ready `url`. In full admin detail, each photo additionally includes `alt`, `isVisible`, `isMain`, and `sortOrder`; the list includes only the primary photo.

Create/update requests send only the IDs of selected Reference entities and manually entered Product values. Full admin detail additionally expands selected reference data needed for display: ID, name, and current price for a fabric/accessory; ID, name, and current duration in minutes for an operation; supplier ID and name; and, for the selected `AdditionalReference`, its ID, name, key, `value`, and `unit`.

Each admin list row contains only `id`, Ukrainian name, `slug`, `type`, `categoryIds`, `minimumWholesalePrice`, the primary photo with URL or `null`, `createdAtUtc`, and `updatedAtUtc`. Full admin detail is returned only for `GET /{id}` and successful `POST` and `PUT`; it is a ready projection for the form and includes expanded categories, all photos with URLs, selected Reference entities, and all applicable prices. The frontend does not make separate requests for each ID belonging to one Product.

The public storefront endpoint, its SEO, availability, variants, and localization behavior are outside the current delivery.

Temporarily, all admin endpoints have explicit `AllowAnonymous`, despite the Host fallback authentication policy. Before production, this access must be replaced by a separately agreed authorization policy; anonymous access is not a production decision.

`Idempotency-Key` is not part of the current create/update contract. The admin frontend must block repeated form submissions, and update addresses the existing `ProductId`.

The current delivery includes full Product CRUD. Base routes:

- `GET /api/v1/products` — paginated list;
- `POST /api/v1/products` — create;
- `GET /api/v1/products/{id}` — full admin detail;
- `PUT /api/v1/products/{id}` — update;
- `DELETE /api/v1/products/{id}` — full Product deletion.

`DELETE` removes Product together with its own detail records and links. `MediaFile` is not deleted and remains available in the Media module.

`PUT` accepts the complete state of all editable Product data: shared fields, photos, categories, content, and data for exactly one type (`Sewing` or `Ppe`). There are no partial endpoints for these parts. If the type changes, previous type-specific details and links are deleted, and the new type's data is saved from the submitted body. `Slug` is not sent: the backend generates it from the Ukrainian name using a transliteration package.

In requests and responses, `type` is a string enum with only `Sewing` or `Ppe`; the numeric representation of the .NET enum is not part of the HTTP contract. Create and full replacement through `PUT` accept this field, and list/detail responses always return it.

For `POST`, the administrator provides the ID in the request body. It must be positive and unique. A repeated ID returns `409 Conflict`; `PUT` takes the ID only from route `{id}` and does not accept it in the request body.

Successful creation returns `201 Created` and full admin detail for the created Product. Read and full update return `200 OK` and the current full admin detail; this gives the frontend all calculations without an additional `GET`. Deletion returns `204 No Content`. A missing Product returns `404 Not Found`, an invalid request returns `400 Bad Request`, and a repeated ID, Ukrainian/Russian name, or rejected deletion of a linked entity returns `409 Conflict`.

`ResultToActionResult` maps `Ardalis.Result.Invalid`, `NotFound`, and `Conflict` to `400`, `404`, and `409`. `ProductsController` explicitly returns `201 Created` for create and `204 No Content` for delete, without changing the agreed contract.

No new error envelope is introduced. The API uses the current `Ardalis.Result` format: its validation errors for `400`, its errors for `409`, and an empty response body for `404`.

For `POST` and `PUT`, the boundary validates string enums before calling Mediator. Only case-sensitive values `type: "Sewing" | "Ppe"` and `retailPercent.source` / `wholesalePercent.source: "Reference" | "Custom"` are allowed; numeric, string-numeric, case-variant, and unknown values are invalid. The error uses the same `ValidationError` array as Application validation, for example:

```json
[
  {
    "identifier": "type",
    "errorMessage": "type must be either Sewing or Ppe."
  }
]
```

or for a PPE percentage:

```json
[
  {
    "identifier": "retailPercent.source",
    "errorMessage": "retailPercent.source must be either Reference or Custom."
  }
]
```

## Required agreement before API code

- final authorization policy before production;

## Accepted corrective change

2026-08-19 — `03.8-catalog-read-model-enrichment` provided the Application
read model with primary/all photo URLs and calculated PPE prices. After that,
`04.2` creates API DTOs/mappers, and `04.3` synchronizes the OpenAPI YAML. Until
those subphases are complete, the contract must not be handed to the frontend as implemented.

## Handoff rule

Before frontend handoff, `product-catalog.openapi.yaml` must describe every available endpoint, parameters, request body, all response codes, errors, and examples. A public contract change requires a compatibility assessment and a frontend client update.
