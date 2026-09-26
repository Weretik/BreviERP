# Backend rules

## Шари та залежності

Дотримуйтеся напрямку залежностей `API → Application → Domain`. Infrastructure залежить від внутрішніх шарів, але не навпаки.

- Domain володіє business invariants, entities, value objects і domain services; у ньому немає transport DTO або Infrastructure dependencies.
- Application оркеструє use-cases через abstractions, а не Infrastructure implementations.
- Infrastructure містить persistence та integration adapters; не володіє business rules.
- API виконує тільки HTTP binding, авторизацію та mapping результату; не містить EF або business logic.

## Ідентифікатори сутностей

Перед створенням або зміною ідентифікатора сутності дотримуйтеся [стратегії ідентифікаторів](identifier-strategy.md). Не використовуйте `Guid` автоматично: тип ID визначається життєвим циклом створення та інтеграції. У Domain обгортайте вибраний scalar у типізований value object; не використовуйте raw primitive ID у `BaseEntity`, `BaseAuditableEntity` або Domain foreign keys.

## CQRS, Mediator і код

Використовуйте feature-by-folder усередині Application. Тримайте command/query, handler, validator і private DTO разом за сценарієм. Один Mediator handler володіє одним use-case; commands змінюють стан, queries не змінюють його.

Передавайте `CancellationToken` крізь увесь flow. Cross-cutting concerns залишайте у pipeline behaviors; не переміщуйте business rules у behaviors.

### Ardalis.Result

- Для очікуваних outcomes use case використовуйте `Ardalis.Result`, а не власні
  success/error wrappers. Для paginated list query базовим результатом є
  `PagedResult<IReadOnlyList<TItem>>` із `PagedInfo`; не створюйте локальний DTO,
  що дублює page/page-size/total metadata.
- Для чистого перетворення `Result<TSource>` у `Result<TTarget>` використовуйте
  `Map`. Для послідовності операцій, що кожна повертає `Result`, використовуйте
  `Bind`/`BindAsync`, коли це прибирає повторні перевірки status. Не приховуйте
  через них write side effects або суттєві DB/state checks.

Використовуйте явні імена: `CreateOrderCommand`, `GetPublicProductListQuery`, `GetAdminProductListQueryHandler`. Не створюйте generic folders або names на кшталт `Common`, `Misc`, `Helper`, `Utils`, `Manager`. Не додавайте abstractions без конкретної потреби.

## Якість структури файлів

- Один production-файл має одну основну відповідальність і, як правило, один top-level type. Не об'єднуйте в одному файлі кілька DTO, specifications, handlers або не пов'язані допоміжні типи лише для скорочення кількості файлів.
- Розташовуйте код у feature-by-folder: command/query, handler, validator, DTO та specification належать конкретному use-case. Specifications завжди зберігайте в `<UseCase>/Specifications/`; заборонено створювати загальну `<Area>/Specifications/` або використовувати specification іншого use-case. `Shared` допускається лише для справді спільного коду Product/feature, а не як місце для неструктурованих типів.
- Назва файла відповідає public top-level type: `CreateProductCommand.cs`, `CreateProductCommandHandler.cs`, `ProductByIdSpec.cs` тощо.
- Дотримуйтеся наявного форматування проєкту. Не використовують стиснутий однорядковий стиль для namespaces, класів, handlers, validators або складних DTO. Перед завершенням зміни запустіть formatter або перевірте diff на відповідність сусіднім файлам.
- Спочатку дослідіть наявні conventions, контракти та залежності, потім вносіть мінімальну завершену реалізацію. Заборонені тимчасові заглушки, часткові каркаси й передчасні твердження про завершення, якщо ними не можна закрити task і підтвердити це перевіркою.
- Не дублюйте capability уже підключеної бібліотеки власним wrapper/utility, доки
  не перевірено, що її тип або extension не покриває погоджений use case.

## Domain events і транзакції

Дотримуйтеся наявного lifecycle domain events: `collect → dispatch → clear`. Use-cases з кількома змінами aggregate-ів координують транзакцію через наявний `IUnitOfWork`.
