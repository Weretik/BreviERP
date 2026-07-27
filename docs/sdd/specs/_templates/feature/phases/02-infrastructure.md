# Фаза 02: Infrastructure та persistence

**Статус:** чернетка  
**Залежить від:** фази 01, якщо функціональність не є read-only над наявною моделлю  
**Блокує:** application-сценарії з persistence

## Результат

Функціональність зберігається та інтегрується за наявними conventions infrastructure модуля.

## Проєктування

- Вплив на DbContext і DbSet:
- EF `IEntityTypeConfiguration`:
- Індекси, обмеження та concurrency:
- Реалізація repository/read-context:
- Стратегія read-projection або no-tracking query:
- Зовнішній адаптер або DI-реєстрація:
- Міграція: потрібна | не потрібна, з причиною:
- Backfill даних/перебудова проєкції, якщо потрібно:

## План файлів

```text
<Module>.Infrastructure/
├── DataBase/
├── Configurations/
├── Migrations/                 лише після погодження
├── Repositories/
├── Projections/
├── Integrations/
└── DependencyInjection/
```

## Критерії приймання

- [ ] Domain/Application не посилаються на Infrastructure.
- [ ] Read-шляхи уникають tracking і N+1-запитів, де це застосовно.
- [ ] Вплив міграції та rollback зафіксовано або є обґрунтування відсутності міграції.
- [ ] Реєстрація адаптерів дотримується extension-патерну модуля.

## Перевірка

- Integration/persistence-тест:
- Перевірка міграції/проєкції:
- Ризики:
