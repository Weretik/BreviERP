# API rules

## Стабільність контракту

Публічні маршрути, action names і response fields стабільні за замовчуванням. Надавайте перевагу адитивним змінам. Несумісну зміну, зачеплених клієнтів, шлях міграції та rollout plan документуйте у feature-специфікації й контракті.

Використовуйте DTO, специфічні для задачі. Не розкривайте entities, persistence types, secrets або internal exceptions. Для write-операцій із ризиком повторного виконання визначайте семантику `Idempotency-Key` до реалізації.

## Контролери

Контролери мають:

- приймати явні request models;
- передавати `CancellationToken` до Mediator;
- повертати усталений Ardalis.Result HTTP mapping;
- оголошувати релевантні `ProducesResponseType` attributes;
- не містити validation, business або EF logic.

Для paginated list endpoint controller повинен зберегти весь
`PagedResult<IReadOnlyList<TItem>>` у response: `value` і `pagedInfo`. Не
використовуйте generic Result-to-HTTP helper, якщо він повертає лише `result.Value`
і відкидає `PagedInfo`; додайте або використайте mapping, що зберігає metadata.

## Валідація, результати та помилки

FluentValidation validators реєструються скануванням Application assembly і запускаються через Mediator validation behavior. Розміщуйте validator у папці відповідного use-case.

Валідуйте форму input: required fields, ranges, formats, enum values, length і комбінації input. State/business invariants перевіряйте у Domain або Application use-case logic.

Для очікуваних failures повертайте Ardalis.Result. Зберігайте чинний API error mapping. Не використовуйте винятки для звичайного validation або not-found control flow і не розкривайте internal details у response.
