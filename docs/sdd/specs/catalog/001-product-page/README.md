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

Повний стан і навігація: [tasks/README.md](tasks/README.md). Головні фази `00–05` лише створюють і впорядковують підфази. AI виконує один точний файл підфази `NN.N-*.md` за раз, позначає завершену задачу `[x]` і не починає інший файл без явної команди. `[P]` означає, що задача може виконуватися паралельно з іншими `[P]` задачами тієї самої підфази після виконання її залежностей.

| Фаза | Вхід | Результат |
| --- | --- | --- |
| [00 — Readiness](tasks/00-readiness.md) | вимоги, відкриті питання | створені й завершені `00.N` scope/tooling підфази |
| [01 — Domain](tasks/01-domain.md) | 00 | створені й завершені `01.N` Product Domain підфази |
| [02 — Infrastructure](tasks/02-infrastructure.md) | 01 | створені й завершені `02.N` persistence/migration підфази |
| [03 — Application](tasks/03-application.md) | 01, 02 | `03.N` CQRS slices, зокрема збагачені list/detail Media URL і ціни для admin frontend |
| [04 — API](tasks/04-api.md) | 03 | `04.N` controllers, HTTP behavior, OpenAPI та API tests — ще відкриті |
| [05 — Verification](tasks/05-verification.md) | 00–04 | `05.N` build/tests і delivery evidence — ще відкриті |

## Журнал рішень

- 2026-07-27 — Зафіксовано доменну та persistence основу; цикл лишається відкритим для розширення.
- 2026-08-07 — Для розрахунку за 480 хвилин `piecesPerShift` передається повним дробовим значенням без округлення; окремий показник завершених виробів не формується. Чернетка без операцій і актуальна тривалість операції без snapshot лишаються чинними.
- 2026-08-10 — Для Sewing цінові рівні 1–10, 11–39 і 40+ шт. беруться відповідно з `AdditionalReference` ключів `profit_10`, `profit_10_40` і `profit_40`; ключі `coefficient_ciz_*` належать СІЗ і не застосовуються до Sewing.
- 2026-07-27 — Визначено Description, Information, Characteristics, тканини, фурнітуру, `MetersPerProduct`, PPE-постачальника та коефіцієнт.
- 2026-07-27 — Зафіксовано три цінові рівні Sewing і два рівні PPE; формули винесено до окремого рішення.
- 2026-07-27 — Прийнято composition persistence-модель: Products + окремі Sewing/PPE details і Sewing links.
- 2026-08-19 — Змінено admin frontend read contract: list передає тільки головне фото з URL та `minimumWholesalePrice`; detail передає URL усіх фото й усі ціни. Додано коригувальну підфазу `03.8` і залежні API/OpenAPI/test tasks.
