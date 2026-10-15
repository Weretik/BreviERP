# Фаза 00 — міграція схем модулів

- [x] Визначити власників таблиць і підтвердити видалення застарілої `public.ProductCategories`.
- [x] Налаштувати кожний DbContext на схему його модуля.
- [x] Генерувати міграції лише через EF CLI.
- [x] Згенерувати та перевірити ідемпотентні SQL-скрипти.
- [x] Застосувати міграції до ізольованої порожньої тестової БД, перевірити схеми, rollback і повторне застосування.
- [x] Застосувати перевірені міграції до локальної БД і перевірити рядки та схеми.

## Порядок впровадження

1. Reference
2. Identity
3. Accounting
4. Catalog (уже в `catalog`)

Rollback для Reference або Identity переміщує схеми PostgreSQL назад у `public` через шлях `Down` міграції. Accounting не має наявної таблиці для переміщення; його перша перевірена міграція створює `accounting.TransactionCategories`.

## Перевірка

- `brevidb_schema_verify`: усі міграції Catalog, Reference, Identity та Accounting застосовано; rollback і повторне застосування Reference, Identity та Accounting завершилися успішно.
- `brevidb`: кількість рядків Reference до й після переміщення була однакова: AdditionalReferences 14, Fabrics 44, Accessories 37, Operations 151, Parts 34, Suppliers 27. Identity зберіг 1 користувача, 3 ролі та 0 refresh-сесій.
- Підсумкова локальна структура має таблиці модулів у `catalog`, `reference`, `identity` та `accounting`; у `public` залишається лише `__EFMigrationsHistory`.
