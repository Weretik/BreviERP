# Фаза 04 — API

> Ця фаза лише створює та впорядковує підфази. Не реалізовуйте весь API layer у цьому файлі.

- [ ] T017 Якщо feature має HTTP/integration surface, переглянути [Шаблони API](#шаблони-api) і вибрати потрібні підфази.
- [ ] T018 Скопіювати кожен потрібний шаблон у `tasks/api/` як окремий файл фактичної підфази: `04.1-<назва>.md`, `04.2-<назва>.md` і далі; замінити `NN` номером і плейсхолдери — конкретними назвами й шляхами.
- [ ] T019 Виконувати API-підфази у порядку: controllers/endpoints → HTTP contracts, authorization і Result mapping → OpenAPI → API tests.
- [ ] T020 Не створювати API-підфази лише для feature без HTTP або integration API.

## Шаблони API

- [04.NN — Controllers та endpoints](api/04.NN-controllers.template.md)
- [04.NN — HTTP contracts, права та Result mapping](api/04.NN-http-behavior.template.md)
- [04.NN — OpenAPI-документація](api/04.NN-contract-documentation.template.md)
- [04.NN — API/integration tests](api/04.NN-tests.template.md)

## Checkpoint

Для HTTP/integration feature API layer має окремі файли підфаз `04.N` для controllers, HTTP-поведінки, документації й integration tests.

## Наступна фаза

[05 — Verification](05-verification.md)
