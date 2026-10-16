# Специфікації функціональностей

## Наявні специфікації

Це індекс проєктних рішень і delivery; [карта фактично реалізованих фіч](../features/README.md) веде до архітектури поточного коду. Статус кожної роботи перевіряй у її README та коді.

| Модуль | Специфікації |
| --- | --- |
| Catalog | [товар і сторінка товару — чинна SDD](catalog/001-product-page/README.md), [ранній історичний знімок товару](catalog/product-page/README.md), [admin список категорій](catalog/admin-product-list/README.md), [медіафайли](catalog/media-files/README.md) |
| Platform | [схеми модулів](platform/001-module-schemas/README.md) |

Кожна нетривіальна зміна починається тут до початку реалізації.

Для нової або зміненої специфікації застосовуйте `$backend-feature-spec` і [правила постачання](../standards/delivery-rules.md). Структура нижче визначає місце проєктних специфікацій BreviERP; конкретні документи додавайте за змістом feature.

Для спільних бізнес-термінів Product, Sewing і PPE використовуйте [глосарій](../../product/glossary.md), а не створюйте нові визначення у кожній специфікації.

## Структура

```text
docs/sdd/specs/
  <module>/
    <NNN>-<feature-slug>/
      README.md                    orchestration feature
      requirements/                поведінка й правила
      design/                      backend-рішення
      data-model.md                модель даних
      contracts/                   API/integration contracts
      traceability.md              зв'язок сценаріїв, задач і доказів
      tasks/                       фази та атомарні задачі AI
      checklist/                   quality gates
```

Використовуйте папку модуля, наприклад `catalog`, `reference`, `identity`, `accounting`, `crm`, `platform` або `cross-module`. Назви функціональностей і фаз мають бути в нижньому регістрі та kebab-case.

Наявні специфікації переводьте на цю структуру поступово: зберігайте ID завершених задач і verification history, якщо користувач не замовив повне перепроєктування feature. Для міграції даних або schema явно описуйте baseline і rollback у `design/`, а rollout та verification — у `tasks/`. Машиночитані публічні API-контракти зберігайте в [`docs/sdd/contracts/`](../contracts/README.md). Посилайтеся на архітектуру й стандарти замість їх копіювання.

## Життєвий цикл

Чернетка → прийнято → у роботі → перевірено → завершено.

Реалізація не повинна непомітно відхилятися від прийнятої специфікації. Якщо це відбувається, додайте датований запис про зміну з причиною, впливом на межі та новою перевіркою.
