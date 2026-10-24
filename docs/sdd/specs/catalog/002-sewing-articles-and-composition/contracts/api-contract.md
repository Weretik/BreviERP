# Proposed HTTP contract

**Status:** implemented in the Catalog API, 2026-10-25. The canonical [sewing composition OpenAPI](../../../../contracts/catalog/sewing-composition.openapi.yaml) defines exact DTOs, operation IDs, access and errors. Existing Product operations remain available. See the [implementation checkpoint](../implementation.md) for rollout and verification limits.

| Method and proposed route | Proposed operationId | Request/response | Access and failure |
| --- | --- | --- | --- |
| `GET /api/v1/products/{productId}/articles` | `listSewingArticles` | Bounded Article list: internal ID, sale SKU, active, order, readiness | Authorized product read; 404 if Product absent/wrong type. |
| `POST /api/v1/products/{productId}/articles` | `createSewingArticle` | `{ saleSku, active, sortOrder }`; created Article with ID | Product-management write; 400 invalid, 404 parent, 409 duplicate SKU. |
| `GET /api/v1/products/{productId}/composition?articleId={id}` | `getSewingComposition` | Model-wide and effective Article lines, each with scope, kind/ref, quantity/unit, review and blockers | Authorized product read; 404 parent/article mismatch. |
| `PUT /api/v1/products/{productId}/composition` | `replaceSewingComposition` | Full model-wide line set and reviewed operation lines plus concurrency token; response new version/readiness | Product-management write; 400 invalid, 404 missing reference, 409 stale version or conflict. |
| `PUT /api/v1/products/{productId}/articles/{articleId}/composition` | `replaceSewingArticleComposition` | Full article-specific material lines plus concurrency token; response effective composition | Product-management write; 400/404/409 as above. |

Use exact DTO schemas, numeric bounds, field names, `ProblemDetails` shape and auth policy already established by Brevi's current contract style. `SaleSku` is a string and remains stable; Article ID is an internal typed ID. `PUT` replacement is idempotent for an identical payload and version. List responses are bounded/paginated if the catalogue permits large article sets. No price amount is returned by this contract until Pricing defines it.

**EN-002 decision:** separate protected controller; `CatalogRead` reads and `CanManageProducts` writes; required body `expectedVersion` backed by Product-level EF optimistic concurrency. Article creation also requires the token. Model code is part of shared composition replacement. An identical payload with a stale version returns 409; clients retain the draft and reload explicitly. This replaces the proposal's identical-payload/version retry wording. Frontend consumes a generated contract from a pinned backend commit.
