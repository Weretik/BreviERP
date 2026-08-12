# Фази реалізації feature

Починайте з `00-readiness.md`. Усі основні фази `00–05` є лише orchestration: деталізацію створюйте окремими файлами підфаз `00.1`, `01.1`… `05.1` у відповідних папках. У підфазах замініть `NN` фактичним номером і призначте унікальні послідовні task IDs.

Не додавайте всю реалізацію до основних фаз: кожна конкретна задача має бути окремим файлом `00.N`, `01.N`, `02.N`, `03.N`, `04.N` або `05.N`.

## Обов'язковий початок

- [00 — Уточнення та готовність](00-readiness.md)
- [01 — Планування Domain](01-domain.md)
- [02 — Планування Infrastructure](02-infrastructure.md)

## Readiness — лише потрібні підфази

- [00.NN — Scope, рішення та CQRS use cases](readiness/00.NN-scope.template.md)
- [00.NN — CLI, scripts та generators](readiness/00.NN-tooling.template.md)

## Domain — лише потрібні slices

- [01.NN — Aggregate та інваріанти](domain/01.NN-aggregate.template.md)
- [01.NN — Value object](domain/01.NN-value-object.template.md)
- [01.NN — Domain events](domain/01.NN-domain-events.template.md)
- [01.NN — Domain tests](domain/01.NN-domain-tests.template.md)

## Infrastructure — лише потрібні slices

- [02.NN — Persistence mapping](infrastructure/02.NN-persistence-mapping.template.md)
- [02.NN — Migration через EF Core CLI](infrastructure/02.NN-migration.template.md)
- [02.NN — Read model](infrastructure/02.NN-read-model.template.md)
- [02.NN — Integration або outbox](infrastructure/02.NN-integration-outbox.template.md)
- [02.NN — Infrastructure tests](infrastructure/02.NN-infrastructure-tests.template.md)

## Application — лише потрібні підфази

- [03 — Application](03-application.md)
- [03.NN — Application contracts](application/03.NN-contracts.template.md)
- [03.NN — Get list](application/03.NN-get-list.template.md)
- [03.NN — Get by id](application/03.NN-get-by-id.template.md)
- [03.NN — Create](application/03.NN-create.template.md)
- [03.NN — Update](application/03.NN-update.template.md)
- [03.NN — Delete](application/03.NN-delete.template.md)
- [03.NN — Інший use case](application/03.NN-other-use-case.template.md)

Кожен Application use case містить свою валідацію, `Ardalis.Result` і unit-тести.

## API — лише потрібні підфази для HTTP/integration feature

Створюйте потрібні підфази після всіх погоджених Application use cases у такому порядку:

1. [04 — API](04-api.md)
2. [04.NN — Controllers та endpoints](api/04.NN-controllers.template.md)
3. [04.NN — HTTP contracts, права та Result mapping](api/04.NN-http-behavior.template.md)
4. [04.NN — OpenAPI-документація](api/04.NN-contract-documentation.template.md)
5. [04.NN — API/integration tests](api/04.NN-tests.template.md)
6. [05 — Verification](05-verification.md)

Не створюйте API-фази лише для feature без HTTP/integration surface.

## Verification — лише потрібні підфази

- [05 — Verification](05-verification.md)
- [05.NN — Build та automated tests](verification/05.NN-build-tests.template.md)
- [05.NN — Delivery documentation та report](verification/05.NN-delivery.template.md)
