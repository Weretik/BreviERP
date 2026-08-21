# Product page — performance admin list query

**Статус:** реалізовано у коригувальному слайсі `03.9`
**Scope:** `GET /api/v1/products`; HTTP/OpenAPI contract, schema та Product business rules не змінюються.

## Спостереження

Перший `GET /api/v1/products` після старту Host API 2026-08-19 повернув `200 OK`, але в логах були:

- EF Core `MultipleCollectionIncludeWarning` для query з кількома collection navigation;
- `Slow request GetAdminProductsQuery: 7740 ms` за порогу 500 ms;
- тривалість HTTP-запиту 9142 ms.

Повторні прогріті запити без перезапуску Host API повернули `200 OK` за 323.7063 ms та
172.2967 ms. Обидва значення нижчі за поріг slow request 500 ms. Отже, на порожньому
списку зафіксована затримка є cold-start cost (JIT, EF model/query compilation або перше
DB connection), а не сталою latency list endpoint.

Поточний `GetAdminProductsSpec` проєктує `Categories`, головне фото та Sewing collections
в один list-read query. Повний час handler також включає count, Media URL lookup і
`IProductReferenceReader.GetSnapshotAsync`, тому warning не вважається єдиною причиною
затримки без вимірювання.

## Рішення

1. Для поточної projection застосувати локальний EF Core `AsSplitQuery()`: collection
   results матеріалізуються окремими фіксованими SQL queries, а не одним cartesian query.
   Якщо наступне профілювання покаже, що fixed split-query set не вкладається в ціль,
   наступним кроком буде плоский page projection і bounded batch projections за ID сторінки.
2. Не застосовувати глобальний `QuerySplittingBehavior` і не додавати `Include` без SQL evidence.

## Незмінні властивості

- Filtering, category descendants, sorting, stable secondary sort `Id`, paging і total count
  лишаються DB-side та семантично ідентичними.
- Response зберігає `categoryIds`, `mainPhoto` URL/null і `minimumWholesalePrice`.
- Додаткові SQL operations bounded за page, а не за кількістю Product rows; N+1 не допускається.
- Міграції, таблиці, OpenAPI YAML та authorization не змінюються.

## Критерії приймання

- Integration test не генерує `MultipleCollectionIncludeWarning` для list page projection.
- SQL/projection tests підтверджують відсутність N+1 і збереження response contract.

## Реалізований крок

`GetAdminProductsSpec` використовує `Query.AsNoTracking().AsSplitQuery()`.
PostgreSQL integration test налаштовує `MultipleCollectionIncludeWarning` як exception
і успішно виконує projection.
