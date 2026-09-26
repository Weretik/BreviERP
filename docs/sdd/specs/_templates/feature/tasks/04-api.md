# Фаза 04 — Планування API й contract-задач

> Створюйте ці задачі лише для HTTP- або integration-поведінки.

- [ ] Зіставити кожну змінену операцію з її сценаріями `SC-*`.
- [ ] Запланувати зрозумілий людині контракт і оновлення версійованого OpenAPI до transport-реалізації.
- [ ] Запланувати сфокусовані Red-тести перед кожною новою transport-поведінкою.
- [ ] Створити окремі задачі для contract, endpoint, HTTP/security mapping і фінального API acceptance/regression coverage, якщо їх можна рев'ювати незалежно.
- [ ] Надати кожній задачі ID `TS-*`, залежності, точні шляхи, test level і checkpoint.
- [ ] Додати кожну API-задачу до `traceability.md`.

## Шаблони

1. [OpenAPI-контракт](api/04.NN-contract-documentation.template.md)
2. [Controllers та endpoints](api/04.NN-controllers.template.md)
3. [HTTP-поведінка, authorization і Result mapping](api/04.NN-http-behavior.template.md)
4. [API acceptance і regression-тести](api/04.NN-tests.template.md)

## Checkpoint

Змінена API-поведінка є contract-first, де це практично. Кожен endpoint і HTTP rule має запланований сфокусований Red-тест, задачу реалізації й acceptance- або contract-evidence. Для feature без HTTP/integration surface у traceability matrix вказано `—`.
