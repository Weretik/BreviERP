# Фаза 03 — Application

> Фаза лише впорядковує підфази. Кожен Product use case має окремий файл `03.N`.

- [x] O03-01 Завершити contracts і Reference/Media abstractions: [03.1-contracts-and-reference.md](application/03.1-contracts-and-reference.md).
- [x] O03-02 Зафіксувати реалізовані create/update handlers: [03.2-create-and-update.md](application/03.2-create-and-update.md).
- [ ] O03-03 Завершити production-ready read use cases: [03.3-admin-read-model.md](application/03.3-admin-read-model.md).
- [x] O03-04 Зафіксувати реалізований delete use case: [03.4-delete-product.md](application/03.4-delete-product.md).
- [ ] O03-05 Завершити validation/handler/projection tests: [03.5-application-tests.md](application/03.5-application-tests.md).

## Checkpoint

Кожен Product use case має окремий handler і `Ardalis.Result`. Проте list use case ще треба переробити на DB-side filtering/sorting/paging (`03.3`); після цього лишаються test докази у `03.5`.

## Наступна фаза

[04 — API](04-api.md)
