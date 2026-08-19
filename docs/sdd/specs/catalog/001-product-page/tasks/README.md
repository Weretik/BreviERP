# Фази реалізації Product page

Головні фази `00–05` лише впорядковують підфази. Кожен конкретний блок роботи
має окремий файл `NN.N`; `[x]` означає, що стан підтверджений наявним кодом,
попередніми test evidence або документацією feature.

## 00 — Readiness

- [00 — orchestration](00-readiness.md)
- [00.1 — Scope та CQRS use cases](readiness/00.1-scope-and-use-cases.md) — виконано
- [00.2 — CLI та generators](readiness/00.2-cli-and-generators.md) — виконано

## 01 — Domain

- [01 — orchestration](01-domain.md)
- [01.1 — Product aggregate](domain/01.1-product-aggregate.md) — виконано
- [01.2 — Domain tests](domain/01.2-domain-tests.md) — виконано

## 02 — Infrastructure

- [02 — orchestration](02-infrastructure.md)
- [02.1 — Product persistence](infrastructure/02.1-product-persistence.md) — виконано
- [02.2 — Product migration](infrastructure/02.2-product-migration.md) — виконано

## 03 — Application

- [03 — orchestration](03-application.md)
- [03.1 — Contracts та Reference](application/03.1-contracts-and-reference.md) — виконано
- [03.2 — Create та update](application/03.2-create-and-update.md) — виконано
- [03.3 — Admin read model](application/03.3-admin-read-model.md) — виконано
- [03.4 — Delete Product](application/03.4-delete-product.md) — виконано
- [03.5 — Application tests](application/03.5-application-tests.md) — виконано
- [03.6 — Localized validation](application/03.6-validation-localization.md) — виконано
- [03.7 — Production hardening](application/03.7-production-hardening.md) — виконано
- [03.8 — Enriched admin read models](application/03.8-catalog-read-model-enrichment.md) — виконано

## 04 — API

- [04 — orchestration](04-api.md)
- [04.1 — ProductsController](api/04.1-products-controller.md) — виконано
- [04.2 — HTTP contracts та behavior](api/04.2-http-contracts-and-behavior.md) — виконано
- [04.2.1 — Write boundary validation та mapper tests](api/04.2.1-write-boundary-validation-and-mapper-tests.md) — не виконано; коригувальна підфаза перед OpenAPI
- [04.3 — OpenAPI](api/04.3-openapi-documentation.md) — не виконано; YAML існує, але ще не звірений з API-кодом
- [04.4 — API tests](api/04.4-api-tests.md) — не виконано

## 05 — Verification

- [05 — orchestration](05-verification.md)
- [05.1 — Build та tests](verification/05.1-build-and-tests.md) — не виконано
- [05.2 — Delivery documentation](verification/05.2-delivery-documentation.md) — не виконано
