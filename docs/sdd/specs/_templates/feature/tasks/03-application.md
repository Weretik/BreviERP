# Фаза 03 — Application

> Ця фаза лише створює та впорядковує підфази. Не реалізовуйте всі Application use cases у цьому файлі.

- [ ] T013 Перевірити погоджені у фазі 00 CQRS use cases та результати Domain/Infrastructure phases.
- [ ] T014 Переглянути шаблони підфаз у розділі [Шаблони Application](#шаблони-application) і вибрати лише ті, що відповідають погодженим use cases.
- [ ] T015 Скопіювати кожен потрібний шаблон у `tasks/application/` як окремий файл фактичної підфази: `03.1-<назва>.md`, `03.2-<назва>.md` і далі; замінити `NN` номером, плейсхолдери — конкретними назвами й шляхами.
- [ ] T016 Призначити кожній підфазі конкретний use case або спільну Application-відповідальність; не змішувати кілька незалежних use cases в одному файлі.

## Шаблони Application

- [03.NN — Contracts та доступ до даних](application/03.NN-contracts.template.md)
- [03.NN — Get list](application/03.NN-get-list.template.md)
- [03.NN — Get by id](application/03.NN-get-by-id.template.md)
- [03.NN — Create](application/03.NN-create.template.md)
- [03.NN — Update](application/03.NN-update.template.md)
- [03.NN — Delete](application/03.NN-delete.template.md)
- [03.NN — Інший use case](application/03.NN-other-use-case.template.md)

## Checkpoint

Для кожного погодженого Application use case або спільної Application-відповідальності створено окремий файл підфази `03.N`; непотрібні шаблони не копіюються.

## Наступна фаза

[04 — API](04-api.md) — лише якщо feature має HTTP або integration API; інакше перейдіть до [05 — Verification](05-verification.md).
