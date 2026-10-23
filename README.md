# BreviERP

An ERP backend for Brevi. The repository contains a modular ASP.NET Core API, business modules, and infrastructure for catalog, reference data, users, and accounting.

> This project is under active development. API contracts and implemented capabilities are described in the [documentation](docs/README.md).

## Technology

- .NET 10 and C#
- ASP.NET Core Web API
- PostgreSQL and Entity Framework Core
- Clean Architecture, DDD, and CQRS
- Mediator, FluentValidation, and Ardalis.Result
- ASP.NET Core Identity, Serilog, and OpenAPI/Swagger
- NUnit for unit, integration, and architecture tests

## Modules

| Module | Responsibility |
| --- | --- |
| Identity | Users, roles, sessions, and authorization. |
| Catalog | Products, categories, and catalog media. |
| Reference | Suppliers, fabrics, accessories, garment parts, and operations. |
| Accounting | Wallets, transactions, and transaction categories. |
| Crm | A dedicated CRM module boundary; domain API scenarios are still in progress. |

## Architecture

The project follows modular Clean Architecture. Dependencies point toward the domain:

```text
Host.Api / Host.Seed
        │
        ├── Module.Api ──→ Module.Application ──→ Module.Domain
        └── Module.Infrastructure ──────────────→ Module.Application + Module.Domain
```

`BuildingBlocks` contains shared technical and domain primitives and does not depend on business modules.

## Quick start

### Prerequisites

- .NET SDK 10
- PostgreSQL accessible from the local environment
- Local secrets and a database connection string

Clone the repository and run:

```powershell
dotnet restore BreviERP.sln
dotnet run --project src/Bootstrapper/Host.Api/Host.Api.csproj --launch-profile https
```

The API will be available at `https://localhost:7142`, and the health check at `https://localhost:7142/health`.

In the `Development` environment, Swagger UI is available at `https://localhost:7142/swagger`.

To run without HTTPS:

```powershell
dotnet run --project src/Bootstrapper/Host.Api/Host.Api.csproj --launch-profile http
```

## Configuration

`Host.Api` uses standard ASP.NET Core configuration. For sensitive local values, use User Secrets or environment variables such as `ConnectionStrings__Default`.

```powershell
dotnet user-secrets set "ADMIN_DEFAULT_PASSWORD" "<local-password>" --project src/Bootstrapper/Host.Api/Host.Api.csproj
```

Do not commit passwords, tokens, or production connection strings. The full settings list is in the [configuration documentation](docs/sdd/operations/configuration/configuration-and-secrets.md).

## Migrations and seed data

A separate host applies migrations and seed data in a controlled way:

```powershell
dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj
```

This command can change the database schema and data. Before running it, especially against a nonlocal database, read the [migrations and seeders guide](docs/sdd/operations/data/migrations-and-seeding.md).

## Verification

```powershell
dotnet restore BreviERP.sln
dotnet build BreviERP.sln --no-restore
dotnet test BreviERP.sln --no-build
```

## Repository structure

```text
BreviERP.sln
├── src/
│   ├── Bootstrapper/       # HTTP API and seed host
│   ├── BuildingBlocks/     # shared abstractions and cross-cutting code
│   └── Modules/            # business modules and their layers
├── tests/                  # unit, integration, and architecture tests
└── docs/                   # versioned engineering documentation
```

## Documentation

- [Architecture](docs/sdd/architecture/README.md)
- [Engineering rules](docs/sdd/standards/README.md)
- [Local startup and operations](docs/sdd/operations/README.md)
- [Feature specifications](docs/sdd/specs/README.md)
- [OpenAPI contract](docs/sdd/contracts/openapi.yaml)

## License

See [LICENSE.txt](LICENSE.txt).
