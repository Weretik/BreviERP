# Фаза 03 — Application і інтеграція з Reference

**Залежності:** [01-domain.md](01-domain.md), [02-infrastructure.md](02-infrastructure.md)  
**Вхід:** `contracts/product-catalog.openapi.yaml`, `requirements/`, `design/domain.md`

- [ ] T020 Створити Product use-case папки у `src/Modules/Catalog/Catalog.Application/Catalog.Application/Features/Product/` за чинною CQRS-конвенцією модуля.
- [ ] T021 Створити commands, request DTO, FluentValidation validators і Mediator handlers для create/update Product та зміни фото, категорій, Sewing/PPE details.
- [ ] T022 Реалізувати read-only queries і Ardalis.Specification projections для admin list/detail та погодженого public product page read-model без зайвого tracking.
- [ ] T023 Додати application abstractions для перевірки `SupplierId`, `FabricId`, `GarmentAccessoryId`, `GarmentPartOperationId` і `AdditionalReferenceId`; реалізація не залежить від Reference Infrastructure.
- [ ] T024 У handlers перевірити існування Reference IDs, одиницю `%` для коефіцієнта та готовність `MediaFile` перед зміною aggregate.
- [ ] T025 Створити contracts і handlers для ідемпотентного очищення Product links після подій видалення Reference data; додати structured logging і correlation context за чинними правилами.
- [ ] T026 [P] Додати unit-тести validators, handlers, projections та повторної delivery події.
- [ ] T027 Перевірити cancellation token, `IUnitOfWork`, domain-event pipeline і `Ardalis.Result` у всіх нових use-cases.

## Checkpoint

Кожен use-case має один Mediator handler, усі Reference-перевірки виконуються в Application, а повторна подія не створює помилку чи дублікати.

