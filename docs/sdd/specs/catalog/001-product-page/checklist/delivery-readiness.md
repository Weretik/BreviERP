# Product page — checklist: готовність delivery

Це фінальна перевірка результату. Статус реалізації відмічається у `tasks/`, а тут перевіряється фактична відповідність коду, контракту й вимог.

## Domain і Application

- [ ] Sewing не зберігає PPE-дані, а PPE не зберігає Sewing-дані.
- [ ] Не можна додати duplicate operation, fabric або accessory ID; у непорожньому Fabric list рівно одна основна тканина.
- [ ] `MetersPerProduct` і `Quantity` валідовані як додатні; `PpeCoefficient` має тільки валідні взаємовиключні стани.
- [ ] Sewing-чернетка без операцій дозволена; розрахунок не ділить на нуль і округлює `piecesPerShift` вниз.
- [ ] Application перевіряє Reference IDs і `%` unit без залежності Catalog від Reference Infrastructure.
- [ ] Повторна подія видалення Reference data не створює помилку чи дублікати; логування дозволяє діагностувати delivery.

## Persistence та API

- [ ] EF one-to-one details, link unique constraints, міграція й rollback перевірені на test database.
- [ ] Міграція не змінює таблиці поза scope і не редагує застосовані міграції.
- [ ] Реалізований API відповідає `contracts/product-catalog.openapi.yaml`: маршрути, DTO, status codes, errors, security та приклади.
- [ ] Controller не містить бізнес-правил; усі use-cases проходять через Mediator, FluentValidation, `IUnitOfWork` і `CancellationToken`.

## Докази завершення

- [ ] Виконано `dotnet restore BreviERP.sln`, `dotnet build BreviERP.sln --no-restore`, `dotnet test BreviERP.sln --no-build`.
- [ ] Пройдені unit, integration, migration і ручні Swagger/API-сценарії з `tasks/05-verification.md`.
- [ ] Документація, OpenAPI і код не розходяться; відхилення від вимог задокументовані.
- [ ] Усі застосовні T001–T040 мають `[x]`; невиконане має явний blocker, власника та план продовження.

