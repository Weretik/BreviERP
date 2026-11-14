# API and Host Structure

## API module structure

```text
<Module>.Api/
├── Features/<Feature>/
│   ├── <Feature>Controller.cs           one audience (admin or public) of one feature
│   └── <Request>.cs                     request models of these controllers
├── DependencyInjection/
│   └── <Module>ModuleExtensions.cs      Add<Module>Module(): Application, Infrastructure, Contracts implementations, controllers
├── GlobalUsing.cs
└── <Module>.Api.csproj
```

Controllers bind HTTP input, call `ISender.Send` with a `CancellationToken`, and return the established Result mapping. They do not run EF queries, create repositories, or contain domain rules. Only `DependencyInjection/` may use Infrastructure types.

## Host structure

The host is the composition root and nothing more. It holds no readers, adapters, notifiers, or any class that knows two modules' data ([ADR-0006](../decisions/0006-module-contracts.md)).

```text
Bootstrapper/Host.Api/
├── Program.cs
├── DependencyInjection/          one file per technical concern
│   ├── ModulesRegistrationExtensions.cs   list of Add<Module>Module() calls
│   ├── Pipeline/, Web/, Observability/, Options/
│   └── SecurityRegistrationExtensions.cs
├── Security/                     access policies, CSRF, trusted proxy middleware
├── Properties/launchSettings.json
└── appsettings*.json

Bootstrapper/Host.Seed/
├── Program.cs                    parses the command line and picks the job
├── DependencyInjection/
└── Jobs/<Job>/                   thin runners that call the owning module's use case or a src/Jobs project
```

## HTTP request pipeline

```text
HTTP
  → developer diagnostics or exception handler
  → forwarded headers, CORS, HTTPS, Serilog HTTP request logging, rate limiting
  → authentication → authorization
  → controller → ISender
  → request logging → performance measurement → validation → exception handling → domain events
  → handler → Result-to-HTTP mapping
```

The host may register modules and middleware, but must not implement business use cases or cross-module adapters.
