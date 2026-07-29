# Фаза 04 — API

**Залежності:** [03-application.md](03-application.md), погоджений `contracts/product-catalog.openapi.yaml`

- [ ] T028 Створити DTO в `src/Modules/Catalog/Catalog.Api/Catalog.Api/Contracts/Products/` строго за погодженим `contracts/product-catalog.openapi.yaml`.
- [ ] T029 Створити або розширити `ProductsController` у `src/Modules/Catalog/Catalog.Api/Catalog.Api/Controllers/`; controller лише мапить HTTP ↔ Mediator і не містить бізнес-правил.
- [ ] T030 Додати погоджені authorization policies до write endpoints; не послаблювати чинну security-конфігурацію.
- [ ] T031 Повернути `Ardalis.Result` через наявний API mapping і зберегти стандартний формат validation/error response.
- [ ] T032 [P] Додати integration/API-тести для create/update, type boundaries, Reference validation, authorization і read-model.
- [ ] T033 Звірити реалізовані маршрути, DTO, приклади й коди результатів з `contracts/product-catalog.openapi.yaml`; не вважати фазу завершеною за розбіжності контракту й коду.

## Checkpoint

Усі endpoints відповідають затвердженому контракту, а API не містить доменних правил або залежностей від Reference Infrastructure.

