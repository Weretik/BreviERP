# Temporary anonymous API access

**Superseded 2026-11-03:** the mode, its code and configuration are removed by [identity 001 cookie sign-in](../../identity/001-cookie-sign-in/README.md). Kept as history.

Date: 2026-10-31. Owner: Host.Api composition, no new module or schema. Status: implemented and locally verified. Production deployment is not performed. User explicitly requested all existing APIs public temporarily, including every HTTP method. No deployment/commit/push is requested by this change.

## Rules and observable scenarios

- R-01: All controller endpoints bypass authentication and role policies when ApiAccess:AnonymousEnabled=true. The repository runtime configuration enables this temporary mode. Set false and restart to restore original authorization metadata/policies. No per-controller attribute removal.
- R-02: Endpoints requiring an actor receive a shared existing, unlocked Admin identity so CRM ownership/audit and Catalog actor checks remain functional. A configured positive ActorUserId selects an eligible Admin; otherwise choose the lowest eligible Admin ID. No user creation, password bypass inside login, fake database IDs, bearer-token issuance or session cookies for this mode. These anonymous actions are attributed to that shared Admin, not an identified human.
- R-03: Existing validation, domain invariants, files checks, idempotency and external-provider readiness remain enforced. Login/refresh still validate their request credentials/CSRF/refresh tokens: anonymous HTTP access does not fabricate successful login. If no eligible actor exists, actor-dependent operations return 503 anonymous_api_actor_unavailable; already public/no-actor operations remain reachable.
- R-04: Swagger UI, generated Swagger/OpenAPI documents are public while enabled, including Production. Generated Swagger has no global bearer requirement in this mode. Original normal-mode versioned module contracts stay intact; this is an explicit temporary deployment access overlay, not a DTO/route change.
- SC-01: Given no Authorization header (or an unusable bearer), when accessing a protected controller action in enabled mode, then authorization and role checks permit entry and actor-dependent handlers see the shared Admin. Test: HTTP integration, GET/POST/PUT/DELETE and named/role/fallback policies.
- SC-02: Given the setting disabled, when accessing a protected endpoint anonymously, then original 401/403 behavior and Swagger bearer requirements remain. Test: HTTP integration.
- SC-03: Given enabled mode, when enumerating actual module controller endpoints, then every endpoint has AllowAnonymous metadata. Test: endpoint inventory over real module assemblies.
- SC-04: Given no eligible Admin, when accessing an actor-dependent endpoint, then 503 with stable code; when accessing an already public action, then no actor lookup is necessary. Test: HTTP integration with actor port fake.
- SC-05: Given enabled mode in Production, when retrieving Swagger UI/JSON and built-in OpenAPI, then documentation is publicly reachable and does not claim bearer is required. Test: HTTP integration.

## Design and responsibilities

Host.Api/Security/AnonymousApiAccess/: options (startup switch, optional actor ID), actor record/port, Identity-backed scoped resolver, request middleware, registration/pipeline/controller-mapping extensions. Host composition can access Identity persistence as existing QuoteAssigneeReader does; module boundaries remain unchanged. Middleware runs after authentication and before authorization, only for /api controller actions with authorization metadata; original principal is replaced by the shared Admin while enabled. AllowAnonymous endpoint convention also bypasses fallback authorization on all controllers. All current controllers use /api routes. No changes to domain/application actor contracts.

Program.cs wires the mode, documents and controller convention. Swagger generation uses the same bound options to remove its global bearer requirement. appsettings.json enables the mode; environment ApiAccess__AnonymousEnabled=false overrides it. No schema/seed changes or changed client-input DTOs/validators.

## Task / traceability / evidence

TS-01 covers SC-01–05 and R-01–04: implement Host.Api composition and scoped selector; update runtime configuration, operations/feature map and this evidence. Depends on: none. Test level: focused HTTP integration without live DB/network, plus existing CRM authorization regression. Tests: tests/HostApiTests/AnonymousApiAccessTests.cs. Red: no-op endpoint/pipeline extension preserves baseline authorization so enabled scenarios fail for behavior, not missing compilation. Green: full implementation. Review feature responsibilities and restore-mode behavior; record exact counts before verified.

No API consumer contract sync is needed: routes, payloads and generated DTOs stay unchanged. Runtime anonymous access and temporary 503 are documented here and in operations; normal 401/403 contracts apply again when disabled. No claim of deployment follows local tests. Existing live customer data and write actions become reachable whenever this mode is deployed and enabled, per the user's requested scope.

EN-01: isolated HostApiTests project, using existing test SDK/NUnit/TestHost versions. The original IntegrationTests project cannot compile because pre-existing ThrowsAsync assertions are incompatible with NUnit 5 (CS1061/NUnit2059). This is a harness prerequisite, not a behavioral Red result. HostApiTests compiles actual Host.Api/module code and performs HTTP tests without a DB; no old test failures are changed or claimed fixed.

## Verification evidence — 2026-10-31

- Baseline IntegrationTests command was blocked by pre-existing NUnit 5 ThrowsAsync source/analyzer errors (CS1061/NUnit2059). This was not counted as Red and unrelated test files were not changed.
- After EN-01 isolated harness: dotnet test tests/HostApiTests/HostApiTests.csproj --filter FullyQualifiedName~AnonymousApiAccessTests: 11 tests compiled/executed, eight expected failures and three passes with the no-op access overlay (protected requests returned 401). This is the behavioral Red checkpoint.
- Green/final command: dotnet test tests/HostApiTests/HostApiTests.csproj --filter FullyQualifiedName~AnonymousApiAccessTests --no-restore --verbosity quiet: 16 passed, zero failed/skipped. The test project also compiles real Host.Api/module dependencies. OpenAPI's generated interceptor namespace was explicitly enabled in the new test project as required by the .NET 10 generator.
- Checks cover GET/POST/PUT/DELETE actor-dependent access without a token, unusable bearer, named policy, authenticated lower-role access, restored normal 401/403 behavior, fallback policy, absent actor 503, public endpoint with no actor lookup, inventory of every real module controller endpoint, global Swagger security on/off, and Swagger UI/Swagger JSON/native OpenAPI availability in Production.
- Read-only SELECT against the existing approved localhost/brevidb found one eligible unlocked Admin. No credentials, personal fields or full connection strings were printed by that check. No database rows/schema were changed. The HTTP tests use a deterministic actor fixture; no application write/email was executed as an operational check.
- Self-review: host-only access override, original module authorization metadata retained, positive configured actor ID validation, eligible real Admin selector, no client-supplied actor/role, no token/cookie issuance, unchanged domain validation/idempotency/foreign keys/provider readiness. One scoped Identity lookup for actor-dependent actions; no actor lookup for already public endpoints or documentation. No module layer dependency changed.
- Spec, feature map, operations and contract navigation describe the temporary mode and rollback. Diff whitespace and changed Markdown links are checked separately before handoff. No commit, push, production restart/deployment, migration, seed or live provider operation is part of this task.

SC-01–05 → TS-01/EN-01 → tests/HostApiTests/AnonymousApiAccessTests.cs → verified locally by the evidence above. Existing IntegrationTests compilation failures remain outside this scope; do not claim that suite passed.
