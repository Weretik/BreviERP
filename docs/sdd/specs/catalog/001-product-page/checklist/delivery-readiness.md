# Product page — checklist: готовність delivery

Це фінальна перевірка результату. Статус реалізації відмічається у `tasks/`, а тут перевіряється фактична відповідність коду, контракту й вимог.

## Domain і Application

- [x] Sewing не зберігає PPE-дані, а PPE не зберігає Sewing-дані.
- [x] Не можна додати duplicate operation, fabric або accessory ID; у Fabric list не більше двох основних тканин, і read-model показує їх першими.
- [x] `MetersPerProduct`, `Quantity` і PPE `BasePrice` валідовані як додатні; роздрібний і оптовий PPE-відсотки мають тільки валідні взаємовиключні стани.
- [x] Sewing-чернетка без операцій дозволена; розрахунок не ділить на нуль і передає `piecesPerShift` повним дробовим значенням без округлення.
- [x] Для кожної пари Sewing Product—Fabric read-model повертає три ціни за `requirements/pricing.md`; усі проміжні й кінцеві значення передаються без округлення.
- [x] Read-model повертає мінімальну й максимальну Sewing-ціну для кожного цінового діапазону разом з `FabricId`, який сформував значення.
- [x] Admin list передає `minimumWholesalePrice`: мінімум усіх наявних Sewing-цін або PPE wholesale price, а за неможливого розрахунку — `0`.
- [x] Admin list передає лише вибране головне фото з `mediaFileId` і URL або `null`; admin detail передає URL усіх фото й усі застосовні Sewing/PPE ціни.
- [x] Application перевіряє Reference IDs і `%` unit без залежності Catalog від Reference Infrastructure.
- [x] Спроба видалити використану operation, fabric або accessory відхиляється без зміни Product і повертає узгоджену помилку конфлікту.

## Persistence та API

- [x] EF one-to-one details, link unique constraints, міграція й rollback перевірені на test database.
- [x] Міграція не змінює таблиці поза scope і не редагує застосовані міграції.
- [x] Реалізований API відповідає `docs/sdd/contracts/catalog/product-catalog.openapi.yaml`: маршрути, DTO, status codes, errors, security та приклади.
- [x] Controller не містить бізнес-правил; усі use-cases проходять через Mediator, FluentValidation, `IUnitOfWork` і `CancellationToken`.

## Докази завершення

- [x] Виконано `dotnet restore BreviERP.sln`, `dotnet build BreviERP.sln --no-restore`, `dotnet test BreviERP.sln --no-build`.
- [x] Пройдені unit, integration, migration і ручні Swagger/API-сценарії з підфаз `tasks/verification/05.N-*.md`.
- [x] Документація, OpenAPI і код не розходяться; відхилення від вимог задокументовані.
- [x] Усі застосовні конкретні задачі з підфаз `tasks/**/NN.N-*.md` мають `[x]`.
