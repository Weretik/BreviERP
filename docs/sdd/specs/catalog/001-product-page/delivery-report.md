# Delivery report — 001 Product page

**Дата:** 2026-08-21
**Статус:** готово до handoff.

## Реалізовано

- Підфази `00.1`–`05.3` завершені: Product Domain, persistence/migration, CQRS, admin Product CRUD API, OpenAPI та automated tests.
- Admin list повертає `minimumWholesalePrice` і лише `mainPhoto` з URL або `null`; detail повертає всі фото з URL і застосовні Sewing/PPE ціни.
- Write boundary приймає лише канонічні строкові enum `Sewing`/`Ppe` та `Reference`/`Custom`; невалідні комбінації повертають `ValidationError[]`.

## Змінені матеріали

- Catalog Product Domain, Application, Infrastructure й API проєкти; unit, PostgreSQL integration та HTTP API tests.
- Feature requirements/design/contracts/OpenAPI, task records і delivery checklist.

## Докази verification

- `dotnet restore BreviERP.sln` — успішно.
- `dotnet build BreviERP.sln --no-restore` — успішно, 0 errors; 2 warnings поза Catalog.
- `dotnet test BreviERP.sln --no-build` із process-scoped `BREVIERP_CATALOG_TEST_CONNECTION` зі значення `Host.Api` `ConnectionStrings:Default` — 47 unit, 1 architecture і 22 integration tests passed.
- Product migration test — 1 passed.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-build --filter FullyQualifiedName~ProductsApiTests` — 8 passed: list, detail, create/update/delete, validation і conflict HTTP behavior.
- `npx --yes @apidevtools/swagger-cli validate docs/sdd/contracts/catalog/product-catalog.openapi.yaml` — valid.

## Delivery confirmation

- Користувач підтвердив ручні Swagger/API-сценарії Sewing/PPE create/update, list/detail,
  invalid combinations і Reference delete conflict 2026-08-21.

## Ризики

- Product endpoints тимчасово мають `AllowAnonymous`; перед production необхідно погодити й увімкнути authorization policy.
- Перший ручний `GET /api/v1/products` після старту Host API зафіксував EF `MultipleCollectionIncludeWarning` і 7740 ms у `GetAdminProductsQuery`. Після warm-up порожній list відповів за 323.7063 ms і 172.2967 ms без slow warning. `03.9` усуває EF warning, а `03.10` замінює повний Reference snapshot вузьким pricing reader. Числовий benchmark на representative dataset не зафіксовано, тому не є performance-SLA.
