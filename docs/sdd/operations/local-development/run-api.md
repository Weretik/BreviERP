# Running Host.Api locally

## Prerequisites

- A .NET SDK matching the solution target (`net10.0`).
- PostgreSQL reachable through `ConnectionStrings:Default`.
- Required local secrets configured; see [configuration and secrets](../configuration/configuration-and-secrets.md).

## Start

From the repository root:

```powershell
dotnet restore BreviERP.sln
dotnet run --project src/Bootstrapper/Host.Api/Host.Api.csproj --launch-profile https
```

The `https` profile listens on `https://localhost:7142` and `http://localhost:5231`. Use the HTTPS address when checking authentication/refresh: the refresh cookie is configured for secure use.

For an HTTP-only endpoint check, use the `http` profile:

```powershell
dotnet run --project src/Bootstrapper/Host.Api/Host.Api.csproj --launch-profile http
```

Before running operations that apply migrations or seed data to a nonlocal database, read [migrations and seeding](../data/migrations-and-seeding.md).

## Stop and rebuild

Stop the running Host.Api process before building if Windows has locked its output DLL. Then run the needed build/test commands from the [testing standards](../../standards/testing-rules.md).
