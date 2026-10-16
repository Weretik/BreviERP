# Частини виробу

**Статична звірка з кодом:** 2026-10-16. **Модуль:** reference.

CRUD-операції та читання списку для довідника «Частини виробу».

## Будова й зв'язки

[Controller](../../../../src/Modules/Reference/Reference.Api/Reference.Api/Controllers/GarmentPartsController.cs) відкриває CRUD. [create handler](../../../../src/Modules/Reference/Reference.Application/Reference.Application/Features/GarmentPart/Create/CreateGarmentPartCommandHandler.cs) і сусідні query/update/delete handlers перевіряють дані; [GarmentPart](../../../../src/Modules/Reference/Reference.Domain/Reference.Domain/GarmentPartOperations/Entities/GarmentPart.cs) є доменною сутністю. [ReferenceEfRepository](../../../../src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/Repositories/ReferenceEfRepository.cs) та [ReferenceDbContext](../../../../src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/DataBase/ReferenceDbContext.cs) зберігають її. Частина виробу групує операції пошиття.

## Структура в коді

Наведено перевірені точки входу та ключові складові фічі; дерево не перелічує всі допоміжні файли.

```text
src/
└── Modules/Reference/
    ├── Reference.Api/Reference.Api/Controllers/GarmentPartsController.cs  # HTTP-вхід
    ├── Reference.Application/Reference.Application/Features/GarmentPart/
    │   ├── Create/CreateGarmentPartCommandHandler.cs  # обробник сценарію
    │   ├── Delete/DeleteGarmentPartCommandHandler.cs  # обробник сценарію
    │   ├── GetList/GetGarmentPartsQueryHandler.cs  # обробник сценарію
    │   └── Update/UpdateGarmentPartCommandHandler.cs  # обробник сценарію
    ├── Reference.Domain/Reference.Domain/GarmentPartOperations/Entities/GarmentPart.cs  # доменна модель
    └── Reference.Infrastructure/Reference.Infrastructure/
        ├── DataBase/ReferenceDbContext.cs  # контекст даних
        └── Repositories/ReferenceEfRepository.cs  # доступ до даних
```

## Схема будови

```mermaid
flowchart LR
    A["GarmentPartsController"] --> B["CRUD handlers"]
    B --> C["GarmentPart"]
    B --> D["Контракти репозиторіїв Reference"]
    D -.-> E["ReferenceEfRepository та ReferenceReadEfRepository"]
    E --> F["ReferenceDbContext"]
    E --> C
    C --> G["Операції пошиття"]
```

## Пов'язані джерела

[OpenAPI](../../contracts/reference/garment-parts.openapi.yaml) · [Межі Reference](../../architecture/overview/modules.md).
