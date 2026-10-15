# Фази реалізації сторінки товару

Головні фази `00–05` лише впорядковують підфази. Кожен конкретний блок роботи
має окремий файл `NN.N`; `[x]` означає, що стан підтверджений наявним кодом,
попередніми свідченнями перевірки або документацією фічі.

## 00 — Готовність

- [00 — координація](00-readiness.md)
- [00.1 — межі та сценарії використання CQRS](readiness/00.1-scope-and-use-cases.md) — виконано
- [00.2 — CLI та генератори](readiness/00.2-cli-and-generators.md) — виконано

## 01 — Domain

- [01 — координація](01-domain.md)
- [01.1 — агрегат Product](domain/01.1-product-aggregate.md) — виконано
- [01.2 — тести Domain](domain/01.2-domain-tests.md) — виконано

## 02 — Infrastructure

- [02 — координація](02-infrastructure.md)
- [02.1 — збереження Product](infrastructure/02.1-product-persistence.md) — виконано
- [02.2 — міграція Product](infrastructure/02.2-product-migration.md) — виконано

## 03 — Application

- [03 — координація](03-application.md)
- [03.1 — контракти та Reference](application/03.1-contracts-and-reference.md) — виконано
- [03.2 — створення й оновлення](application/03.2-create-and-update.md) — виконано
- [03.3 — адміністративна модель читання](application/03.3-admin-read-model.md) — виконано
- [03.4 — видалення Product](application/03.4-delete-product.md) — виконано
- [03.5 — тести Application](application/03.5-application-tests.md) — виконано
- [03.6 — локалізована валідація](application/03.6-validation-localization.md) — виконано
- [03.7 — посилення для промислового середовища](application/03.7-production-hardening.md) — виконано
- [03.8 — збагачені адміністративні моделі читання](application/03.8-catalog-read-model-enrichment.md) — виконано
- [03.9 — швидкодія запиту адміністративного списку Product](application/03.9-admin-list-query-performance.md) — виконано
- [03.10 — вузький читач Reference для цін](application/03.10-admin-list-pricing-reference-reader.md) — виконано

## 04 — API

- [04 — координація](04-api.md)
- [04.1 — ProductsController](api/04.1-products-controller.md) — виконано
- [04.2 — HTTP-контракти та поведінка](api/04.2-http-contracts-and-behavior.md) — виконано
- [04.2.1 — валідація межі запису й тести маперів](api/04.2.1-write-boundary-validation-and-mapper-tests.md) — виконано
- [04.3 — OpenAPI](api/04.3-openapi-documentation.md) — виконано
- [04.4 — тести API](api/04.4-api-tests.md) — виконано

## 05 — Перевірка

- [05 — координація](05-verification.md)
- [05.1 — збирання й тести](verification/05.1-build-and-tests.md) — виконано
- [05.2 — документація постачання](verification/05.2-delivery-documentation.md) — виконано
- [05.3 — перевірка швидкодії адміністративного списку Product](verification/05.3-admin-list-query-performance-verification.md) — виконано
