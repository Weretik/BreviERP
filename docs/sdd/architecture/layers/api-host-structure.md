# API and Host Structure

## API module structure

```text
<Module>.Api/
├── Endpoints/<Feature>/                 same feature names as Application (Settings, Pricing/Ppe, Users)
│   ├── <Feature>Endpoints.cs            Map<Feature>Endpoints(): one MapGroup per audience with route prefix, policy, tags
│   └── <UseCase>Endpoint.cs             Map() and the handler of one use case, with its <UseCase>Request next to it
├── Access/                              policy names of the module's pages
├── DependencyInjection/
│   └── <Module>ModuleExtensions.cs      Add<Module>Module() (Application, Infrastructure, Contracts implementations)
│                                        and Map<Module>Endpoints()
├── GlobalUsing.cs
└── <Module>.Api.csproj
```

Endpoints bind HTTP input (`ISender` with `[FromServices]`), call `ISender.Send` with a `CancellationToken`, and return the shared Result mapping (`ToHttpResult`, `ToNoContentHttpResult` in `BuildingBlocks.Api/Result`); modules not yet moved still use controllers with `ToActionResult`. The host maps every moved module in `MapModuleEndpoints()`. They do not run EF queries, create repositories, or contain domain rules. Only `DependencyInjection/` may use Infrastructure types. No folder is named after a kind of class (`Controllers/`, `Contracts/`, `Requests/`, `Mapping/`, `Options/`, `Results/`); `tests/ArchitectureTests/ApiLayoutTests.cs` checks it. The shared result mapping (`ToActionResult`, `ToNoContentResult`) lives in `BuildingBlocks.Api/Result`.

## Host structure

The host is the composition root and nothing more. It holds no readers, adapters, notifiers, or any class that knows two modules' data ([ADR-0006](../decisions/0006-module-contracts.md)).

```text
Bootstrapper/Host.Api/
├── Program.cs                    nothing else besides DependencyInjection/ (ModuleCompositionTests)
├── DependencyInjection/          one file per technical concern
│   ├── ModulesRegistrationExtensions.cs   list of Add<Module>Module() calls
│   ├── Pipeline/, Web/, Observability/
│   └── SecurityRegistrationExtensions.cs
├── Properties/launchSettings.json
└── appsettings*.json

Bootstrapper/Host.Seed/
├── Program.cs                    parses the command line and picks the job
├── SeedHostServicesExtensions.cs registers the modules the jobs use
└── Jobs/
    ├── SeedJobDispatcher.cs      mutually exclusive command selection
    ├── JobCommands.cs            SendAsync: runs a module command, a failed result fails the step
    ├── Sync/                     brevierp-sync steps (ISyncTask), each sends one module command
    ├── Pricing/                  runners that call src/Jobs/SewingPricing and CatalogImport.PpePricing
    └── *CommandRunner.cs         other thin runners
```

`tests/ArchitectureTests/JobDependencyTests.cs` checks that a job uses no module Infrastructure, no DbContext and resolves only `ISender` and `ILoggerFactory`.

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
