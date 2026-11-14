# Module rules

**Applies to:** every module in `src/Modules`, the hosts in `src/Bootstrapper`, the jobs in `src/Jobs`, and the test projects in `tests`.
**Checked by:** architecture tests in `tests/ArchitectureTests`, migration review, and code audit.

When to create a business module, how it is built, how modules talk to each other, how folders are organized inside it, where its data lives, and how its input is validated. The sections from «Module shape» to «Validation» are shared word for word with KedrStore. General layer rules are in the [backend rules](backend-rules.md); the reason for the Contracts layer is in [ADR-0006](../architecture/decisions/0006-module-contracts.md).

## Owning module first

Every feature belongs to one business module. Before an SDD design, name the owning module from the [module map](../architecture/overview/modules.md) and the data it owns.

- If the feature changes data or rules that an existing module owns, it goes into that module, even when it is large.
- If the feature introduces a new business capability whose data no existing module owns and whose rules do not depend on another module's internals (for example articles and static site pages next to a catalog), propose a new module in the SDD instead of growing an unrelated one. Example: content pages belong in a `Content` module, not in `Catalog`.
- A new module is an architecture decision: record it in the SDD design with the owned data, the public contract other modules may use, and the alternatives considered (extend an existing module or create a new one). The user accepts it before implementation.
- Do not split a capability into a separate module only because it has many files; size is a reason to split folders inside the module.

## Solution root

```text
<Solution>.sln
├── Directory.Build.props      settings shared by every project: TargetFramework, Nullable, ImplicitUsings, analyzers
├── Directory.Packages.props   central package management: every NuGet version, ManagePackageVersionsCentrally=true
├── global.json                SDK version
├── src/                       Bootstrapper/, BuildingBlocks/, <Module>/
├── tests/                     UnitTests/, IntegrationTests/, ArchitectureTests/
├── tools/                     scripts the team runs by hand
└── docs/
```

- A `.csproj` holds only what is specific to that project: its SDK, references, and `<PackageReference Include="..." />` without `Version`. Settings that every project shares go into `Directory.Build.props`; package versions go only into `Directory.Packages.props`, so one package has one version in the whole solution.
- Dockerfiles that restore before copying the sources copy `Directory.Build.props`, `Directory.Packages.props`, and `global.json` together with the project files.
- No other files at the root besides the solution, these props files, repository configuration (`.editorconfig`, `.gitignore`, `.gitattributes`, `.config/`), `README.md`, `AGENTS.md`, `CLAUDE.md`, and the license.

**BreviERP:** `Directory.Build.props` sets `net10.0`, `Nullable` and `ImplicitUsings` for all 72 projects; `Directory.Packages.props` holds the 54 package versions (`SkiaSharp` was on two versions, now one, 4.154.0, the one the API already ran); `global.json` pins SDK 10.0.1xx. `tests/ArchitectureTests/SolutionRootTests.cs` keeps it so.

## Module shape

Every business module has the same five projects. The host only registers modules; it holds no business code.

| Project | Owns | References |
| --- | --- | --- |
| `<Module>.Domain` | Aggregates, entities, value objects, domain errors and events, invariants | `BuildingBlocks.Domain` only |
| `<Module>.Application` | Commands, queries, handlers, FluentValidation validators, ports for persistence and integrations | `<Module>.Domain`, `<Module>.Contracts`, `BuildingBlocks.Application`, other modules' `Contracts` only |
| `<Module>.Infrastructure` | `DbContext`, entity configurations, migrations, repositories, external clients, implementations of the module's own `Contracts` | `<Module>.Application`, `<Module>.Domain`, `BuildingBlocks.Infrastructure`, other modules' `Contracts` only |
| `<Module>.Api` | Thin Minimal API endpoints that map the HTTP request to one Mediator command or query and the result to HTTP; the `Add<Module>Module()` registration and `Map<Module>Endpoints()` | `<Module>.Application`, `<Module>.Infrastructure` (registration only), `BuildingBlocks.Api`, other modules' `Contracts` only (for example access policy names) |
| `<Module>.Contracts` | What other modules may use: read and command interfaces, their DTOs, integration events, shared constants | no project of this or another module; only base packages such as `Ardalis.Result` |
| Host | `Program.cs`, configuration, middleware, the list of modules | each module's `Api` (and `Infrastructure` for job or seed hosts) |

- Scaffold projects with the .NET CLI (`dotnet new classlib`, `dotnet sln add`, `dotnet add reference`) and match the existing modules' folder layout and project settings instead of writing project files by hand.
- Create `<Module>.Contracts` together with the module, even while it is empty: it is the only place another module may look at.
- Register the module in the host with one call, `Add<Module>Module(configuration)`, defined in `<Module>.Api/DependencyInjection/`. It registers the module's Application, Infrastructure, and Contracts implementations; `Map<Module>Endpoints()` maps its endpoint groups. Endpoints and request models never use Infrastructure types.
- **Ports and DI, one chain.** Application declares what it needs as interfaces in `Application/Abstractions/` (`IOrderRepository`, `IUnitOfWork`, `IOneCClient`; for time use .NET `TimeProvider`). Infrastructure implements them (`Persistence/` for the `DbContext` and repositories, `Integrations/<System>/` for a client and its options) and binds them in one public entry point, `Add<Module>Infrastructure(configuration)`, in `Infrastructure/DependencyInjection/`. `Add<Module>Module(configuration)` in the Api calls it plus the Application registration (validators). The host's `Program.cs` calls only `Add<Module>Module(...)` for each module. Smaller public `Add*` helpers exist only when a job host or a test needs a subset, and say so in their summary.
- **Lifetimes.** A Singleton (and any `BackgroundService` or `IHostedService`, which are Singletons) never takes a Scoped service such as a `DbContext`, a repository, or the current user in its constructor; it takes `IServiceScopeFactory` and creates a scope per operation (`using var scope = scopeFactory.CreateScope();`). Every host turns on `ValidateScopes` and `ValidateOnBuild` in every environment (`builder.Host.UseDefaultServiceProvider(...)` or `ConfigureContainer(new DefaultServiceProviderFactory(...))`), and a test builds each host's container in Production mode, so a captive dependency or a missing registration fails at startup or in CI, not in production.
- Add the module to the architecture tests so its project references, its isolation, and its folder rules are checked.

## Transactional outbox

A use case that writes data and also has an external side effect (1C or another ERP, Telegram or e-mail, site revalidation, payment or delivery API, CDN upload) or must tell another module about the change does not call the outside world inside or around its transaction. It writes an outbox row in the same `SaveChanges`; after the commit the request tries the row once, and a dispatcher retries what is left.

- The outbox table and the dispatcher are shared code in `BuildingBlocks.Infrastructure`; each module owns its rows (in its own schema) and its handlers. Handlers are idempotent: the receiver gets a stable key and ignores a repeat.
- On a scale-to-zero host such as Cloud Run there is no in-process loop, hosted timer, or Hangfire. One protected internal endpoint per backend (`POST /internal/outbox/dispatch`, OIDC token of the scheduler's service account) is called every minute by one Cloud Scheduler job; it returns at once when nothing is due. No message bus or MassTransit.
- A failed row stays visible (admin page or log alert) with its last error after the retry limit; nothing is dropped silently.
- Not for work inside one module's transaction, for read-only calls, or when the user must see the external result synchronously.
- The SDD design names the message types, payload, handler, idempotency key, and retry limit. Details: the `backend-feature-spec` skill reference `references/transactional-outbox.md`.

**BreviERP today:** no outbox yet. These use cases call the outside world directly inside or right after their save and move to the outbox in the Outbox stage: Telegram messages (`SendProductionOrderTelegram`, reminder dispatch, site request notifications), КП e-mail through Gmail (`SendQuote`), CDN uploads and site page revalidation after content changes, and the Google Sheets and KeepinCRM syncs that write back. The two-save handlers in «Known deviations» are the first to move.

## Communication between modules

A module never reads another module's database, entities, `DbContext`, repositories, handlers, or Domain types. It uses only the other module's `Contracts` project.

- **Provider.** The module that owns the data publishes what others need in `<Module>.Contracts`: a small interface such as `IProductPriceReader`, the DTOs it returns (plain records, not Domain types), and integration events. The provider implements the interface in its Infrastructure (or Application) and registers it in `Add<Module>Module()`.
- **Consumer.** The module that needs the data references the provider's `Contracts` project from its Application (or Infrastructure, for an adapter) and injects the interface. Its unit tests replace the interface with a fake.
- **Reads** go through a contract interface. **"After X happens"** reactions go through an integration event declared in the provider's `Contracts` and handled in the consumer's Application, so the provider does not know the consumer.
- **Commands** to another module, such as "create a production order", are also a contract interface or an event; never call another module's handler or Mediator request type directly.
- Keep each contract small and named in the provider's words. Change it compatibly: add members or new interfaces instead of changing a shape a consumer already uses.
- The host does not implement cross-module adapters. An adapter that maps a provider's contract into the consumer's own port lives in the consumer's Infrastructure under `Adapters/<Provider>/`.

## Folder structure inside a module

Folders follow the business, not the kind of class. The same applies to the whole repository: no dump folders such as `Common/`, `Misc/`, `Helpers/`, `Utils/`, `Temp/`, or `Old/`, and no stray scripts or files at a project root. Do not create layer-wide or per-aggregate type folders such as `Entities/`, `Enums/`, `Errors/`, `ValueObjects/`, `Interfaces/`, `DTOs/`, `Models/`, `Services/`, or `Configurations/` that collect unrelated features. Namespaces follow folders.

```text
<Module>.Domain/
  <Aggregate>/                     Order.cs, OrderLine.cs, OrderStatus.cs, OrderId.cs, OrderErrors.cs, OrderPlaced.cs
  <Aggregate>/<Part>/              only when the aggregate grows: a sub-concept such as Lines/, not a type
<Module>.Application/
  Abstractions/                    ports: what Application asks from the outside world (IOrderRepository, IUnitOfWork, IClock, ICatalogDbContext)
  Features/<Feature>/
    Commands/<UseCase>/            PlaceOrderCommand.cs, PlaceOrderHandler.cs, PlaceOrderValidator.cs, PlaceOrderResponse.cs
    Queries/<UseCase>/             GetOrderDetailsQuery.cs, GetOrderDetailsHandler.cs, GetOrderDetailsValidator.cs, OrderDetailsDto.cs
    EventHandlers/                 handlers of domain or other modules' integration events for this feature
    Shared/                        DTOs, mapping, and policies used by several use cases of this feature
<Module>.Infrastructure/
  Persistence/                     <Module>DbContext.cs, design-time factory, Migrations/
  Persistence/<Aggregate>/         entity configurations, repository, read queries of that aggregate
  Integrations/<System>/           external clients such as 1C, Telegram, storage
  Adapters/<Provider>/             adapters from another module's Contracts to this module's own ports
  Contracts/<Feature>/             implementations of the module's own Contracts interfaces
  DependencyInjection/             registration of the layer
<Module>.Api/
  Endpoints/<Feature>/             <Feature>Endpoints.cs (one MapGroup per audience), one <UseCase>Endpoint.cs per use case, its <UseCase>Request
  DependencyInjection/             Add<Module>Module()
<Module>.Contracts/
  <Feature>/                       interfaces, DTOs, integration events, constants other modules use
```

- **Domain.** One folder per aggregate holds its root, child entities, value objects, enums, errors, and events. A value object used by several aggregates gets a folder named after the concept (`Money/`, `Slugs/`).
- **Application, one use case = one folder.** Every use case is one folder under `Commands/` (changes state) or `Queries/` (reads only), named after the action (`PlaceOrder`, `GetOrderDetails`). The folder holds, flat and together, everything of that use case: the request (`<UseCase>Command` or `<UseCase>Query`), its handler (`<UseCase>Handler`), its validator (`<UseCase>Validator`), and its result (`<UseCase>Response` for a command, `<Name>Dto` for a query). One request per folder: a second query or command is a second folder. No subfolders inside a use case (`DTOs/`, `Validators/`, `Models/`); a type that two use cases share goes to the feature's `Shared/`. Ports that Application needs from the outside world (repositories, unit of work, clock, read `DbContext` interfaces, external services) live in `Abstractions/` and are implemented in Infrastructure. A command handler never returns a read model bigger than the new identifier or status; a query handler never changes state. Admin, public, and mobile variants of the same feature live in the same feature folder as separate use cases (`GetRedirects`, `GetPublicRedirects`), not in separate `Public…` feature folders. A use case's request and response types live in its folder; the name `Contracts` is reserved for the module's Contracts project.
- **Infrastructure.** Persistence is grouped by aggregate, the same names as in Domain; integrations by external system.
- **Api (Minimal API).** Endpoints are grouped by the same features as Application, never in one flat `Endpoints/` folder. `Endpoints/<Feature>/<Feature>Endpoints.cs` maps the feature's groups (admin, public, mobile) with their route prefix, authorization policy, and tags; each use case has its own `<UseCase>Endpoint.cs` with its HTTP `Request` type next to it. The endpoint maps the `Request` to the Application command or query; the HTTP request is never bound straight to a command or query, so the API contract and the use case can change separately. Moving from controllers keeps the OpenAPI contract identical (compare the snapshot before and after).
- A folder with eight or more production files is a signal to look for a sub-concept to split out, not a reason to create type folders.

## Layer responsibilities and the handler

| Where | Does | Example |
| --- | --- | --- |
| Handler (Application) | the order of steps | load, call the domain, save |
| Aggregate (Domain) | the rules | no zero quantity, minimum order sum 500, a shipped order cannot be cancelled |
| Infrastructure | how exactly data is stored and fetched | EF Core, the 1C HTTP client |
| Api | HTTP | route, request model, status codes |

A command handler follows five steps:

1. Load the data it needs (aggregates through the module's ports, read models through `Contracts` of other modules).
2. Check what needs outside data: the referenced product exists, a slug is free, a configured root category is excluded. Return `NotFound` / `Invalid` / `Conflict` here.
3. Call the aggregate's method; the business rules are checked there and raise a domain error. The handler does not repeat or replace them (no `if (order.Status == Shipped) return Conflict` next to `order.Cancel()`); the pipeline maps a domain error to the response.
4. Save everything with one `SaveChanges`, that is one transaction. A batch command that processes items independently (an import, a migration of old files) may save once per item and says so in its summary.
5. Return a simple result: the new id, a status, or a DTO from the use-case folder, never a domain entity or value object.

- **Rich domain.** Entities have private setters and change state only through methods that check their invariants; a rule that involves only one aggregate's own data lives in that aggregate, not in the handler. A rule that needs several aggregates of the same module goes to a domain service in `Domain/<Aggregate>/`, called by the handler.
- **Queries** read through projections straight to DTOs (`Select`), never load aggregates to map them, and never call `SaveChanges`.
- **Api** endpoints map the HTTP request to one Mediator command or query and the result to HTTP through the one error mapper; they hold no rule, no query, no `try`/`catch`, and no domain type.
- The architecture tests check that request result types and Api code use no Domain types and that Domain entities have no public setters.

## Test structure

Tests mirror the code they check, so a test is found the same way as its code.

```text
tests/UnitTests/<Module>/Domain/<Aggregate>/                <Aggregate>Tests.cs
tests/UnitTests/<Module>/Application/<Feature>/             <UseCase>HandlerTests.cs, <UseCase>ValidatorTests.cs
tests/IntegrationTests/<Module>/Api/<Feature>/              HTTP tests of the feature's endpoints
tests/IntegrationTests/<Module>/Persistence/<Aggregate>/    database tests of that aggregate
tests/IntegrationTests/<Module>/Integrations/<System>/      external system adapters
tests/IntegrationTests/<Module>/Contracts/<Feature>/        the module's Contracts implementations
tests/IntegrationTests/Platform/                            host, pipeline, jobs
tests/ArchitectureTests/<Rule>/                             one folder per rule group: Layers, Modules, Folders, Hosts
tests/<Kind>/TestSupport/                                   factories, fakes, builders shared by that test project
```

- A test file sits in exactly one module and one layer folder; a namespace follows its path. No loose files at a test project root and no folders such as `Misc/`, `Helpers/`, `Common/`, or `Temp/`.
- CI runs only the affected tests on a pull request. A change in `src/<Module>/` or `tests/*/<Module>/` runs the unit and integration tests of that module and of every module whose projects reference it (read from the `.csproj` references, not a hand-kept list). Shared building blocks, hosts, build files (`Directory.*.props`, the solution), test support, and CI files run everything; documentation runs no module tests. Architecture tests and `Platform` integration tests always run, and a push to the main branch runs everything as the safety net before deploy. The selection script and its own tests live in `tools/ci/`, and the selection relies on the `tests/<Project>/<Module>/` folder and namespace rule above.
- Every module has at least: unit tests of its Domain aggregates, unit tests of its command and query handlers and validators, integration tests of its endpoints and persistence, and its place in the architecture tests.

## Architecture tests

The rules above are only real when a test fails on a violation. The architecture test project checks, for every business module:

- project references: Domain only `BuildingBlocks.Domain`; Application, Infrastructure, and Api reference another module only through its `Contracts`; `Contracts` references no module project;
- code dependencies (namespaces), the same rules, so a transitive reference cannot bypass them;
- Api endpoints do not use Infrastructure types;
- folders: no type folders (`Entities`, `Enums`, `Errors`, `ValueObjects`, `Configurations`, `Interfaces`, `DTOs`, `Models`, `Common`, `Helpers`, `Utils`), command and query handlers sit under `Features.<Feature>.Commands.<UseCase>` / `.Queries.<UseCase>` with no deeper folder, each use-case namespace holds one request and one handler, ports sit in `Abstractions`;
- hosts contain no class that implements a module port;
- each module's EF model maps only its own entities, with no navigation or foreign key to another module's entity or schema;
- test sources sit in a responsibility folder and their namespace follows the path.

A new module, layer, or rule is added to these tests in the same change, test first: the new architecture test must fail on the old code (Red) before the code is moved (Green), as the project's testing rules require for any behavior.

## Hosts and jobs

A host is the composition root, nothing more.

```text
Host.Api/
  Program.cs                       pipeline and startup
  appsettings*.json, Dockerfile
  DependencyInjection/             one file per technical concern: modules list, security, web, observability, options
```

- The host calls `Add<Module>Module()` for each module and configures cross-cutting concerns (authentication, CORS, logging, health checks, OpenAPI). It does not contain readers, adapters, notifiers, or any class that knows two modules' data; such a class belongs to a module's Infrastructure behind a `Contracts` interface.
- Access policies are declared once in the host from the names in `Identity.Contracts` (or the project's access module).
- A job or seed host (`Host.Jobs`, `Host.Seed`) only parses the command line, picks the job, and runs it. The job's logic is a use case of the module that owns the data (a command in its Application, run through Mediator or a published job interface in its `Contracts`), or a separate job project for a large one-off import.
- A job project has its own folders by step (`Source/`, `Planning/`, `Execution/`), references the owning module's `Contracts` (or, for a data import that owns no tables, that module's Infrastructure as a documented exception), and never maps another module's tables in its own `DbContext`.

## Database schema per module

- Each module owns its tables in its own PostgreSQL schema, named after the module in lower case (`content`, `crm`). Its `DbContext` calls `modelBuilder.HasDefaultSchema("<module>")`.
- Keep the module's migration history in its schema: `UseNpgsql(connection, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "<module>"))`, in both runtime registration and the design-time factory.
- One module's `DbContext` maps only its own tables. Do not add foreign keys, joins, or EF navigation to another module's tables; store the other module's identifier as a value and read its data through that module's `Contracts`.
- **No entity of another module inside an entity.** An entity never holds a navigation property, a collection, or a field typed with another module's entity (for example, `OrderLine.Product` from the Catalog domain). It keeps the other module's id as a plain value (`ProductId`, no foreign key) and a snapshot of what the owning aggregate needs at that moment (`ProductName`, `UnitPrice`), so an order still reads correctly after the product changes or disappears.
- The first migration of a new module creates its schema; check it against an empty database as the [database rules](database-rules.md) require.

## Extensibility without over-engineering

Add new behavior by adding a class, not by editing a growing chain of conditions, but only when the need is real.

| What is added | Tool | Example |
| --- | --- | --- |
| Another variant of the same action | Strategy: one port in `Application/Abstractions/`, one implementation per variant in Infrastructure, chosen through keyed DI or a registry, never a growing `switch` | Delivery providers, payment methods |
| The same behavior around every use case | A Mediator pipeline behavior in `BuildingBlocks.Application/Behaviors/` | Logging, validation, performance, transactions |
| Another reaction to something that happened | A new domain or integration event handler; across modules or to an outside system through the outbox | «Order placed» → notification, export to the accounting system |
| Switching a capability on or off without a deploy | A feature flag, only by a recorded product decision, with an owner and a removal date | Gradual rollout of a new checkout |

- **Rule of three.** Introduce the abstraction at the third variant, reaction, or copy, not before. Two cases stay a plain `if` or a short `switch`; mapping an enum to a name or a status is not a growing chain and stays a `switch` expression.
- No speculative interfaces, factories, base classes, generic repositories, or plug-in points "for the future". An interface exists only for a port (an outside dependency that Application must not know) or for a variant that already has at least two implementations.
- A pipeline behavior holds only cross-cutting plumbing; business rules stay in Domain and handlers.
- An audit reports both directions: a `switch` or `if` chain on a type or code that has reached three branches in more than one place, and an abstraction with a single implementation and no port role.

## Errors

Business outcomes travel as results; exceptions are only for the unexpected.

- **One error type.** The shared kernel defines `Error(Code, Message, Type)` with `ErrorType` = `Validation`, `NotFound`, `Conflict`, `Forbidden` (and `Unexpected`). Each module's Domain keeps its errors in static `<Aggregate>Errors` classes with stable codes such as `Orders.AlreadyShipped`. Domain and Application never mention HTTP: no status codes, `IResult`/`IActionResult`, `HttpContext`, or `BadHttpRequestException`.
- **Results, not exceptions, for business rules.** Every business rule in Domain, from "name is required" and "too long" to "only a draft can be published", reports a typed error through a result (`order.Cancel()` returns `Result`, a factory returns `Result<Order>`); the handler returns that error. Throw only for programming errors (`ArgumentNullException` guards, an impossible state) and real failures (database down, missing configuration). Do not throw a business exception and catch it in a controller or pipeline behavior to turn it into a status code.
- **One mapper.** Exactly one place in the shared Api layer maps `ErrorType` to HTTP: `Validation` 400, `NotFound` 404, `Conflict` 409, `Forbidden` 403, anything else 500. Endpoints call it; they never pick status codes from a result themselves and never wrap handlers in `try`/`catch`. The error body is `ProblemDetails` (validation errors under `errors` by field).
- **One global exception handler.** The host registers `AddProblemDetails()`, `AddExceptionHandler<GlobalExceptionHandler>()`, and `UseExceptionHandler()`. The handler logs the exception and answers 500 as `ProblemDetails` with the `traceId`; the client never gets the exception text or a stack trace (the developer page is for local development only). A job host logs a failed run and exits non-zero.
- **Language.** Messages a user can see (validation and business errors) are written in the users' language; codes and logs stay in English.

## Validation

- Server validation is mandatory. Frontend validation is only for user experience and is never a reason to skip a server check.
- Every command and every query that accepts client input has a FluentValidation validator (`<UseCase>Validator`) in the use-case folder. `ValidationBehavior` runs it before the handler and turns failures into a `Validation` error with the field name.
- Validators check the shape of the input: required fields, lengths, ranges, formats, and paging limits. Business invariants and state checks stay in Domain or the handler; do not query the database from a validator unless the project documents an exception.
- Validation messages are in the users' language: set FluentValidation's `LanguageManager` culture for the built-in messages, and write custom `WithMessage` texts in that language.
- Do not repeat validator rules in endpoints or request models.

## BreviERP specifics

- Business modules: `Accounting`, `Ai`, `Catalog`, `Content`, `Crm`, `Identity`, `Media`, `Production`, `Purchasing`, `Reference`, `Reminders`, `Warehouse`. Projects live in `src/Modules/<Module>/<Module>.<Layer>/<Module>.<Layer>/`, including `<Module>.Contracts`.
- Hosts are `src/Bootstrapper/Host.Api` (HTTP) and `src/Bootstrapper/Host.Seed` (migrations, seed, Cloud Run jobs). Register a new module in both when it has jobs or seed data.
- Operational jobs are in `src/Jobs` ([jobs overview](../../../src/Jobs/README.md)). `CatalogImport` and `SewingPricing` are data imports that may reference `Catalog.Infrastructure` and `Reference.Infrastructure` (the documented exception above).
- Architecture tests list the modules in `tests/ArchitectureTests/`; add a new module there.
- Container checks: `Host.Api` and `Host.Seed` turn on `ValidateScopes` and `ValidateOnBuild` in every environment; `tests/IntegrationTests/Platform/DependencyInjection/ServiceProviderValidationTests.cs` builds both containers in Production mode without a database. Each module registers its validators in `Add<Module>Module()` through `AddModuleValidators` (`BuildingBlocks.Application`), which skips child validators that a parent builds with `new`.
- Validation: `AddModuleValidators` switches FluentValidation to `UkrainianValidationLanguage` (`BuildingBlocks.Application/Validation`): the built-in messages are Ukrainian whatever the request culture and never name the field, because the admin shows each message under its own field. `tests/IntegrationTests/Platform/Validation/ValidationCoverageTests.cs` fails when a command or query with input has no validator or when a validator message is not Ukrainian or contains `{PropertyName}`. A request whose response is not `Result` (for example `PagedResult`) gets its validation failures from `GlobalExceptionHandler` as a 400 `ProblemDetails` with `errors` by field.
- Data isolation: `tests/IntegrationTests/Platform/Data/ModuleDataIsolationTests.cs` builds the EF model of every module `DbContext` in the API container (no database) and fails on an entity, navigation, or foreign key of another module or schema. Audit 2026-11-14: all 12 models are clean and no migration creates a cross-schema foreign key; the only cross-schema SQL is the one-time data takeover in the first Media and Warehouse migrations. Cross-module ids keep snapshots (Crm quote items, Production items and orders, Warehouse stock and movements, Content page texts).
- Errors: `GlobalExceptionHandler` (`BuildingBlocks.Api/Errors`) is the one place that turns an exception into an answer: `ValidationException` 400, `BadHttpRequestException` its status, anything else 500 «Сталася помилка на сервері.», always with `traceId` and never with the exception text; `tests/HostApiTests/GlobalExceptionHandlerTests.cs` guards it. `ExceptionBehavior` keeps only `DomainException` → 400 and a unique-key violation → 409 until the Errors PR. `Error(Code, Message, ErrorType)` lives in `BuildingBlocks.Domain/Errors`; `ErrorResults.ToResult()` (`BuildingBlocks.Application/Errors`) is the one place that turns it into an `Ardalis.Result` (`Validation` → Invalid with the code as the key, `NotFound`, `Conflict`, `Forbidden`, `Unexpected` → Error), and `ExceptionBehavior` uses it for a `DomainException`. `tests/UnitTests/Platform/Errors/ErrorTests.cs` fails when a module `*Errors` factory returns a non-Ukrainian message or a code without the module prefix.
- `Crm` is the reference implementation of a module schema: `HasDefaultSchema("crm")` and `MigrationsHistoryTable("__EFMigrationsHistory", "crm")`.

## Known deviations

Existing code that does not follow these rules yet. Do not copy it into new code. The [architecture audit of 2026-11-14](../architecture/README.md#structure-migration) moves it to the rules in stages; remove a row when its stage is merged.

| Where | Deviation | Plan |
| --- | --- | --- |
| Repository root | `deploy/` (Cloudflare Worker) and `deployment/` (catalogue import scripts) are folders the root layout does not list | Jobs audit stage: move under `tools/` or `src/Jobs` with the runbooks |
| Infrastructure entry points (ports and DI chain) | Not one `Add<Module>Infrastructure(configuration)` everywhere: `Catalog` (`AddCatalogDbContextServices` + `AddCatalogInfrastructureServices`), `Reference` (`AddReferenceDbContextServices`, `AddReferenceInfrastructureServices`, `AddReferenceServices`), `Identity` (`AddIdentityInfrastructure`, `AddIdentityInfrastructureServices`, `AddIdentityDbContextServices`, `AddIdentityAuthentication`), `Crm` (`AddCrmDbContextServices` + `AddCrmKeepinCrmSync`); `Host.Seed` calls these helpers directly; Mediator, access policies and the CSRF middleware (`Host.Api/Security`, which still reads `Identity.Infrastructure` options) stay in `Host.Api` | Stage 6 |
| Side effects (transactional outbox) | External calls (Telegram, Gmail, CDN, site revalidation, sheet write-backs) run directly in handlers, some after a second `SaveChanges` | Outbox stage |
| Errors (audit 2026-11-14) | About 270 business-rule `throw`s of `InvalidOperationException`/`ArgumentException` in Domain become 500; Catalog, Reference and Accounting still throw `DomainException` instead of returning the `Error`; handlers catch 14 `DomainException`, 6 own business exceptions and 19 `Exception`; `ToActionResult` answers with lists, not `ProblemDetails`, and 20 Api files pick status codes themselves. Fixed: `GlobalExceptionHandler`, pipeline catch-all removed (validation PR); shared `Error`/`ErrorType`, `ErrorResults` mapping, Ukrainian texts of every module error and result (errors part 1) | Errors part 2: Domain methods return `Result`/`Error` module by module, `catch` blocks in handlers removed; `ProblemDetails` bodies and the one Api mapper with the Minimal API stage |
| All Api projects | MVC controllers instead of Minimal API `Endpoints/<Feature>/` | Last stage, after the full audit and its fixes (OpenAPI snapshot unchanged) |
| CI test selection | Catalog and Reference tests are not all under `tests/<Project>/<Module>/`, so a change there (or in a module they reference) runs every test | Stage 9 |
| `Catalog`, `Reference`, `Identity`, `Accounting` Domain | Type folders `Entities/`, `Enums/`, `Errors/`, `ValueObjects/`, `Constants/` | Stage 4 |
| All Application projects | No `Commands/` and `Queries/` folders; `DTOs/`, `Validators/`, `Specifications/` type folders; separate `Public…` feature folders; `Contracts/` folders with DTOs and ports | Stage 5 |
| All Application projects, one use case = one folder | 216 handlers named `…QueryHandler`/`…CommandHandler` instead of `<UseCase>Handler`; 124 of 366 validators in `Validators/` subfolders, 50 `DTOs/` and 43 `Specifications/` subfolders inside use cases; 16 handlers share a folder with another handler and 15 files hold more than one request; ports in 19 `Contracts/` folders (and `Identity.Application/Contracrts`) instead of `Abstractions/` | Stage 5 |
| Handlers against the five steps (audit 2026-11-14, 423 handlers) | Two saves without one transaction: `CreateProductionOrder`, `CopyProductionOrder`, `SubmitSiteRequest`, `SendProductionOrderTelegram`, `ImportOldSiteCategories`, `LinkPurchasePayment`; `PreviewPpePrices` returns the Domain record `PpePriceCalculation`; 14 handlers over 150 lines (imports, `RepriceQuote`, `GetFinanceReport`, `BookBankStatement`, `TransferStock`) to check for business rules that belong in Domain. No handler sets entity properties directly; Domain types have no public setters; no query handler saves (two classes, `WorkshopSettingsHandler` and the timesheet handlers, serve a query and a command together and split in stage 5); `UploadFinanceAttachment` checks the record state before the aggregate call, to compare with the domain rule | Stage 5, per module |
| Api using Domain types (handler rule: Api holds no domain type) | 29 Api files import a `*.Domain` namespace: Accounting 13, Catalog 7, Content 7, Ai 1, Identity 1 (request mappers and controllers using enums and value objects) | Stage 7 |
| All Infrastructure projects | Flat `Configurations/` (`Congigurations/` in Catalog and Reference), `DataBase/`, `Repositories/`, `Seeders/`; external systems outside `Integrations/` | Stage 6 |
| `Catalog.Api`, `Reference.Api`, `Identity.Api` | `Controllers/` and `Contracts/` instead of `Features/<Feature>/` | Stage 7 |
| `Host.Seed/Jobs` | Job steps live in the host | Stage 8 |
| `tests/*` | Tests do not mirror modules and layers; 42 Catalog test files at the `UnitTests` root; no architecture tests for Catalog layers, Reference, Identity, Contracts, folders, hosts | Stage 9 |
| `Catalog` | Tables in the `public` schema | Keep: moving tables needs a separate migration and rollout plan |
| `Identity`, `Reference` | Own schema, but migration history in `public.__EFMigrationsHistory` | Keep: needs a separate migration decision |
| `Crm.Api` `UploadPrivateFileRequest`, `Catalog.Api` PPE pricing requests | Data annotations on request models | Move rules into the use-case validator when the request changes |
