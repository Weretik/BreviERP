# BreviERP API — Frontend Integration Guide

The versioned OpenAPI entry point is [openapi.json](openapi.json). The build generates it from the code, so it lists every public endpoint. Generate types and clients from this file; do not copy DTOs from backend source code.

## Authentication

`POST /api/auth/cookie/login` signs the browser in: it sets the HttpOnly session cookie `__Host-brevi.session` on `api.brevi.com.ua` and the readable CSRF cookie `brevi.csrf` with `Domain=brevi.com.ua`, and returns the current user. No token is returned. Send every request with credentials (`withCredentials: true` / `credentials: 'include'`).

Every `POST`, `PUT`, `PATCH` and `DELETE` sends header `X-CSRF-Token` with the current `brevi.csrf` value; without it the API answers `400 Invalid CSRF token.` `POST /api/auth/cookie/logout` ends only this browser's session and expires both cookies. `GET /api/auth/session/me` returns `userId`, `email`, `fullName` and `roles`; call it on start to learn whether the user is signed in (`401` means show the login page).

The host uses a fallback policy that requires authentication. Only endpoints explicitly marked `AllowAnonymous` in the controller are public. Admin signs in with the session cookie on `api.brevi.com.ua`, sends requests with credentials and adds `X-CSRF-Token` (the `brevi.csrf` cookie) to changing requests; see the session flow (`docs/sdd/architecture/security/authentication/session-flow.md`).

## HTTP Conventions

- JSON uses ASP.NET Core camelCase property names.
- Paginated responses use `{ pagedInfo, value }`. `pagedInfo` contains `pageNumber`, `pageSize`, `totalPages`, and `totalRecords`; `value` contains the row array.
- Validation errors return `400` in the current Ardalis validation error format. Do not rely on localized message text.
- `404` has no response body. `409` returns an array of conflict messages. `401` means authentication is missing or invalid; `403` means the authenticated user lacks the required access.
- Decimal values are sent as JSON numbers. IDs are integers unless the schema explicitly defines `uuid` or `string`.

## Current API Map

| Area                             | Route                                                                                      | Access                                                     | OpenAPI coverage                                                                                                                                                   |
| -------------------------------- | ------------------------------------------------------------------------------------------ | ---------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Products                         | `/api/v1/products`, `/api/v1/products/{id}`                                                | `access:<page>` («Права доступу»)                                        | [Catalog product contract](openapi.json)                                                                                                   |
| «Товари» page                    | `/api/v1/products/overview`, `/api/v1/products/{id}/overview`                              | Read `CatalogRead`; write `access:<page>` («Права доступу»)              | [Product overview contract](openapi.json)                                                                                                 |
| Product categories               | `/api/reference/product-categories/...`                                                    | `access:<page>` («Права доступу»); `store/{language}` is anonymous       | not yet added                                                                                                                                                      |
| Photo library «Фото»             | `/api/media/folders/...`, `/api/media/files/...`                                           | `access:media`                     | [Media library contract](openapi.json)                                                                                                         |
| Suppliers                        | `/api/reference/suppliers`                                                                 | `access:<page>` («Права доступу»)                                        | [Reference supplier contract](openapi.json)                                                                                                    |
| Pricing Reference data           | `/api/reference/fabrics`, `/fabric-colors`, `/garment-accessories`, `/garment-part-operations` | Reads `CatalogRead`; writes `access:<page>` («Права доступу»)            | [Fabrics](openapi.json), [colours](openapi.json), [accessories](openapi.json), [operations](openapi.json) |
| Other reference data             | `/api/reference/garment-parts`, `/pricing-parameters`, `/ppe-markup-rules`, `/product-sections` | Garment parts `access:<page>` («Права доступу»); other reads `CatalogRead`, writes `access:<page>` («Права доступу») | [Garment parts](openapi.json), [pricing parameters](openapi.json), [product sections](openapi.json) |
| Sewing calculations/publications | `/api/v1/products/{productId}/articles/{articleId}/pricing/...`                            | CatalogRead for reads; `access:<page>` («Права доступу») for writes        | [Sewing pricing](openapi.json)                                                                                                              |
| Sewing settings / service costs  | `/api/v1/pricing/sewing/settings...`, `/api/reference/pricing/other-costs/{code}/versions` | Settings read: CatalogRead; revisions: `access:<page>` («Права доступу»)   | [Pricing settings and costs](openapi.json)                                                                                                  |
| Session                          | `/api/auth/cookie/login`, `/api/auth/cookie/logout`, `/api/auth/session/me`                | Login anonymous; logout and me need the session cookie     | [Identity session contract](openapi.json)                                                                                                         |
| Staff accounts                   | `/api/admin/users...`                                                                      | `CanManageUsers` (Admin)                                   | [Staff accounts contract](openapi.json)                                                                                                             |

Before using an endpoint without a versioned OpenAPI contract, frontend and backend must agree on the request, response, status codes, and authorization, then add the contract to [aggregate OpenAPI](openapi.json).

The pricing contract is implemented in the backend; frontend consumption and the clean backend commit pin are TS-005 work. A saved calculation may contain blockers and no prices. Only explicit successful publication establishes a current Article price; existing Product price fields still use their legacy projection until consumer cutover.
