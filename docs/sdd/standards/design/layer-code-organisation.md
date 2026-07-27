# Організація шарів і коду

Використовуйте feature-by-folder усередині Application. Тримайте command/query, handler, validator і private DTO разом за сценарієм. Handler володіє одним сценарієм.

Розміщуйте business invariants у Domain; orchestration в Application; adapters в Infrastructure; HTTP binding і Result mapping в API. Ніколи не впроваджуйте Infrastructure implementations в Application.

Використовуйте явні names: `CreateOrderCommand`, `GetPublicProductListQuery`, `GetAdminProductListQueryHandler`. Уникайте generic folders і names, таких як `Common`, `Misc`, `Helper`, `Utils` або `Manager`.

Розділяйте код за незалежною відповідальністю, а не лише за розміром файлу. Тримайте цілісний малий код разом; не створюйте abstractions лише заради формальності.
