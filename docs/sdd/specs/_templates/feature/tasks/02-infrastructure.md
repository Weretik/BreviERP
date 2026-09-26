# Фаза 02 — Планування Infrastructure-задач та enablers

> Створіть лише файли Infrastructure `TS-*` і спільні `EN-*`, потрібні сценаріям.

- [ ] Переглянути сценарії, `design/infrastructure.md` і `data-model.md`.
- [ ] Визначити persistence, constraints, migrations, read models, integrations, outbox-поведінку, configuration та operational prerequisites.
- [ ] Використовувати `TS-*` для безпосередньо тестованої поведінки, а `EN-*` — для спільної або підготовчої роботи із задокументованим test-first винятком.
- [ ] Додати `Covers` або `Enables`, залежності, точні шляхи, verification і checkpoint до кожної створеної задачі.
- [ ] Додати кожну задачу до `traceability.md`.

## Шаблони

- [Спільний enabler](enablers/EN-NNN-enabler.template.md)
- [Persistence mapping](infrastructure/02.NN-persistence-mapping.template.md)
- [Migration](infrastructure/02.NN-migration.template.md)
- [Read model](infrastructure/02.NN-read-model.template.md)
- [Integration або outbox](infrastructure/02.NN-integration-outbox.template.md)
- [Infrastructure coverage](infrastructure/02.NN-infrastructure-tests.template.md)

## Checkpoint

Кожну потрібну Infrastructure-відповідальність або спільну передумову зіставлено зі сценаріями. Застосовані migrations не редагуються, generated work використовує CLI репозиторію, а кожен виняток `EN-*` має замінну verification.
