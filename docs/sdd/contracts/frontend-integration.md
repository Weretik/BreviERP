# BreviERP API — Frontend Integration Guide

The versioned OpenAPI entry point is [openapi.yaml](openapi.yaml). Generate types and clients from this file; do not copy DTOs from backend source code. The aggregate contract currently includes only APIs with a separately created and verified versioned contract. An endpoint missing from this file indicates a contract coverage gap, not that the endpoint is absent at runtime.

## Authentication

`POST /api/auth/session/login` returns an opaque Bearer access token and sets two cookies: the HttpOnly refresh cookie `kedr.rt`, sent only to the refresh route, and the browser-readable CSRF cookie `kedr.csrf`. These names are the current legacy BreviERP configuration; the frontend must use them as the runtime contract until a separate migration changes them. For a cross-origin login, the frontend must use `credentials: 'include'`, otherwise the browser will not store the response cookies.

For protected operations, send `Authorization: Bearer <accessToken>`. To refresh the session, call `POST /api/auth/session/refresh` with `credentials: 'include'` and send the current value of the `kedr.csrf` cookie in the `X-CSRF-Token` header. The refresh endpoint rotates both cookies and returns a new access token. `POST /api/auth/session/logout` also requires Bearer authentication, revokes all of the user's active refresh sessions, and deletes both cookies; for cross-origin logout, use `credentials: 'include'` so the browser applies the cookie deletion headers. `GET /api/auth/session/me` returns `userId`, `email`, and `roles`; the response does not include permissions.

The host uses a fallback policy that requires authentication. Only endpoints explicitly marked `AllowAnonymous` in the controller are public.

## HTTP Conventions

- JSON uses ASP.NET Core camelCase property names.
- Paginated responses use `{ pagedInfo, value }`. `pagedInfo` contains `pageNumber`, `pageSize`, `totalPages`, and `totalRecords`; `value` contains the row array.
- Validation errors return `400` in the current Ardalis validation error format. Do not rely on localized message text.
- `404` has no response body. `409` returns an array of conflict messages. `401` means authentication is missing or invalid; `403` means the authenticated user lacks the required access.
- Decimal values are sent as JSON numbers. IDs are integers unless the schema explicitly defines `uuid` or `string`.

## Current API Map

| Area | Route | Access | OpenAPI coverage |
| --- | --- | --- | --- |
| Products | `/api/v1/products`, `/api/v1/products/{id}` | Anonymous in the current controller | [Catalog product contract](catalog/product-catalog.openapi.yaml) |
| Product categories | `/api/reference/product-categories/...` | Anonymous in the current controller | not yet added |
| Catalog media | `/api/catalog/media/...` | Anonymous in the current controller | not yet added |
| Suppliers | `/api/reference/suppliers` | Anonymous in the current controller | [Reference supplier contract](reference/suppliers.openapi.yaml) |
| Pricing Reference data | `/api/reference/fabrics`, `/garment-accessories`, `/garment-part-operations` | Anonymous GET; writes require CanManageProducts | [Fabrics](reference/fabrics.openapi.yaml), [accessories](reference/garment-accessories.openapi.yaml), [operations](reference/garment-part-operations.openapi.yaml) |
| Other reference data | `/api/reference/garment-parts`, `/additional-references` | Anonymous in the current controllers | [Garment parts](reference/garment-parts.openapi.yaml), [additional references](reference/additional-references.openapi.yaml) |
| Sewing calculations/publications | `/api/v1/products/{productId}/articles/{articleId}/pricing/...` | CatalogRead for reads; CanManageProducts for writes | [Sewing pricing](catalog/sewing-pricing.openapi.yaml) |
| Sewing settings / service costs | `/api/v1/pricing/sewing/settings...`, `/api/reference/pricing/other-costs/{code}/versions` | Settings read: CatalogRead; revisions: CanManageProducts | [Pricing settings and costs](catalog/sewing-pricing.openapi.yaml) |
| Session | `/api/auth/session/login`, `/refresh`, `/logout`, `/me` | Login/refresh are anonymous; logout/profile require Bearer | [Identity session contract](identity/session.openapi.yaml) |

Before using an endpoint without a versioned OpenAPI contract, frontend and backend must agree on the request, response, status codes, and authorization, then add the contract to [aggregate OpenAPI](openapi.yaml).

The pricing contract is implemented in the backend; frontend consumption and the clean backend commit pin are TS-005 work. A saved calculation may contain blockers and no prices. Only explicit successful publication establishes a current Article price; existing Product price fields still use their legacy projection until consumer cutover.
