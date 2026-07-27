# Архітектура backend

BreviERP — модульний ASP.NET Core backend на .NET 10. Він використовує Clean Architecture, DDD, CQRS, Mediator, FluentValidation, Ardalis.Result, EF Core/PostgreSQL і Serilog.

## Оберіть за питанням

### Потрібна загальна картина

- [Карта системи](overview/system-map.md) — хости, модулі й напрямок залежностей.
- [Структура проєкту](overview/project-structure.md) — дерево рішення та візуалізація шарів.
- [Межі шарів](overview/layers.md) — ownership і заборонені залежності.
- [Карта модулів](overview/modules.md) — Identity, Catalog, Reference, Accounting, Crm і BuildingBlocks.

### Потрібно розмістити код у шарі

- [Domain](layers/domain-structure.md)
- [Application](layers/application-structure.md)
- [Infrastructure](layers/infrastructure-structure.md)
- [API та Host](layers/api-host-structure.md)

### Потрібно зрозуміти виконання

- [Потік сценарію](runtime/use-case-flow.md)
- [Mediator behaviors](runtime/mediator-behaviors.md)
- [Кросфункціональний pipeline](runtime/cross-cutting.md)
- [Persistence та інтеграції](runtime/persistence-and-integrations.md)

### Потрібна довідка платформи

- [Технологічний стек](platform/technology-stack.md)
- [Довідка BuildingBlocks](platform/building-blocks/README.md)

### Потрібна ідентичність або контроль доступу

- [Identity, автентифікація та авторизація](security/README.md)

Документи архітектури описують сталі правила. Запланована зміна належить до `docs/sdd/specs`, а не сюди, якщо лише вона не змінює стале правило.
