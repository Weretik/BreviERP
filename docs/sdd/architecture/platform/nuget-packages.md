# BreviERP direct NuGet dependencies

**Verified:** 2026-10-15 against the `.csproj` files in `src/` and `tests/`. All 61 direct `PackageReference` declarations (45 unique name/version pairs) are listed below, not transitive dependencies. Versions in `.csproj` are the source of truth; update this catalog in the same commit when a package changes. The owning project is identified by each table heading. The purpose describes the package role, but the dependency declaration alone does not prove runtime use. The “verified in code” label refers to use in the repository, not necessarily in every owning project.

## Dependency group roles

- **Host.Api:** ASP.NET Core, Mediator source generator, OpenAPI/Swagger, Serilog, EF Core tools, and Scriban templating. Check concrete behavior in `src/Bootstrapper/Host.Api/Program.cs` and dependency registrations.
- **Host.Seed:** a separate migration and seeding host; its startup is described in the [operational instructions](../../operations/data/migrations-and-seeding.md).
- **BuildingBlocks.Domain:** typed values and domain primitives; **BuildingBlocks.Application:** results, specifications, validation, Mediator, EF/Npgsql, and shared services; **BuildingBlocks.Api:** map results to HTTP.
- **Catalog.Infrastructure:** slug normalization and transliteration for module adapters.
- **Tests:** NUnit, the test SDK and adapter, mocks, coverage, API TestHost, and YAML/OpenAPI validation. Test packages must not be presented as runtime dependencies.

## Examples of verified usage

| Pattern | Source in code |
| --- | --- |
| Mediator and behavior ordering | [pipeline registration](../../../../src/Bootstrapper/Host.Api/DependencyInjection/ServiceRegistration/Pipeline/MediatorRegistrationExtensions.cs) |
| FluentValidation | [validator registration](../../../../src/Bootstrapper/Host.Api/DependencyInjection/ServiceRegistration/Pipeline/FluentValidationRegistrationExtensions.cs) |
| EF Core with PostgreSQL | [Catalog DbContext registration](../../../../src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure/DependencyInjection/CatalogDbContextExtensions.cs) |
| Serilog, Swagger, and OpenAPI | [HTTP host](../../../../src/Bootstrapper/Host.Api/Program.cs) |
| Slugify.Core and TranslitKit | [Catalog slug generator](../../../../src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure/Products/SlugGeneration/ProductSlugGenerator.cs) |
| libphonenumber-csharp | [phone number helper service](../../../../src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application/Helpers/PhoneNumberHelper.cs) |
| Moq and NUnit | [Product unit tests](../../../../tests/UnitTests/ProductCommandHandlerTests.cs) |
| YamlDotNet | [session OpenAPI contract validation](../../../../tests/UnitTests/SessionOpenApiContractTests.cs) |

For a package marked “declared,” find a call, type, or configuration in the code before claiming it is actually used. If a package appears unnecessary, removing it requires a separate build and test check.

## [Host.Api.csproj](../../../../src/Bootstrapper/Host.Api/Host.Api.csproj)

| Package | Version from `.csproj` | Package purpose | Verification status |
| --- | --- | --- | --- |
| `Mediator.SourceGenerator` | `3.0.1` | Generate the Mediator implementation | Verified in code |
| `Microsoft.AspNetCore.OpenApi` | `10.0.2` | Generate OpenAPI in the host | Verified in code |
| `Microsoft.EntityFrameworkCore.Tools` | `10.0.2` | EF Core command-line tools | Declared in `.csproj`; actual use not independently verified |
| `Microsoft.OpenApi` | `2.7.5` | OpenAPI model | Declared in `.csproj`; actual use not independently verified |
| `Scriban` | `7.2.3` | Text templating engine | Declared in `.csproj`; actual use not independently verified |
| `Serilog.AspNetCore` | `10.0.0` | Serilog integration with ASP.NET Core | Verified in code |
| `Serilog.Enrichers.Environment` | `3.0.1` | Environment properties for logs | Declared in `.csproj`; actual use not independently verified |
| `Serilog.Enrichers.Process` | `3.0.0` | Process properties for logs | Declared in `.csproj`; actual use not independently verified |
| `Serilog.Settings.Configuration` | `10.0.0` | Configure Serilog from configuration | Declared in `.csproj`; actual use not independently verified |
| `Serilog` | `4.3.0` | Structured logging | Verified in code |
| `Swashbuckle.AspNetCore` | `10.0.0` | Swagger UI and OpenAPI tooling | Verified in code |

## [Host.Seed.csproj](../../../../src/Bootstrapper/Host.Seed/Host.Seed.csproj)

| Package | Version from `.csproj` | Package purpose | Verification status |
| --- | --- | --- | --- |
| `Serilog.AspNetCore` | `10.0.0` | Serilog integration with ASP.NET Core | Verified in code |

## [BuildingBlocks.Api.csproj](../../../../src/BuildingBlocks/BuildingBlocks.Api/BuildingBlocks.Api.csproj)

| Package | Version from `.csproj` | Package purpose | Verification status |
| --- | --- | --- | --- |
| `Ardalis.Result` | `10.1.0` | Standardized use-case results and their HTTP mapping | Verified in code |

## [BuildingBlocks.Application.csproj](../../../../src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application/BuildingBlocks.Application.csproj)

| Package | Version from `.csproj` | Package purpose | Verification status |
| --- | --- | --- | --- |
| `Ardalis.Result.FluentValidation` | `10.1.0` | Convert FluentValidation errors into results | Declared in `.csproj`; actual use not independently verified |
| `Ardalis.Result` | `10.1.0` | Standardized use-case results and their HTTP mapping | Verified in code |
| `Ardalis.Specification.EntityFrameworkCore` | `9.3.1` | Execute specifications in EF Core | Verified in code |
| `Ardalis.Specification` | `9.3.1` | Describe query rules and read specifications | Verified in code |
| `DotNetEnv` | `3.1.1` | Load `.env` configuration | Declared in `.csproj`; actual use not independently verified |
| `FluentValidation.DependencyInjectionExtensions` | `12.1.1` | Register validators in the dependency-injection container | Verified in code |
| `FluentValidation` | `12.1.1` | Validate incoming commands and queries | Verified in code |
| `libphonenumber-csharp` | `9.0.23` | Normalize and validate phone numbers | Verified in code |
| `Mediator.Abstractions` | `3.0.1` | Mediator request and handler contracts | Verified in code |
| `Microsoft.AspNetCore.Components.Authorization` | `10.0.2` | ASP.NET Core authorization contracts | Declared in `.csproj`; actual use not independently verified |
| `Microsoft.AspNetCore.Http.Abstractions` | `2.3.9` | HTTP abstractions | Declared in `.csproj`; actual use not independently verified |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | `10.0.2` | Persist Identity data through EF Core | Declared in `.csproj`; actual use not independently verified |
| `Microsoft.EntityFrameworkCore.Design` | `10.0.2` | EF Core design-time support | Declared in `.csproj`; actual use not independently verified |
| `Microsoft.EntityFrameworkCore.Relational` | `10.0.2` | EF Core relational features | Declared in `.csproj`; actual use not independently verified |
| `Microsoft.EntityFrameworkCore` | `10.0.2` | ORM and DbContext | Verified in code |
| `Microsoft.Extensions.Logging.Abstractions` | `10.0.2` | Logging abstractions | Declared in `.csproj`; actual use not independently verified |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | `10.0.0` | EF Core provider for PostgreSQL | Verified in code |
| `Serilog.AspNetCore` | `10.0.0` | Serilog integration with ASP.NET Core | Verified in code |
| `Serilog.Enrichers.Environment` | `3.0.1` | Environment properties for logs | Declared in `.csproj`; actual use not independently verified |
| `Serilog.Enrichers.Process` | `3.0.0` | Process properties for logs | Declared in `.csproj`; actual use not independently verified |
| `Serilog.Enrichers.Thread` | `4.0.0` | Thread properties for logs | Declared in `.csproj`; actual use not independently verified |
| `Serilog.Settings.Configuration` | `10.0.0` | Configure Serilog from configuration | Declared in `.csproj`; actual use not independently verified |
| `Serilog.Sinks.Console` | `6.1.1` | Write logs to the console | Declared in `.csproj`; actual use not independently verified |
| `Serilog.Sinks.Seq` | `9.0.0` | Write logs to Seq | Declared in `.csproj`; actual use not independently verified |
| `Unidecode.NET` | `2.2.1` | Unicode transliteration | Declared in `.csproj`; actual use not independently verified |

## [BuildingBlocks.Domain.csproj](../../../../src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj)

| Package | Version from `.csproj` | Package purpose | Verification status |
| --- | --- | --- | --- |
| `Ardalis.SmartEnum` | `8.2.0` | Typed domain enums | Verified in code |
| `Vogen` | `8.0.4` | Typed value objects | Verified in code |

## [Catalog.Infrastructure.csproj](../../../../src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure/Catalog.Infrastructure.csproj)

| Package | Version from `.csproj` | Package purpose | Verification status |
| --- | --- | --- | --- |
| `Slugify.Core` | `5.1.1` | Generate a URL slug | Verified in code |
| `TranslitKit` | `1.0.3` | Transliterate the name before generating a URL slug | Verified in code |

## [ArchitectureTests.csproj](../../../../tests/ArchitectureTests/ArchitectureTests.csproj)

| Package | Version from `.csproj` | Package purpose | Verification status |
| --- | --- | --- | --- |
| `Microsoft.NET.Test.Sdk` | `18.0.1` | Run .NET tests | Declared in `.csproj`; actual use not independently verified |
| `NUnit.Analyzers` | `4.11.2` | NUnit analyzers | Declared in `.csproj`; actual use not independently verified |
| `NUnit` | `4.4.0` | Test framework | Verified in code |
| `NUnit3TestAdapter` | `6.1.0` | Connect NUnit to the test runner | Declared in `.csproj`; actual use not independently verified |

## [IntegrationTests.csproj](../../../../tests/IntegrationTests/IntegrationTests.csproj)

| Package | Version from `.csproj` | Package purpose | Verification status |
| --- | --- | --- | --- |
| `coverlet.collector` | `6.0.4` | Collect test coverage | Declared in `.csproj`; actual use not independently verified |
| `Microsoft.AspNetCore.TestHost` | `10.0.2` | HTTP test host | Declared in `.csproj`; actual use not independently verified |
| `Microsoft.NET.Test.Sdk` | `18.0.1` | Run .NET tests | Declared in `.csproj`; actual use not independently verified |
| `Moq` | `4.20.72` | Mock objects for tests | Verified in code |
| `NUnit.Analyzers` | `4.11.2` | NUnit analyzers | Declared in `.csproj`; actual use not independently verified |
| `NUnit` | `4.4.0` | Test framework | Verified in code |
| `NUnit3TestAdapter` | `6.1.0` | Connect NUnit to the test runner | Declared in `.csproj`; actual use not independently verified |

## [UnitTests.csproj](../../../../tests/UnitTests/UnitTests.csproj)

| Package | Version from `.csproj` | Package purpose | Verification status |
| --- | --- | --- | --- |
| `coverlet.collector` | `6.0.4` | Collect test coverage | Declared in `.csproj`; actual use not independently verified |
| `Microsoft.NET.Test.Sdk` | `18.0.1` | Run .NET tests | Declared in `.csproj`; actual use not independently verified |
| `Microsoft.OpenApi.YamlReader` | `2.7.5` | Read OpenAPI YAML in tests | Declared in `.csproj`; actual use not independently verified |
| `Moq` | `4.20.72` | Mock objects for tests | Verified in code |
| `NUnit.Analyzers` | `4.11.2` | NUnit analyzers | Declared in `.csproj`; actual use not independently verified |
| `NUnit` | `4.4.0` | Test framework | Verified in code |
| `NUnit3TestAdapter` | `6.1.0` | Connect NUnit to the test runner | Declared in `.csproj`; actual use not independently verified |
| `YamlDotNet` | `16.3.0` | Parse YAML in tests | Verified in code |

When adding a package, record in the feature specification what need it addresses, why existing tools are insufficient, and how it affects architecture, security, deployment, and verification. [Technology stack](technology-stack.md) describes current approaches without duplicating this list.
