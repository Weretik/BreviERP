# Фаза 02 — Планування Infrastructure

> Ця фаза лише створює та впорядковує підфази. Не реалізовуйте весь Infrastructure layer у цьому файлі.

- [ ] T009 Перевірити `design/infrastructure.md` і `data-model.md`; визначити потрібні persistence mapping, constraints, indexes, migration та read model.
- [ ] T010 Визначити, чи є зовнішня інтеграція, integration event, outbox, файл, cache або інший Infrastructure adapter у scope.
- [ ] T011 Переглянути шаблони підфаз у розділі [Шаблони Infrastructure](#шаблони-infrastructure) і вибрати лише потрібні Infrastructure slices.
- [ ] T012 Скопіювати кожен потрібний шаблон у `tasks/infrastructure/` як окремий файл фактичної підфази: `02.1-<назва>.md`, `02.2-<назва>.md` і далі; замінити `NN` номером, плейсхолдери — конкретними назвами й шляхами.

## Шаблони Infrastructure

- [02.NN — Persistence mapping](infrastructure/02.NN-persistence-mapping.template.md)
- [02.NN — Migration через EF Core CLI](infrastructure/02.NN-migration.template.md)
- [02.NN — Read model](infrastructure/02.NN-read-model.template.md)
- [02.NN — Integration або outbox](infrastructure/02.NN-integration-outbox.template.md)
- [02.NN — Infrastructure tests](infrastructure/02.NN-infrastructure-tests.template.md)

## Checkpoint

Для кожного потрібного Infrastructure-рішення створено окремий файл підфази `02.N`; застосовані migrations не редагуються.

## Наступна фаза

Після завершення всіх створених підфаз `02.N` перейдіть до [03 — Application](03-application.md).
