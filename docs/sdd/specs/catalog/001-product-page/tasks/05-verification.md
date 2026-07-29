# Фаза 05 — Verification і handoff

**Залежності:** [00-readiness.md](00-readiness.md)–[04-api.md](04-api.md)

- [ ] T034 Виконати `dotnet restore BreviERP.sln`.
- [ ] T035 Виконати `dotnet build BreviERP.sln --no-restore`.
- [ ] T036 Виконати `dotnet test BreviERP.sln --no-build`.
- [ ] T037 Виконати міграційний тест і ручні Swagger-сценарії з create/update Sewing, create/update PPE, невалідними комбінаціями і повторною подією видалення.
- [ ] T038 Пройти кожен застосовний пункт `checklist/delivery-readiness.md`; для пропущеного пункту зафіксувати причину й ризик.
- [ ] T039 Оновити `README.md`, `contracts/api-contract.md`, `contracts/product-catalog.openapi.yaml`, обидва документи `checklist/` і відповідні requirement/design документи за фактичною реалізацією; не приховувати відхилення від спеки.
- [ ] T040 Підготувати delivery report: змінені файли, результати restore/build/test, ручні перевірки й залишкові ризики.

## Checkpoint

Feature завершена лише якщо всі застосовні T001–T040 мають `[x]`, а verification не містить невирішених блокерів.

