# Карта backend-фіч BreviERP

**Статичну звірку з кодом проведено:** 2026-10-16. Карта охоплює чинні HTTP-сценарії за модулями. Кожен опис пояснює вхід, компоненти, дані та міжмодульні зв'язки; [SDD](../specs/README.md) містить рішення й історію delivery. При новій фічі або зміні її будови оновлюй опис та індекс у тій самій зміні.

Фактичні шляхи й ролі файлів дивись у картці фічі під заголовком «Структура в коді»; потік між складовими — під заголовком «Схема будови».

## Catalog

- [Товари Catalog](catalog/products.md) — створення, читання, заміна й видалення Sewing/PPE-товарів.
- [Категорії товарів](catalog/categories.md) — Admin CRUD та список для Storefront.
- [Медіафайли каталогу](catalog/media.md) — Завантаження, список і видалення файлів для товарів.

## Reference

- [Постачальники](reference/suppliers.md) — CRUD і читання списку.
- [Тканини](reference/fabrics.md) — CRUD і читання списку.
- [Фурнітура](reference/garment-accessories.md) — CRUD і читання списку.
- [Частини виробу](reference/garment-parts.md) — CRUD і читання списку.
- [Операції пошиття](reference/garment-part-operations.md) — CRUD і читання списку.
- [Додаткові довідники](reference/additional-references.md) — CRUD і читання списку.

## Identity

- [Сеанс користувача](identity/session.md) — Login, refresh, logout і me.

## Інші модулі

[Accounting і Crm](../architecture/overview/modules.md) присутні у структурі; на дату звірки окремих HTTP-сценаріїв для них у контролерах немає. Їх не позначено готовими фічами. Підготовку схем і seed дивись у [операціях](../operations/data/migrations-and-seeding.md).
