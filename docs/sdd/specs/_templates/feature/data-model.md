# <назва feature> — data model

## Контекст

<Які дані потрібні.>

## Модель

```text
<Aggregate/Table>
├── <поле>
└── <зв’язок>
```

## Ідентифікатори

Для кожної нової сутності, таблиці, event/message або integration contract заповніть
цей розділ **до** створення коду. Вибір має відповідати
[`identifier-strategy.md`](../../../standards/identifier-strategy.md).

| Об'єкт | ID / ключ | Де генерується | Чи потрібен `PublicId` | Обґрунтування |
| --- | --- | --- | --- | --- |
| `<Entity>` | `<typed int | typed long | typed Guid | composite>` | `<central DB | service | client>` | `<так | ні>` | `<рішення за життєвим циклом>` |

## Інваріанти та цілісність

- <domain invariant>
- <database constraint>
