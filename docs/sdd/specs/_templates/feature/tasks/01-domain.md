# Фаза 01 — Планування Domain-задач

> Створіть лише файли Domain `TS-*`, потрібні сценаріям. Не реалізовуйте всю feature в цьому orchestration-файлі.

- [ ] Переглянути правила сценаріїв, `design/domain.md` і `data-model.md`.
- [ ] Визначити aggregates, value objects, invariants і переходи стану.
- [ ] Визначити, чи потрібні domain events для ефектів між aggregates.
- [ ] Створити одну малу Domain-задачу на відповідальність із релевантного шаблону.
- [ ] Надати кожній задачі ID `TS-*`, `Covers: SC-*`, залежності, точні шляхи, test level і checkpoint.
- [ ] Додати кожну Domain-задачу до `traceability.md`.

## Шаблони

- [Aggregate та invariants](domain/01.NN-aggregate.template.md)
- [Value object](domain/01.NN-value-object.template.md)
- [Domain events](domain/01.NN-domain-events.template.md)
- [Domain coverage](domain/01.NN-domain-tests.template.md)

## Checkpoint

Кожна потрібна Domain-відповідальність має одну простежувану технічну задачу. Domain не залежить від Application, Infrastructure або API. Для сценаріїв без нової Domain-поведінки в matrix явно вказано `—`.
