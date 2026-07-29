# Фаза 02 — Infrastructure і міграція

**Залежності:** [01-domain.md](01-domain.md)  
**Вхід:** `data-model.md`, `design/infrastructure.md`

- [ ] T014 Розширити `src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure/Congigurations/ProductConfiguration.cs` та додати EF configurations для detail і link entities зі схеми `data-model.md`.
- [ ] T015 Налаштувати one-to-one PK/FK для Sewing/PPE details і unique constraints для `(ProductId, FabricId)`, `(ProductId, GarmentAccessoryId)` та `(ProductId, GarmentPartOperationId)`.
- [ ] T016 Не додавати EF navigation properties або physical FK до Reference DbContext; зберегти Reference IDs як value objects/скаляри.
- [ ] T017 Створити нову EF migration у `src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure/Migrations/`; не редагувати `20260727004045_InitialCatalog`.
- [ ] T018 [P] Додати integration-тести EF mapping, PK/FK, unique constraints і lifecycle Product у `tests/IntegrationTests/`.
- [ ] T019 Застосувати міграцію на порожній test database, перевірити створену схему та rollback до rollout.

## Checkpoint

Схема містить тільки потрібні таблиці; міграція не змінює наявні таблиці поза scope.

