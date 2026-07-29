# <назва feature> — checklist: готовність delivery

- [ ] Domain invariants і type boundaries перевірені тестами.
- [ ] Persistence constraints, migration і rollback перевірені, якщо застосовно.
- [ ] API відповідає OpenAPI-контракту з `docs/sdd/contracts/<module>/<feature>.openapi.yaml`, включно з errors і security.
- [ ] restore/build/test виконані або failure задокументовано.
- [ ] Документація, контракт у `docs/sdd/contracts/` і код узгоджені; агрегований `openapi.yaml` посилається на контракт feature через `$ref`.
- [ ] Усі застосовні task IDs закриті; blocker має власника й наступний крок.
