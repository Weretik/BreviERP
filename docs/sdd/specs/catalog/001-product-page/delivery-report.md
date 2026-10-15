# Звіт про постачання — 001 Сторінка товару

**Дата:** 2026-08-21
**Статус:** готово до handoff.

## Реалізовано

- Підфази `00.1`–`05.3` завершені: Product Domain, persistence/migration, CQRS, admin Product CRUD API, OpenAPI та automated tests.
- Список адміністратора повертає `minimumWholesalePrice` і лише `mainPhoto` з URL або `null`; детальна відповідь повертає всі фото з URL і застосовні ціни Sewing/PPE.
- Межа запису приймає лише канонічні рядкові значення enum `Sewing`/`Ppe` та `Reference`/`Custom`; невалідні комбінації повертають `ValidationError[]`.

## Змінені матеріали

- Проєкти Catalog Product Domain, Application, Infrastructure й API; модульні тести, інтеграційні тести PostgreSQL та тести HTTP API.
- Вимоги, проєктні рішення, контракти й OpenAPI фічі, записи завдань і контрольний список поставки.

## Свідчення перевірки

- `dotnet restore BreviERP.sln` — успішно.
- `dotnet build BreviERP.sln --no-restore` — успішно, 0 errors; 2 warnings поза Catalog.
- `dotnet test BreviERP.sln --no-build` із `BREVIERP_CATALOG_TEST_CONNECTION`, заданою лише для процесу зі значення `Host.Api` `ConnectionStrings:Default`, — успішно пройшли 47 модульних, 1 архітектурний і 22 інтеграційні тести.
- Тест міграції Product — 1 успішний.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-build --filter FullyQualifiedName~ProductsApiTests` — пройшли 8 тестів: список, деталі, створення/оновлення/видалення, валідація та поведінка HTTP при конфлікті.
- `npx --yes @apidevtools/swagger-cli validate docs/sdd/contracts/catalog/product-catalog.openapi.yaml` — valid.

## Підтвердження постачання

- Користувач підтвердив ручні Swagger/API-сценарії Sewing/PPE create/update, list/detail,
  invalid combinations і Reference delete conflict 2026-08-21.

## Ризики

- Кінцеві точки Product тимчасово мають `AllowAnonymous`; перед виробничим запуском необхідно погодити й увімкнути політику авторизації.
- Перший ручний `GET /api/v1/products` після старту Host API зафіксував EF `MultipleCollectionIncludeWarning` і 7740 мс у `GetAdminProductsQuery`. Після прогрівання порожній список відповів за 323.7063 мс і 172.2967 мс без попередження про повільний запит. `03.9` усуває попередження EF, а `03.10` замінює повний знімок Reference вузьким читачем даних ціноутворення. Числове вимірювання на репрезентативному наборі даних не зафіксовано, тому це не є гарантованим показником швидкодії.
