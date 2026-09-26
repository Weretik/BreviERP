# <назва feature> — checklist: готовність специфікації

- [ ] Scope і спостережувана поведінка повні та не суперечать одне одному.
- [ ] Кожне змінене бізнес-правило має сталий ID `R-*`.
- [ ] Кожна поведінка в scope має сталий scenario ID `SC-*`.
- [ ] Кожне правило має happy-path і релевантні негативні або граничні приклади.
- [ ] Given/When/Then не містять деталей реалізації.
- [ ] Кожен `[NEEDS CLARIFICATION]` закрито або винесено за scope.
- [ ] Кожна вимога має design-рішення або статус «відкладено».
- [ ] data model не містить speculative data/relations.
- [ ] Для кожної нової сутності, таблиці, event/message або integration contract обрано ID за [`identifier-strategy.md`](../../../../standards/identifier-strategy.md), а рішення й обґрунтування записані в `data-model.md`.
- [ ] Contract, security, idempotency та integration dependencies погоджені.
- [ ] Для зміненої HTTP-поведінки зрозумілий людині контракт і шлях до machine-readable OpenAPI визначені до transport-реалізації.
- [ ] `traceability.md` зіставляє кожен сценарій із рівнем тестування та задачами `TS-*`/`EN-*`; tasks не виходять за scope.
- [ ] Кожна задача має точні шляхи, залежності, checkpoint і заплановані докази Red/Green/regression або обґрунтований test-first виняток.
