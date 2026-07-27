# Правила використання BuildingBlocks

## Повторно використовуйте перед створенням

| Потреба | Спочатку використовуйте | Не робіть |
| --- | --- | --- |
| Очікуваний збій сценарію | Ardalis.Result і наявний Result mapping | не кидайте звичайні validation/not-found exceptions |
| Input validation | FluentValidation плюс ValidationBehavior | не перевіряйте лише в контролері |
| Бізнес-інваріант | `IDomainError`/`DomainException` або aggregate behavior | не переносіть domain rule у behavior |
| Поточний користувач/permissions | `ICurrentUserService` або `IPermissionService` | не впроваджуйте `IHttpContextAccessor` в Application |
| Domain event | collection `BaseEntity` плюс dispatcher behavior | не publish-те напряму з контролера |
| DB migration/seeding | startup extensions `IDatabaseMigrator`/`ISeeder` | не мігруйте/seed-те всередині handler-а |
| Request diagnostics | наявні behaviors/log helpers | не логуйте raw sensitive payloads |

## Правило розширення

Додавайте shared primitive лише коли щонайменше два модулі потребують однакової сталої технічної abstraction. Feature-specific abstraction залишається у власному Application-модулі. Бізнес-концепт ніколи не переноситься до BuildingBlocks.

Документуйте додавання або зміну порядку cross-cutting infrastructure в ADR і специфікації функціональності.
