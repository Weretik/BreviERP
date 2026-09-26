# Стратегія ідентифікаторів

Прочитайте цей документ перед створенням сутності, зміною primary key або публікацією ідентифікатора через API чи інтеграцію. Тип ідентифікатора визначається життєвим циклом сутності — де вона створюється, зберігається, передається та ідентифікується — а не зручним значенням C# за замовчуванням.

## Таблиця рішень

| Ситуація | Типовий вибір |
| --- | --- |
| Малий стабільний довідник | `int` |
| Звичайна бізнес-сутність | `long` |
| Дуже велика реляційна таблиця | `long` |
| Одна центральна база даних | зазвичай `long` |
| Кілька незалежних writers або services | `Guid` |
| Offline-створення | `Guid` |
| Об'єднання незалежних баз даних | `Guid` |
| Ідентифікатор event або message | `Guid` |
| Ідентифікатор у public API | `Guid` або окремий `PublicId` |
| Реляційна модель із великою кількістю FK | зазвичай `long` |
| Join table | часто складений ключ |
| Людинозрозумілий номер документа | окремий незмінний `string`, ніколи не primary key |

## Алгоритм рішення

```text
Потрібно обрати ID
│
├─ Він генерується в кількох незалежних місцях, offline або під час об'єднання БД?
│  └─ Так: Guid
│
└─ Ні: використовується одна центральна реляційна БД?
   └─ Так: long
      └─ Це малий стабільний довідник? int

Потрібно приховати внутрішній database ID у public API, URL або інтеграції?
└─ Додайте окремий Guid PublicId; реляційний PK залиште типу long.
```

## Конвенція BreviERP

У Domain кожен новий або змінений ID сутності та Domain foreign key має бути strongly typed value object над вибраним scalar. У BreviERP співіснують explicit `readonly record struct` у Catalog і Vogen value objects в Accounting та Reference. Дотримуйтеся конвенції модуля й сусідніх ID; не додавайте або не вилучайте Vogen у межах сторонньої feature.

```csharp
// Бізнес-сутність Catalog: явний типізований ID над вибраним scalar.
public readonly record struct ProductId
{
    public int Value { get; }

    public static ProductId Create(int value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value));

        return new ProductId(value);
    }

    private ProductId(int value) => Value = value;
}

public sealed class Product : BaseAuditableEntity<ProductId>;

// Domain foreign key використовує той самий типізований ID.
public ProductId ProductId { get; private set; }

// Модуль, який уже використовує Vogen, зберігає цю конвенцію.
[ValueObject<int>]
public readonly partial struct WalletId;

// Events, messages і незалежно створювані об'єкти — типізований Guid ID.
[ValueObject<Guid>]
public readonly partial struct IntegrationEventId;

// Окремий публічний ідентифікатор, коли внутрішній ID не можна розкривати.
Guid PublicId;

// Людинозрозумілий номер документа.
string DocumentNumber; // unique, immutable, не PK
```

Не додавайте `BaseAuditableEntity<long>`, `BaseEntity<Guid>` або raw scalar FK у Domain. Raw scalar IDs дозволені лише в Infrastructure persistence-only record, який не перетинає межу Domain, наприклад у таблиці idempotency. Для EF Core зіставляйте типізовані Domain IDs з їхніми scalar-колонками через чинні converters і conventions проєкту.

Існуючі legacy-моделі з primitive IDs не мігруйте в межах сторонньої feature. Таку зміну виконуйте лише як окрему явно погоджену міграцію з аналізом API, FK та даних.

Людинозрозумілий номер призначений для людей та інтеграцій. Він може мати формат на кшталт `DOC-YYYYMMDD-NNNN`, тоді як база даних використовує ефективний числовий primary key.

## Checklist для review

Перед погодженням нової сутності дайте відповіді:

1. Де вона створюється — в одній базі чи в кількох незалежних системах?
2. Чи потрібне offline-створення або подальше об'єднання баз?
3. Це бізнес-запис із багатьма FK чи малий довідник?
4. Чи потрібно приховати внутрішню identity від public clients?
5. Чи потрібен окремий людинозрозумілий номер документа?
6. Чи обгорнуто вибраний scalar у типізований Domain ID і чи типізовано Domain foreign keys?

Не використовуйте `Guid` для кожної сутності лише тому, що його можна згенерувати в application code.
