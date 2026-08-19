# <назва feature> — checklist: готовність специфікації

- [ ] Вимоги повні, тестовані та не суперечать одна одній.
- [ ] Кожен `[NEEDS CLARIFICATION]` закрито або винесено за scope.
- [ ] Кожна вимога має design-рішення або статус «відкладено».
- [ ] data model не містить speculative data/relations.
- [ ] Для кожної нової сутності, таблиці, event/message або integration contract обрано ID за [`database-rules.md`](../../../../standards/database-rules.md#вибір-ідентифікатора-сутності), а рішення й обґрунтування записані в `data-model.md`.
- [ ] Кожна вимога покрита task ID; tasks не виходять за scope.
- [ ] Contract, security, idempotency та integration dependencies погоджені; для HTTP-контракту визначено шлях `docs/sdd/contracts/<module>/<feature>.openapi.yaml`.
