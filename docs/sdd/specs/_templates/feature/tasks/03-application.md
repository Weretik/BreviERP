# Фаза 03 — Планування Application-задач

> Створіть один малий файл `TS-*` на кожен use case або спільну Application-відповідальність.

- [ ] Вивести Application use cases зі сценаріїв `SC-*`, а не з припущень про CRUD.
- [ ] Вибрати лише шаблони, що відповідають погодженій поведінці.
- [ ] Надати кожній задачі ID `TS-*`, покриті сценарії, залежності, точні шляхи, test level і checkpoint.
- [ ] Де практично, тримати validation, result-поведінку, orchestration і сфокусовані тести в одній задачі use case.
- [ ] Додати кожну Application-задачу до `traceability.md`.

## Шаблони

- [Contracts і доступ до даних](application/03.NN-contracts.template.md)
- [Get list](application/03.NN-get-list.template.md)
- [Get by id](application/03.NN-get-by-id.template.md)
- [Create](application/03.NN-create.template.md)
- [Update](application/03.NN-update.template.md)
- [Delete](application/03.NN-delete.template.md)
- [Інший use case](application/03.NN-other-use-case.template.md)

## Checkpoint

Кожен погоджений use case пов'язано з його acceptance-сценаріями. Задачі не дублюють Domain rules і не додають HTTP concerns.
