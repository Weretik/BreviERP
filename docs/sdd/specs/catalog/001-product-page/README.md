# 001 — Product page

**Модуль:** Catalog  
**Статус:** у роботі  
**Власник:** Catalog  
**Створено:** 2026-07-27

Створення та розвиток товарів двох типів: Sewing і PPE. Специфікація описує спільні дані Product, типоспецифічні правила, модель зберігання та майбутній delivery.

Спільні терміни визначені у [глосарії продукту](../../../../product/glossary.md).

## Вимоги

1. [Огляд, межі та відкриті питання](requirements/overview.md)
2. [Описові дані Product](requirements/product-content.md)
3. [Sewing: операції, тканини та фурнітура](requirements/sewing.md)
4. [PPE: постачальник і коефіцієнт](requirements/ppe.md)
5. [Ціноутворення](requirements/pricing.md)

## Технічний дизайн

1. [Domain: модель, розрахунки й інтеграція з Reference](design/domain.md)
2. [Infrastructure: persistence, EF Core і міграції](design/infrastructure.md)
3. [Поточна технічна база](design/implementation-baseline.md)
4. [Модель даних](data-model.md)

## Delivery

1. [API contract](contracts/api-contract.md)
2. [Checklist: готовність специфікації](checklist/spec-readiness.md)
3. [Checklist: готовність delivery](checklist/delivery-readiness.md)

## Задачі реалізації для AI

Кожен файл у `tasks/` — одна послідовна фаза реалізації. AI виконує тільки незавершені задачі поточної фази, позначає завершену задачу `[x]` і переходить далі лише після checkpoint. `[P]` означає, що задача може виконуватися паралельно з іншими `[P]` задачами тієї самої фази після виконання її залежностей.

| Фаза | Вхід | Результат |
| --- | --- | --- |
| [00 — Уточнення](tasks/00-readiness.md) | вимоги, відкриті питання | погоджені межі та готовий API contract |
| [01 — Domain](tasks/01-domain.md) | 00 | інваріанти й unit-тести Product |
| [02 — Infrastructure](tasks/02-infrastructure.md) | 01 | EF mapping і міграція |
| [03 — Application](tasks/03-application.md) | 01, 02 | CQRS use-cases і інтеграція з Reference |
| [04 — API](tasks/04-api.md) | 03, API contract | HTTP endpoints і integration tests |
| [05 — Verification](tasks/05-verification.md) | 00–04 | перевірений delivery і оновлена документація |

## Журнал рішень

- 2026-07-27 — Зафіксовано доменну та persistence основу; цикл лишається відкритим для розширення.
- 2026-07-27 — Прийнято 480 хвилин за зміну, `floor` для завершених виробів, чернетку без операцій і актуальну тривалість операції без snapshot.
- 2026-07-27 — Визначено Description, Information, Characteristics, тканини, фурнітуру, `MetersPerProduct`, PPE-постачальника та коефіцієнт.
- 2026-07-27 — Зафіксовано три цінові рівні Sewing і два рівні PPE; формули винесено до окремого рішення.
- 2026-07-27 — Прийнято composition persistence-модель: Products + окремі Sewing/PPE details і Sewing links.
