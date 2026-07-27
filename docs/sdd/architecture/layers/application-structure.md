# Структура Application

## Відповідальність

Application виконує один сценарій за раз. Вона координує Domain та абстракції; не володіє реалізацією persistence або transport.

## Цільова структура на функціональність

```text
<Module>.Application/
├── Features/
│   └── <Area>/<UseCase>/
│       ├── <UseCase>Command.cs | <UseCase>Query.cs
│       ├── <UseCase>CommandHandler.cs | <UseCase>QueryHandler.cs
│       ├── DTOs/               приватні DTO сценарію, лише за потреби
│       ├── Validators/
│       │   └── <UseCase>Validator.cs
│       └── Extensions/         лише цілісні LINQ/mapping helpers
├── Contracts/
│   ├── Persistence/            абстракції DbContext/read/repository
│   ├── Integrations/           абстракції зовнішніх сервісів
│   └── Services/
├── Integrations/               application orchestration зовнішніх потоків
├── Jobs/                       логіка job-сценарію, ніколи не scheduler wiring
└── <Module>ApplicationAssemblyMarker.cs
```

## Візуалізація сценарію

```text
Command або Query
       │
       ▼
FluentValidation validator ── invalid ──→ Result.Invalid
       │ valid
       ▼
Handler ──→ Domain aggregate / Application abstraction
       │
       └──→ Result.Success | Result.NotFound | Result.Conflict | Result.Invalid
```

## Правила

- Commands записують; queries лише читають.
- Handler має один сценарій та отримує лише абстракції.
- Query використовує projection/no-tracking, де це доречно.
- Validators володіють input validation; Domain володіє бізнес-інваріантами.
- Публічний cross-module DTO розміщуйте в контракті модуля, не поряд із handler-ом.
