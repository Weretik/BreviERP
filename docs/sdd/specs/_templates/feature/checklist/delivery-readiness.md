# <назва feature> — checklist: готовність delivery

- [ ] Кожен рядок `SC-*` у scope в `traceability.md` має статус verified або явно deferred.
- [ ] Domain invariants і type boundaries перевірені тестами, де застосовно.
- [ ] Application orchestration, validation і result-поведінку перевірено тестами.
- [ ] Persistence constraints, migration, rollback та integrations перевірені, якщо застосовно.
- [ ] API відповідає OpenAPI-контракту з `docs/sdd/contracts/<module>/<feature>.openapi.yaml`, включно з errors і security.
- [ ] Кожна задача `TS-*`, що реалізує поведінку, містить коректні докази Red, Green, refactor і regression.
- [ ] Кожен виняток `EN-*` містить причину й замінну verification.
- [ ] restore/build/test виконані або точний failure і його owner задокументовані.
- [ ] Документація, контракт у `docs/sdd/contracts/` і код узгоджені; агрегований `openapi.yaml` посилається на контракт feature через `$ref`.
- [ ] Усі застосовні task IDs закриті; blocker має власника й наступний крок.
