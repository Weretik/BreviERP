# Довідка BuildingBlocks

BuildingBlocks — спільна технічна основа. Він визначає примітиви для кількох модулів і реалізації, але ніколи не володіє бізнес-правилами Catalog, Reference, Identity, Accounting або Crm.

## Карта проєктів

```text
src/BuildingBlocks/
├── BuildingBlocks.Domain/            примітиви сутностей, аудиту, помилок і подій
├── BuildingBlocks.Application/       Mediator behaviors, контракти, notifications і журналювання
├── BuildingBlocks.Infrastructure/    спільні adapter implementations, migrations і початкове наповнення
└── BuildingBlocks.Api/               Ardalis.Result → MVC mapping
```

## Читайте в такому порядку

1. [Примітиви Domain](domain.md)
2. [Примітиви й поведінки Application](application.md)
3. [Сервіси Infrastructure і запуск](infrastructure.md)
4. [Відображення результату API](api.md)
5. [Правила використання та доступність](usage-rules.md)

Не створюйте інший спільний базовий тип, поведінку конвеєра або сервісну абстракцію до перевірки цієї довідки.
