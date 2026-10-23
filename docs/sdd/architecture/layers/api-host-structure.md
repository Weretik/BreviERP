# API and Host Structure

## API module structure

```text
<Module>.Api/
├── Controllers/
│   └── <Area>Controller.cs
├── Contracts/
├── GlobalUsing.cs
└── <Module>.Api.csproj
```

Controllers bind HTTP input, call `ISender.Send` with a `CancellationToken`, and return the established Result mapping. They do not run EF queries, create repositories, or contain domain rules.

## Host structure

```text
Bootstrapper/Host.Api/
├── Program.cs
├── DependencyInjection/
│   └── ServiceRegistration/
│       ├── ApplicationRegistrationExtensions.cs
│       ├── Pipeline/
│       ├── SecurityRegistrationExtensions.cs
│       └── Web/
├── Properties/launchSettings.json
└── appsettings*.json
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

Host is the composition root. It may register modules and middleware, but must not implement business use cases.
