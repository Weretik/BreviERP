# Identifier strategy

Read this document before creating an entity, changing a primary key, or exposing an identifier through an API or integration. An identifier's type depends on the entity lifecycle—where it is created, stored, transferred, and identified—not on a convenient C# default.

## Decision table

| Situation | Typical choice |
| --- | --- |
| Small, stable reference catalog | `int` |
| Ordinary business entity | `long` |
| Very large relational table | `long` |
| One central database | Usually `long` |
| Multiple independent writers or services | `Guid` |
| Offline creation | `Guid` |
| Merging independent databases | `Guid` |
| Event or message identifier | `Guid` |
| Identifier in a public API | `Guid` or a separate `PublicId` |
| Relational model with many FKs | Usually `long` |
| Join table | Often a composite key |
| Human-readable document number | A separate immutable `string`, never the primary key |

## Decision algorithm

```text
Choose an ID
│
├─ Is it generated in multiple independent places, offline, or when databases merge?
│  └─ Yes: Guid
│
└─ No: is there one central relational database?
   └─ Yes: long
      └─ Is this a small, stable reference catalog? int

Need to hide the internal database ID in a public API, URL, or integration?
└─ Add a separate Guid PublicId; keep the relational PK as long.
```

## BreviERP convention

In Domain, every new or changed entity ID and domain foreign key must be a typed value object over the selected scalar type. BreviERP has both explicit `readonly record struct` types in Catalog and Vogen value objects in Accounting and Reference. Follow the module's rules and adjacent IDs; do not add or remove Vogen as part of an unrelated feature.

```csharp
// Catalog business entity: an explicit typed ID over the selected scalar.
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

// Domain foreign keys use the same typed ID.
public ProductId ProductId { get; private set; }

// A module already using Vogen keeps that convention.
[ValueObject<int>]
public readonly partial struct WalletId;

// Events, messages, and independently created objects use typed Guid IDs.
[ValueObject<Guid>]
public readonly partial struct IntegrationEventId;

// Separate public identifier when the internal ID must not be exposed.
Guid PublicId;

// Human-readable document number.
string DocumentNumber; // unique, immutable, not the PK
```

Do not add `BaseAuditableEntity<long>`, `BaseEntity<Guid>`, or a primitive scalar foreign key in Domain. Such scalar IDs are allowed only in Infrastructure records used solely for persistence and not passed across the Domain boundary, such as an idempotency table. With EF Core, map typed domain IDs to scalar columns using the project's current converters and rules.

Do not migrate existing legacy models with primitive IDs as part of an unrelated feature. Make such a change only as a separate, explicitly agreed migration with analysis of APIs, FKs, and data.

A human-readable number is for people and integrations. It may use a format such as `DOC-YYYYMMDD-NNNN`, while the database uses an efficient numeric primary key.

## Review checklist

Before approving a new entity, answer:

1. Where is it created—in one database or multiple independent systems?
2. Is offline creation or later merging of databases required?
3. Is it a business record with many FKs or a small reference catalog?
4. Must the internal identifier be hidden from public clients?
5. Is a separate human-readable document number needed?
6. Is the selected scalar wrapped in a typed domain ID, and are domain foreign keys typed?

Do not use `Guid` for every entity merely because it can be generated in Application code.
