# Операції пошиття

**Статична звірка з кодом:** 2026-10-16. **Модуль:** reference.

CRUD-операції та читання списку для довідника «Операції пошиття».

## Будова й зв'язки

[Controller](../../../../src/Modules/Reference/Reference.Api/Reference.Api/Controllers/GarmentPartOperationsController.cs) відкриває CRUD. [create handler](../../../../src/Modules/Reference/Reference.Application/Reference.Application/Features/GarmentPartOperation/Create/CreateGarmentPartOperationCommandHandler.cs) і сусідні query/update/delete handlers перевіряють дані; [GarmentPartOperation](../../../../src/Modules/Reference/Reference.Domain/Reference.Domain/GarmentPartOperations/Entities/GarmentPartOperation.cs) є доменною сутністю. [ReferenceEfRepository](../../../../src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/Repositories/ReferenceEfRepository.cs) та [ReferenceDbContext](../../../../src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/DataBase/ReferenceDbContext.cs) зберігають її. Операція належить частині виробу та містить тривалість Min; Product зберігає посилання на вибрані операції.

## Структура в коді

Наведено перевірені точки входу та ключові складові фічі; дерево не перелічує всі допоміжні файли.

```text
src/
└── Modules/Reference/
    ├── Reference.Api/Reference.Api/Controllers/GarmentPartOperationsController.cs  # HTTP-вхід
    ├── Reference.Application/Reference.Application/Features/GarmentPartOperation/
    │   ├── Create/CreateGarmentPartOperationCommandHandler.cs  # обробник сценарію
    │   ├── Delete/DeleteGarmentPartOperationCommandHandler.cs  # обробник сценарію
    │   ├── GetList/GetGarmentPartOperationsQueryHandler.cs  # обробник сценарію
    │   └── Update/UpdateGarmentPartOperationCommandHandler.cs  # обробник сценарію
    ├── Reference.Domain/Reference.Domain/GarmentPartOperations/Entities/GarmentPartOperation.cs  # доменна модель
    └── Reference.Infrastructure/Reference.Infrastructure/
        ├── DataBase/ReferenceDbContext.cs  # контекст даних
        └── Repositories/ReferenceEfRepository.cs  # доступ до даних
```

## Схема будови

```mermaid
flowchart LR
    A["GarmentPartOperationsController"] --> B["CRUD handlers"]
    B --> C["GarmentPartOperation"]
    B --> D["Контракти репозиторіїв Reference"]
    D -.-> E["ReferenceEfRepository та ReferenceReadEfRepository"]
    E --> F["ReferenceDbContext"]
    E --> C
    C --> G["Частина виробу та тривалість Min"]
    H["Product"] --> I["ID вибраних операцій"]
    I --> C
```

## Пов'язані джерела

[OpenAPI](../../contracts/reference/garment-part-operations.openapi.yaml) · [Межі Reference](../../architecture/overview/modules.md).
