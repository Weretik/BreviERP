# Фурнітура

**Статична звірка з кодом:** 2026-10-16. **Модуль:** reference.

CRUD-операції та читання списку для довідника «Фурнітура».

## Будова й зв'язки

[Controller](../../../../src/Modules/Reference/Reference.Api/Reference.Api/Controllers/GarmentAccessoriesController.cs) відкриває CRUD. [create handler](../../../../src/Modules/Reference/Reference.Application/Reference.Application/Features/GarmentAccessory/Create/CreateGarmentAccessoryCommandHandler.cs) і сусідні query/update/delete handlers перевіряють дані; [GarmentAccessory](../../../../src/Modules/Reference/Reference.Domain/Reference.Domain/GarmentAccessories/Entities/GarmentAccessory.cs) є доменною сутністю. [ReferenceEfRepository](../../../../src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/Repositories/ReferenceEfRepository.cs) та [ReferenceDbContext](../../../../src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/DataBase/ReferenceDbContext.cs) зберігають її. Фурнітура має постачальника; Sewing-товар посилається на її ID та власну кількість.

## Структура в коді

Наведено перевірені точки входу та ключові складові фічі; дерево не перелічує всі допоміжні файли.

```text
src/
└── Modules/Reference/
    ├── Reference.Api/Reference.Api/Controllers/GarmentAccessoriesController.cs  # HTTP-вхід
    ├── Reference.Application/Reference.Application/Features/GarmentAccessory/
    │   ├── Create/CreateGarmentAccessoryCommandHandler.cs  # обробник сценарію
    │   ├── Delete/DeleteGarmentAccessoryCommandHandler.cs  # обробник сценарію
    │   ├── GetList/GetGarmentAccessoriesQueryHandler.cs  # обробник сценарію
    │   └── Update/UpdateGarmentAccessoryCommandHandler.cs  # обробник сценарію
    ├── Reference.Domain/Reference.Domain/GarmentAccessories/Entities/GarmentAccessory.cs  # доменна модель
    └── Reference.Infrastructure/Reference.Infrastructure/
        ├── DataBase/ReferenceDbContext.cs  # контекст даних
        └── Repositories/ReferenceEfRepository.cs  # доступ до даних
```

## Схема будови

```mermaid
flowchart LR
    A["GarmentAccessoriesController"] --> B["CRUD handlers"]
    B --> C["GarmentAccessory"]
    B --> D["Контракти репозиторіїв Reference"]
    D -.-> E["ReferenceEfRepository та ReferenceReadEfRepository"]
    E --> F["ReferenceDbContext"]
    E --> C
    C --> G["Постачальник"]
    H["Sewing товар"] --> I["ID фурнітури та кількість"]
    I --> C
```

## Пов'язані джерела

[OpenAPI](../../contracts/reference/garment-accessories.openapi.yaml) · [Межі Reference](../../architecture/overview/modules.md).
