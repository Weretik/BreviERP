# Фаза 03 — Application

> Фаза лише впорядковує підфази. Кожен Product use case має окремий файл `03.N`.

- [x] O03-01 Завершити contracts і Reference/Media abstractions: [03.1-contracts-and-reference.md](application/03.1-contracts-and-reference.md).
- [x] O03-02 Зафіксувати реалізовані create/update handlers: [03.2-create-and-update.md](application/03.2-create-and-update.md).
- [x] O03-03 Завершити production-ready read use cases: [03.3-admin-read-model.md](application/03.3-admin-read-model.md).
- [x] O03-04 Зафіксувати реалізований delete use case: [03.4-delete-product.md](application/03.4-delete-product.md).
- [x] O03-05 Завершити validation/handler/projection tests: [03.5-application-tests.md](application/03.5-application-tests.md).
- [x] O03-06 Локалізувати та повністю покрити validation Product use cases: [03.6-validation-localization.md](application/03.6-validation-localization.md).
- [ ] O03-07 Закрити production-hardening risks Application: [03.7-production-hardening.md](application/03.7-production-hardening.md).

## Checkpoint

Кожен Product use case має окремий handler і `Ardalis.Result`. List виконує filtering/sorting/paging на DB-side; validation, handler та projection test докази зафіксовані у `03.5`, а локалізована validation-модель — у `03.6`. Production hardening для category validation, photo IDs, concurrent conflicts, detail projection і architecture checks лишається відкритим у `03.7`.

## Наступна фаза

[04 — API](04-api.md)
