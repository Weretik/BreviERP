# Фаза 00 — Уточнення та готовність до реалізації

**Залежності:** немає  
**Блокує:** усі наступні фази

- [ ] T001 Прочитати всі документи `requirements/`, `design/`, `data-model.md` і `checklist/spec-readiness.md`; створити перелік суперечностей та невизначеностей без припущень.
- [ ] T002 Погодити або виключити з цього delivery: локалізацію Information/Characteristics, Markdown allow-list, ціноутворення, кілька категорій, історію виробничих норм і правила media.
- [ ] T003 Погодити write-авторизацію, slug uniqueness, pagination/filtering і public read-model; зафіксувати рішення в `contracts/api-contract.md` та відповідних requirement-файлах.
- [ ] T004 Погодити контракт, ідемпотентність, retry та observability подій видалення `GarmentPartOperation`, `Fabric` і `GarmentAccessory`; оновити `design/domain.md`.
- [ ] T005 Описати в `contracts/product-catalog.openapi.yaml` точні маршрути, request/response DTO, помилки та приклади до створення API-коду.
- [ ] T006 Позначити виконані пункти у `checklist/spec-readiness.md`.

## Checkpoint

Не починати Domain, доки кожне рішення, яке впливає на модель або контракт, не має явного статусу «прийнято» або «поза scope».

