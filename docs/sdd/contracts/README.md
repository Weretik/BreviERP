# API Contracts

[`openapi.json`](openapi.json) is the only machine-readable contract of the public HTTP API. It is **generated from the code**: every build of `Host.Api` writes it ([ADR-0015](../architecture/decisions/0015-openapi-generated-from-code.md)). Do not edit it by hand.

## How it works

1. An endpoint describes itself in code: `.WithName("getFabrics")` (unique camelCase operation id, the frontend sees it), `.WithSummary("List fabrics")`, `.WithDescription(...)` for rules a reader needs, typed `.Produces<TResponse>()` and `.ProducesProblem(status)` for every error it returns, and its request type through the handler parameters. A file answer declares its content type: `.Produces(StatusCodes.Status200OK, contentType: "application/pdf")`.
2. `dotnet build` (or any build or test of `Host.Api`) regenerates `openapi.json`. Commit it together with the endpoint change.
3. CI builds the solution and fails with «OpenAPI contract is out of date» when the committed file differs from the code.
4. [`OpenApiDocumentRulesTests`](../../../tests/HostApiTests/Http/OpenApiDocumentRulesTests.cs) requires an operation id, a summary and a tag on every operation, unique ids, a typed body on every read, and numbers documented as numbers. Unit tests check that an operation is published through [`ApiContract`](../../../tests/UnitTests/TestSupport/ApiContract.cs).
5. brevi-web downloads `openapi.json` from `master` (`npm run contracts:sync`), generates types with `openapi-typescript` and calls the API through `openapi-fetch`. Its CI fails when its copy is behind `master`, and a daily job opens a sync PR. A breaking change of a path, parameter or schema becomes a TypeScript error there.

The document describes the code; it does not make an endpoint public. The host endpoint conventions, authorization policies and the fallback policy decide runtime access.

## Changing the API

- Record decisions a person must read (boundaries, consumers, compatibility, security, errors, idempotency, deployment) in the feature's `contracts/api-contract.md`.
- Write the API or unit test first, then the endpoint; the regenerated `openapi.json` shows the contract change in review.
- A breaking change (removed or renamed path, parameter or field, changed type) needs a migration path for the admin and the site.
- In the PR say that brevi-web must sync the contract; its daily job also opens the sync PR on its own.

## Frontend handoff

- [Frontend integration guide](frontend-integration.md): authentication, CSRF, pagination, errors, and current routes.
- Swagger UI in Development: `/swagger`, reading `/openapi/v1.json`.

## Access

Every operation requires a signed-in Admin session cookie (`__Host-brevi.session`) unless the roles and policies (`docs/sdd/architecture/security/authorization/roles-and-policies.md`) list it as public. Changing requests also send `X-CSRF-Token`. The temporary anonymous mode (`ApiAccess:AnonymousEnabled`) is removed.
