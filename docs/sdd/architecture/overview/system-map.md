# Карта системи

## Виконувані хости

- `src/Bootstrapper/Host.Api` компонує DI, middleware, контролери, автентифікацію та OpenAPI.
- `src/Bootstrapper/Host.Seed` є console-хостом для підтверджених операцій початкового наповнення.

## Бізнес-модулі

- Catalog володіє товарами, категоріями та медіафайлами каталогу.
- Reference володіє довідниками постачальників, тканин, фурнітури, частин виробу й операцій.
- Identity володіє ASP.NET Identity, ролями, політиками, інтеграцією current-user/session та seeders.
- Accounting володіє гаманцями, транзакціями й категоріями транзакцій.
- Crm присутній як модульна межа, але не має реалізованих предметних endpoint-ів.
- BuildingBlocks володіє спільними абстракціями та кросфункціональною реалізацією; він не може набувати бізнес-правил.

## Напрямок залежностей

```text
Host.Api / Host.Seed
        │
        ├── Module.Api ──→ Module.Application ──→ Module.Domain
        └── Module.Infrastructure ──────────────→ Module.Application + Module.Domain

BuildingBlocks.* постачає спільні примітиви; він не залежить від бізнес-модулів.
```

Немає прямої залежності Domain від Application чи Infrastructure, а також залежності Application від конкретного DbContext, HTTP client, scheduler-а або EF repository.
