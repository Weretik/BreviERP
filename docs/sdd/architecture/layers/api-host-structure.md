# Структура API та Host

## Структура API-модуля

```text
<Module>.Api/
├── Controllers/
│   └── <Area>Controller.cs
├── Contracts/
├── GlobalUsing.cs
└── <Module>.Api.csproj
```

Контролери зв’язують HTTP input, викликають `ISender.Send` із `CancellationToken` та повертають усталений Result mapping. Вони не виконують EF query, не конструюють repositories і не містять domain rules.

## Структура хоста

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

## Request pipeline

```text
HTTP
  → developer diagnostics або exception handler
  → forwarded headers, CORS, HTTPS, Serilog request logging, rate limiter
  → authentication → authorization
  → controller → ISender
  → request logging → performance → validation → exception → domain events
  → handler → Result-to-HTTP mapping
```

Host є composition root. Він може реєструвати модулі та middleware, але не має реалізовувати бізнес-сценарій.
