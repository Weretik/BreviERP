# Структура Application

## Відповідальність

Application виконує один сценарій за раз. Вона координує Domain та абстракції; не володіє реалізацією збереження даних або транспорту.

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
├── Jobs/                       логіка job-сценарію, ніколи не планувальник wiring
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

- Команди змінюють стан; запити лише читають.
- Обробник має один сценарій та отримує лише абстракції.
- Запит, що повертає модель читання списку/деталей без доменної поведінки, використовує
  `Specification<TEntity, TResult>` з `AsNoTracking` і SQL `Select` у DTO; обробник
  не матеріалізує агрегат для подальшого ручного відображення.
- Запит списку з пагінацією повертає `PagedResult<IReadOnlyList<TItem>>` з `PagedInfo`,
  а не локальну оболонку сторінки. Чисте перетворення одного Result в інший виконуйте через
  `Result.Map`; послідовність кроків, що повертають Result, — через `Bind`/`BindAsync`,
  коли це не приховує перевірок стану.
- Валідатори відповідають за валідацію вхідних даних; Domain володіє бізнес-інваріантами.
- Кожна специфікація належить одному сценарію і лежить у його `Specifications/`. Не створюйте спільний `<Area>/Specifications/` і не імпортуйте специфікацію з іншого сценарію. Якщо два сценарії мають однакову просту умову, кожен має власну локальну специфікацію; виняток — лише справді спільний код у явно названому `Shared/`, після підтвердженої потреби.
- Публічні міжмодульні DTO розміщуйте в контракті модуля, не поряд з обробником.
