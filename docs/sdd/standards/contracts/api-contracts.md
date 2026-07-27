# API та контракти

Публічні маршрути, action names і response fields стабільні за замовчуванням. Надавайте перевагу адитивним змінам. Несумісну зміну контракту, зачеплених клієнтів, шлях міграції та rollout plan документуйте у специфікації функціональності.

Контролери мають:

- приймати явні request models;
- передавати `CancellationToken` до Mediator;
- повертати усталений Ardalis.Result HTTP mapping;
- оголошувати релевантні `ProducesResponseType` attributes;
- не містити validation/business/EF logic.

Використовуйте DTO, специфічні для задачі. Не розкривайте entities, persistence types, secrets або internal exceptions. Для write-операції, чутливої до повтору, визначте семантику `Idempotency-Key` до реалізації.
