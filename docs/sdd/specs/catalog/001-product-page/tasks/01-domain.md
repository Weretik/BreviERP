# Фаза 01 — Domain

**Залежності:** [00-readiness.md](00-readiness.md)  
**Вхід:** `requirements/`, `design/domain.md`, `data-model.md`

- [ ] T007 Прочитати наявний aggregate `src/Modules/Catalog/Catalog.Domain/Catalog.Domain/Products/Entities/Product.cs` і зберегти чинні правила фото та категорій.
- [ ] T008 Створити типоспецифічні domain entities/value objects у `src/Modules/Catalog/Catalog.Domain/Catalog.Domain/Products/` для Sewing details, тканин, фурнітури, операцій, PPE details і `PpeCoefficient` відповідно до `data-model.md`.
- [ ] T009 Розширити `Product` методами створення та зміни type-specific даних; заборонити Sewing-дані для PPE і PPE-дані для Sewing.
- [ ] T010 Реалізувати доменні інваріанти: унікальність operation/fabric/accessory ID, одна основна тканина в непорожньому списку, додатні `MetersPerProduct`/`Quantity`, взаємовиключні стани `PpeCoefficient`.
- [ ] T011 Реалізувати calculation value object/service для показників Sewing з обробкою пустого списку та нульової суми; округлювати `piecesPerShift` вниз.
- [ ] T012 [P] Додати або оновити unit-тести в `tests/UnitTests/` для T009–T011: межі типів, дублікати, коефіцієнт, чернетка, ділення на нуль і округлення.
- [ ] T013 Запустити цільові unit-тести; виправити лише дефекти Domain цієї feature.

## Checkpoint

Domain не залежить від Infrastructure або Reference entity. Усі інваріанти з `checklist/delivery-readiness.md` для Domain покриті тестами.

