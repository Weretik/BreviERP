# Proposed HTTP contract

**Status:** design proposal. Update [machine-readable OpenAPI](../../../../contracts/catalog/product-catalog.openapi.yaml) and operation IDs before implementation. Existing product operations remain compatible; this file does not claim the routes exist.

| Method and proposed route | Proposed operationId | Request/response | Access and failure |
| --- | --- | --- | --- |
| `GET /api/v1/products/{productId}/articles` | `listSewingArticles` | Bounded Article list: internal ID, sale SKU, active, order, readiness | Authorized product read; 404 if Product absent/wrong type. |
| `POST /api/v1/products/{productId}/articles` | `createSewingArticle` | `{ saleSku, active, sortOrder }`; created Article with ID | Product-management write; 400 invalid, 404 parent, 409 duplicate SKU. |
| `GET /api/v1/products/{productId}/composition?articleId={id}` | `getSewingComposition` | Model-wide and effective Article lines, each with scope, kind/ref, quantity/unit, review and blockers | Authorized product read; 404 parent/article mismatch. |
| `PUT /api/v1/products/{productId}/composition` | `replaceSewingComposition` | Full model-wide line set and reviewed operation lines plus concurrency token; response new version/readiness | Product-management write; 400 invalid, 404 missing reference, 409 stale version or conflict. |
| `PUT /api/v1/products/{productId}/articles/{articleId}/composition` | `replaceSewingArticleComposition` | Full article-specific material lines plus concurrency token; response effective composition | Product-management write; 400/404/409 as above. |

Use exact DTO schemas, numeric bounds, field names, `ProblemDetails` shape and auth policy already established by Brevi's current contract style. `SaleSku` is a string and remains stable; Article ID is an internal typed ID. `PUT` replacement is idempotent for an identical payload and version. List responses are bounded/paginated if the catalogue permits large article sets. No price amount is returned by this contract until Pricing defines it.

**EN-002:** settle route split, optimistic concurrency and authorization with the current product API owner; then write the OpenAPI operation schema. Frontend must consume the generated/approved contract version, not hand-maintain an assumed DTO.
