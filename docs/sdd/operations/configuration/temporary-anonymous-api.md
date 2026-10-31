# Temporary public access to all BreviERP APIs

The user requested every current API to be reachable without a bearer token temporarily. Host.Api appsettings.json now sets ApiAccess:AnonymousEnabled=true, in every environment unless overridden. This includes reads, writes, deletes and previously private CRM files; it is not read-only access.

## Use and restore

Restart the API to load the changed startup configuration. Swagger UI is available at /swagger, Swagger JSON at /swagger/v1/swagger.json and built-in OpenAPI at /openapi/v1.json even outside Development while the mode is enabled. No Authorize step is required for API calls.

To restore original access rules, set environment variable ApiAccess__AnonymousEnabled=false (or change the matching configuration setting) and restart. Original controller attributes, named policies and fallback policy remain in place. Production documentation returns to its original Development-only availability when disabled.

## Actor attribution

Some operations require a real user ID to preserve actor/owner foreign keys and internal Admin checks. The middleware resolves the lowest-ID existing unlocked Admin from Identity, or the eligible Admin specified by ApiAccess:ActorUserId. It assigns that identity internally for actor-dependent requests, including requests carrying unusable or lower-privilege credentials. It creates no users, passwords, bearer tokens or cookies. All such actions are attributed to the shared Admin, not an authenticated visitor.

If no eligible Admin exists, actor-dependent endpoints return HTTP 503 with code anonymous_api_actor_unavailable. Already public endpoints and public documentation do not require this lookup. An empty database still needs the project's normal approved seeding; no seed/migration is run by enabling this mode.

Login and refresh endpoints remain callable anonymously but still validate their credentials/refresh cookie/CSRF as required by their business operation. Domain validation, concurrency/idempotency, file checks, provider readiness, CORS and rate limits stay in force. Public access does not guarantee a successful business result for invalid input or unconfigured Gmail.

Normal module OpenAPI contracts describe access with this mode disabled. Temporary deployment behavior overrides authentication requirements only; routes and DTOs are unchanged. Generated Swagger removes its global bearer requirement while the mode is enabled.

## Verification

Run dotnet test tests/HostApiTests/HostApiTests.csproj --filter FullyQualifiedName~AnonymousApiAccessTests. This uses an in-process server and fixed actor fixture; it never calls Google, dispatches mail or writes to the application database. See the [specification](../../specs/platform/002-temporary-anonymous-api/README.md) for evidence and the existing IntegrationTests NUnit 5 compilation limitation.
