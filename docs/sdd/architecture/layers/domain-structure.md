# Структура Domain

## Відповідальність

Domain володіє бізнес-моделлю та її інваріантами. Він має бути зрозумілим без знань ASP.NET Core, EF Core, SQL, HTTP або зовнішніх клієнтів.

## Цільова структура модуля

```text
<Module>.Domain/
├── Entities/                 агрегати та сутності
│   └── <Area>/
├── ValueObjects/             перевірені концептуальні значення й типізовані ID
├── Errors/                   фабрики/коди доменних помилок
├── Events/                   доменні події
├── Services/                 лише чисті доменні сервіси
├── Specifications/           domain-level специфікації, якщо застосовно
├── Constants/
└── <Module>.Domain.csproj
```

## Діаграма розміщення

```text
Application command
        │ викликає поведінку
        ▼
Aggregate / Entity ───→ Value object
        │ забезпечує інваріант │ перевіряє концепт
        ├──→ Domain error
        └──→ Domain event (накопичується, але не dispatch-иться тут)
```

## Правила

- Фабрики й поведінкові методи забезпечують інваріанти.
- Використовуйте value object, коли primitive дозволив би недопустимий стан.
- Доменні події описують завершений бізнес-факт; їх накопичує сутність.
- Request DTO, DbSet, EF configuration, repository implementation, HTTP call або logging dependency тут не належать.
