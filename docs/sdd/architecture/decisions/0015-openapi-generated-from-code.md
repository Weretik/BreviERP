# ADR-0015: The OpenAPI contract is generated from the code at build

- Status: Accepted
- Date: 2026-11-15 (owner's request)
- Owner: Виталий (project owner)

## Context

The public API contract was written by hand as 56 YAML files in `docs/sdd/contracts/`. The code already produced its own OpenAPI document (`Microsoft.AspNetCore.OpenApi`, plus Swashbuckle for Swagger UI), but nothing tied the two together: 128 reads had no typed answer in the code's document, 46 endpoints had no operation id, and about 30 unit tests compared DTOs with the YAML by hand. brevi-web copied the YAML at a pinned commit with `contracts:sync` and generated its types from it, so a drift reached the admin unnoticed.

## Alternatives considered

- Keep hand-written YAML and compare it with the generated document in a test: rejected; two sources still have to be edited for every change, and a structural comparison of two documents is fragile.
- Swashbuckle as the generator: rejected; the framework generator is already registered, supports .NET 10 and runs at build time, so a second generator only duplicates it.
- Generate the document in a test (`WebApplicationFactory`): rejected as the main path; the build-time generator writes the file with no extra step, and CI only compares it.

## Decision

- The build of `Host.Api` writes the document of the code to [`docs/sdd/contracts/openapi.json`](../../contracts/openapi.json) through `Microsoft.Extensions.ApiDescription.Server` (`OpenApiGenerateDocuments`). It is the only machine-readable API contract; the hand-written YAML files are removed.
- CI builds the solution and fails when the committed `openapi.json` differs from the generated one: commit the regenerated file with every endpoint change.
- Each endpoint describes itself in code: `WithName` (a unique camelCase operation id), `WithSummary` (and `WithDescription` for rules a reader needs), typed `Produces<T>` and `ProducesProblem` for errors. `tests/HostApiTests/Http/OpenApiDocumentRulesTests.cs` requires an operation id, a summary and a tag on every operation, a typed body on every read (files and redirects are listed), and forbids `integer|string` numbers.
- `StrictNumberSchemaTransformer` documents numbers as numbers. Swagger UI (Development) reads `/openapi/v1.json`; Swashbuckle's generator is removed.
- Unit tests that need the contract read `openapi.json` through `tests/UnitTests/TestSupport/ApiContract.cs` (an operation is published); comparisons of DTOs with the YAML are gone, because the schema now is the DTO.
- Decisions a person must read (consumers, compatibility, idempotency, errors) stay in the feature's `contracts/api-contract.md`.
- brevi-web downloads `openapi.json` from `master` of this repository, generates types with `openapi-typescript` and calls the API through `openapi-fetch`; its CI fails when its copy is behind (brevi-web ADR).

## Why

- The contract cannot drift from the code: it is the code's own description, checked by CI.
- One change of an endpoint is one edit; the frontend sees a breaking change as a type error.

## Trade-offs

- Every build starts the host once (a few seconds) without a database; the host removes background services for that run. The container image build skips it.
- Examples and long prose of the old YAML are gone; summaries and descriptions moved into the code.
- Schema names follow the C# types (`FabricRowDTO`), so brevi-web renamed its type references once.

## Sources and checks

- [Contracts](../../contracts/README.md), `src/Bootstrapper/Host.Api/Host.Api.csproj`, `src/Bootstrapper/Host.Api/DependencyInjection/Http/OpenApi/`, `tests/HostApiTests/Http/OpenApiDocumentRulesTests.cs`, the CI step «Check the OpenAPI contract is up to date».
- The same decision applies to KedrStore.
