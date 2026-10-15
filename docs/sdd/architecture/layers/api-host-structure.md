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

Контролери зв’язують вхідні дані HTTP, викликають `ISender.Send` із `CancellationToken` та повертають усталене відображення Result. Вони не виконують запити EF, не створюють репозиторіїв і не містять доменних правил.

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

## Конвеєр HTTP-запиту

```text
HTTP
  → діагностика розробника або обробник винятків
  → переслані заголовки, CORS, HTTPS, журналювання HTTP-запитів Serilog, обмеження частоти
  → автентифікація → авторизація
  → controller → ISender
  → журналювання запиту → вимірювання швидкодії → валідація → обробка винятків → доменні події
  → обробник → відображення Result у HTTP
```

Host є точкою складання залежностей. Він може реєструвати модулі та проміжне програмне забезпечення, але не має реалізовувати бізнес-сценарій.
