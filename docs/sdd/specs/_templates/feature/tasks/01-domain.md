# Фаза 01 — Планування Domain

> Ця фаза лише створює та впорядковує підфази. Не реалізовуйте весь Domain layer у цьому файлі.

- [ ] T005 Перевірити `design/domain.md` і `data-model.md`; визначити потрібні aggregate roots, value objects, інваріанти та допустимі state transitions.
- [ ] T006 Визначити, чи потрібні domain events для side effects між aggregates; не створювати їх для простого внутрішнього виклику.
- [ ] T007 Переглянути шаблони підфаз у розділі [Шаблони Domain](#шаблони-domain) і вибрати лише потрібні Domain slices.
- [ ] T008 Скопіювати кожен потрібний шаблон у `tasks/domain/` як окремий файл фактичної підфази: `01.1-<назва>.md`, `01.2-<назва>.md` і далі; замінити `NN` номером, плейсхолдери — конкретними назвами й шляхами.

## Шаблони Domain

- [01.NN — Aggregate та інваріанти](domain/01.NN-aggregate.template.md)
- [01.NN — Value object](domain/01.NN-value-object.template.md)
- [01.NN — Domain events](domain/01.NN-domain-events.template.md)
- [01.NN — Domain tests](domain/01.NN-domain-tests.template.md)

## Checkpoint

Для кожного потрібного Domain-рішення створено окремий файл підфази `01.N`; Domain не залежить від Infrastructure.

## Наступна фаза

Після завершення всіх створених підфаз `01.N` перейдіть до [02 — Планування Infrastructure](02-infrastructure.md).
