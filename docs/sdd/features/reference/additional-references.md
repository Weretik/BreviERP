# Додаткові довідники

**Статична звірка з кодом:** 2026-10-16. **Модуль:** reference.

CRUD-операції та читання списку для довідника «Додаткові довідники».

## Будова й зв'язки

[Controller](../../../../src/Modules/Reference/Reference.Api/Reference.Api/Controllers/AdditionalReferencesController.cs) відкриває CRUD. [create handler](../../../../src/Modules/Reference/Reference.Application/Reference.Application/Features/AdditionalReference/Create/CreateAdditionalReferenceCommandHandler.cs) і сусідні query/update/delete handlers перевіряють дані; [AdditionalReference](../../../../src/Modules/Reference/Reference.Domain/Reference.Domain/AdditionalReferences/Entities/AdditionalReference.cs) є доменною сутністю. [ReferenceEfRepository](../../../../src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/Repositories/ReferenceEfRepository.cs) та [ReferenceDbContext](../../../../src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/DataBase/ReferenceDbContext.cs) зберігають її. Значення AdditionalReference читаються під час обчислень цін товару через Reference reader.

## Структура в коді

Наведено перевірені точки входу та ключові складові фічі; дерево не перелічує всі допоміжні файли.

```text
src/
└── Modules/Reference/
    ├── Reference.Api/Reference.Api/Controllers/AdditionalReferencesController.cs  # HTTP-вхід
    ├── Reference.Application/Reference.Application/Features/AdditionalReference/
    │   ├── Create/CreateAdditionalReferenceCommandHandler.cs  # обробник сценарію
    │   ├── Delete/DeleteAdditionalReferenceCommandHandler.cs  # обробник сценарію
    │   ├── GetList/GetAdditionalReferenceQueryHandler.cs  # обробник сценарію
    │   └── Update/UpdateAdditionalReferenceCommandHandler.cs  # обробник сценарію
    ├── Reference.Domain/Reference.Domain/AdditionalReferences/Entities/AdditionalReference.cs  # доменна модель
    └── Reference.Infrastructure/Reference.Infrastructure/
        ├── DataBase/ReferenceDbContext.cs  # контекст даних
        └── Repositories/ReferenceEfRepository.cs  # доступ до даних
```

## Схема будови

```mermaid
flowchart LR
    A["AdditionalReferencesController"] --> B["CRUD handlers"]
    B --> C["AdditionalReference"]
    B --> D["Контракти репозиторіїв Reference"]
    D -.-> E["ReferenceEfRepository та ReferenceReadEfRepository"]
    E --> F["ReferenceDbContext"]
    E --> C
    G["Обчислення цін товару"] --> H["Reference reader"]
    H --> C
```

## Пов'язані джерела

[OpenAPI](../../contracts/reference/additional-references.openapi.yaml) · [Межі Reference](../../architecture/overview/modules.md).
