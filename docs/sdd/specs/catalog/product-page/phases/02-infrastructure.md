# Фаза 02: Persistence товару за типом

**Статус:** у проєктуванні  
**Залежить від:** [фази Domain](01-domain.md)  
**Блокує:** Application-сценарії створення та оновлення товару

## Результат

Спільні дані Product зберігаються в `catalog.Products`, а типові дані — у таблицях деталей за composition-моделлю без nullable-полів іншого типу товару.

## Цільові таблиці

| Таблиця | Власник | Призначення |
| --- | --- | --- |
| `Products` | Catalog | спільні властивості та discriminator `Type` |
| `ProductPhotos` | Catalog | наявна owned collection фото |
| `ProductCategoryLinks` | Catalog | наявна owned collection категорій |
| `SewingProductDetails` | Catalog | `MetersPerProduct`, один запис на Sewing товар |
| `SewingProductFabrics` | Catalog | тканини, основна ознака, порядок |
| `SewingProductAccessories` | Catalog | фурнітура, кількість, порядок |
| `SewingProductOperations` | Catalog | посилання на операції трудомісткості |
| `PpeProductDetails` | Catalog | постачальник і режим/значення коефіцієнта |

## EF Core правила

- `SewingProductDetails` і `PpeProductDetails` мапляться як one-to-one detail record із `ProductId` одночасно PK/FK.
- Sewing collections мають composite unique keys: `(ProductId, FabricId)`, `(ProductId, GarmentAccessoryId)`, `(ProductId, GarmentPartOperationId)`.
- Reference IDs зберігаються як типізовані value objects/скаляри. Між module DbContext-ами не додаються EF navigation properties до Reference entities.
- Посилання на Reference не отримують фізичний FK cascade. Видалення Reference data синхронізується ідемпотентними подіями.
- Detail table не може замінити Domain/Application перевірку відповідності `Product.Type`.

## Міграція

Потрібна нова погоджена EF migration після реалізації Domain-моделі та EF configurations. Не редагувати застосовані міграції. Rollback має видаляти лише нові таблиці до моменту появи production-даних.

## Критерії приймання

- [ ] У схемі немає Sewing-specific колонок у `PpeProductDetails` та Ppe-specific колонок у `SewingProductDetails`.
- [ ] Один Product не може мати два detail records одного типу.
- [ ] Унікальність link-таблиць підтримується БД.
- [ ] Міграція застосовується на порожній БД і не змінює наявні таблиці не за scope.
- [ ] Domain/Application не залежить від Infrastructure або Reference EF entities.

## Перевірка

- Integration-тести EF mapping, unique constraints і lifecycle aggregate-а.
- Migration test на порожній test database.
- Ручна перевірка схеми та rollback до rollout.
