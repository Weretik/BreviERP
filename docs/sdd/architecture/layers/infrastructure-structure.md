# Структура Infrastructure

## Відповідальність

Infrastructure — шар адаптерів. Він реалізує Application contracts і зберігає/передає дані до технологічно специфічних систем.

## Цільова структура модуля

```text
<Module>.Infrastructure/
├── DataBase/
│   └── <Module>DbContext.cs
├── Configurations/          EF IEntityTypeConfiguration класи
├── Migrations/              EF migrations і model snapshot
├── Repositories/            EF repository implementations
├── Projections/             побудова та persistence read-моделі
├── Integrations/            HTTP/queue/storage adapter implementations
├── Services/                storage, export, notification adapters
├── DependencyInjection/     Add<Module>... extension methods
└── <Module>.Infrastructure.csproj
```

## Візуалізація адаптера

```text
Application abstraction
          ▲
          │ реалізує
Infrastructure adapter
     ├── EF Core DbContext ──→ PostgreSQL
     ├── external client ────→ storage / інший сервіс
     └── projection builder ─→ read model
```

## Правила

- EF configurations і migrations ніколи не переносяться в Domain або Application.
- Реєструйте адаптери через наявний module DI extension.
- Infrastructure може посилатися на Application та Domain; зворотний напрямок заборонено.
- Тримайте мапінг зовнішнього payload на межі адаптера та не допускайте generated client types усередину.
