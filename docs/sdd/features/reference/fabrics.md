# Тканини

**Статична звірка з кодом:** 2026-10-16. **Модуль:** reference.

CRUD-операції та читання списку для довідника «Тканини».

## Будова й зв'язки

[Controller](../../../../src/Modules/Reference/Reference.Api/Reference.Api/Controllers/FabricsController.cs) відкриває CRUD. [create handler](../../../../src/Modules/Reference/Reference.Application/Reference.Application/Features/Fabric/Create/CreateFabricCommandHandler.cs) і сусідні query/update/delete handlers перевіряють дані; [Fabric](../../../../src/Modules/Reference/Reference.Domain/Reference.Domain/GarmentAccessories/Entities/Fabric.cs) є доменною сутністю. [ReferenceEfRepository](../../../../src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/Repositories/ReferenceEfRepository.cs) та [ReferenceDbContext](../../../../src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/DataBase/ReferenceDbContext.cs) зберігають її. Тканини використовуються у складі Sewing-товарів.

## Структура в коді

Наведено перевірені точки входу та ключові складові фічі; дерево не перелічує всі допоміжні файли.

```text
src/
└── Modules/Reference/
    ├── Reference.Api/Reference.Api/Controllers/FabricsController.cs  # HTTP-вхід
    ├── Reference.Application/Reference.Application/Features/Fabric/
    │   ├── Create/CreateFabricCommandHandler.cs  # обробник сценарію
    │   ├── Delete/DeleteFabricCommandHandler.cs  # обробник сценарію
    │   ├── GetList/GetAllFabricQueryHandler.cs  # обробник сценарію
    │   └── Update/UpdateFabricCommandHandler.cs  # обробник сценарію
    ├── Reference.Domain/Reference.Domain/GarmentAccessories/Entities/Fabric.cs  # доменна модель
    └── Reference.Infrastructure/Reference.Infrastructure/
        ├── DataBase/ReferenceDbContext.cs  # контекст даних
        └── Repositories/ReferenceEfRepository.cs  # доступ до даних
```

## Схема будови

```mermaid
flowchart LR
    A["FabricsController"] --> B["CRUD handlers"]
    B --> C["Fabric"]
    B --> D["Контракти репозиторіїв Reference"]
    D -.-> E["ReferenceEfRepository та ReferenceReadEfRepository"]
    E --> F["ReferenceDbContext"]
    E --> C
    G["Sewing товар"] --> C
```

## Пов'язані джерела

[OpenAPI](../../contracts/reference/fabrics.openapi.yaml) · [Межі Reference](../../architecture/overview/modules.md).
