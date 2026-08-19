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
│       ├── Specifications/     EF/Ardalis specifications лише цього сценарію
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
- Query, що повертає list/detail read-model без Domain behavior, використовує
  `Specification<TEntity, TResult>` з `AsNoTracking` і SQL `Select` у DTO; handler
  не materialize-ить aggregate для подальшого ручного mapping.
- Paginated list query повертає `PagedResult<IReadOnlyList<TItem>>` з `PagedInfo`,
  а не локальний page-wrapper. Pure Result-to-Result mapping виконуйте через
  `Result.Map`; послідовність Result-returning кроків — через `Bind`/`BindAsync`,
  коли це не приховує state checks.
- Validators володіють input validation; Domain володіє бізнес-інваріантами.
- Кожна specification належить одному use case і лежить у його `Specifications/`. Не створюйте спільний `<Area>/Specifications/` і не імпортуйте specification з іншого use case. Якщо два сценарії мають однаковий простий predicate, кожен має власну локальну specification; виняток — лише справді спільний код у явно названому `Shared/`, після підтвердженої потреби.
- Публічний cross-module DTO розміщуйте в контракті модуля, не поряд із handler-ом.
