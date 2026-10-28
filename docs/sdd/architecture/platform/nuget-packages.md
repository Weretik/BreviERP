# BreviERP NuGet dependencies

Versions and owning projects live only in the `.csproj` files under `src/` and `tests/`; read them there (for example `rg PackageReference src tests`). This document records the role of each dependency group and where usage is verified. Update it only when a group's role or a key library changes, never to copy versions.

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

For any package, find a call, type, or configuration in the code before claiming it is actually used. If a package appears unnecessary, removing it requires a separate build and test check.
