# Product page — design: поточна технічна база

## Реалізовано

- Domain: спільна Product-модель, взаємовиключні Sewing/PPE details, тканини, фурнітура, операції, ціноутворення та валідація інваріантів.
- Infrastructure: EF mapping detail/link-таблиць, Catalog migration і rollback, SQL read projections та міжмодульна перевірка використання Reference-даних.
- Application: create, full replace, list, detail і delete CQRS use cases, Reference validation та збагачені admin read-models.
- API: `/api/v1/products` CRUD, строкові enum на write boundary, OpenAPI contract і HTTP/API integration tests.

## Залишкова перевірка

- Ручні Swagger/API сценарії для Sewing/PPE create/update та Reference delete conflict ще не проведені в запущеному Host API; див. `tasks/verification/05.2-delivery-documentation.md`.

## Технічні правила інтеграції з Reference

- Application перевіряє існування `SupplierId`, `FabricId`, `GarmentAccessoryId`, `GarmentPartOperationId` і вибраних `AdditionalReferenceId` для роздрібного/оптового відсотків перед зміною товару; для кожного такого значення також перевіряється одиниця `%`.
- Product зберігає лише IDs довідникових даних і властивості власних зв’язків; не копіює назви, ціни, контакти або довідникові entities.
- Видалення `GarmentPartOperation`, `Fabric` чи `GarmentAccessory` відхиляється, якщо сутність використовується хоча б одним Product. Зв’язки Product не очищуються автоматично й міжмодульні події для цього не публікуються.
- Видалення `MediaFile` відхиляється, якщо він прив’язаний до Product як фото. Спочатку посилання потрібно прибрати з Product.
- Розрахунок Sewing читає актуальну тривалість операції; якщо потрібна історична норма, це окрема технічна зміна.

## Відкрита performance-корекція

Під час ручного `GET /api/v1/products` 2026-08-19 зафіксовано
`MultipleCollectionIncludeWarning`, `GetAdminProductsQuery` 7740 ms і HTTP 9142 ms.
Реалізація та вимірювана повторна перевірка описані відповідно в
[admin-list-query-performance.md](admin-list-query-performance.md),
`tasks/application/03.9-admin-list-query-performance.md` і
`tasks/verification/05.3-admin-list-query-performance-verification.md`.
