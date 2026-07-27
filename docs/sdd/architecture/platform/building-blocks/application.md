# BuildingBlocks.Application

## Структура

```text
BuildingBlocks.Application/
├── Behaviors/
│   ├── RequestLoggingBehavior
│   ├── ExceptionBehavior
│   ├── PerformanceBehavior + options
│   ├── ValidationBehavior
│   └── DomainEventDispatcherBehavior
├── Contracts/
│   ├── ICurrentUserService, IPermissionService
│   └── module-facing technical abstractions
├── Notifications/
│   ├── IDomainEventContext, IDomainEventDispatcher
│   └── DomainEventNotification + handler
├── Logging/                  стабільні structured log helpers
├── Helpers/PhoneNumberHelper.cs
└── ApplicationAssemblyMarker.cs
```

## Behaviors

Хост реєструє RequestLogging, Performance, Validation, Exception і DomainEventDispatcher саме в такому порядку. Точну семантику описано в [Mediator behaviors](../../runtime/mediator-behaviors.md).

## Notifications

`IDomainEventContext` відкриває сутності з накопиченими events. `IDomainEventDispatcher` публікує events. `DomainEventNotification` є Mediator notification wrapper. Default notification handler навмисно порожній; module-specific handlers підписуються на notification/event pattern, де це потрібно.

## Підхід до logging і helpers

Папка Logging містить стабільні structured templates для requests, validation, performance, domain events, cancellation і unhandled failures. Використовуйте їх через наявні behaviors; не логуйте raw sensitive request data.

`PhoneNumberHelper` — спільна утиліта normalisation/validation номера телефону. Повторно використовуйте її замість дублювання phone parsing.

## Доступність контрактів

`ICurrentUserService` та `IPermissionService` мають shared Infrastructure implementations. Не впроваджуйте application abstraction у нову функціональність, доки module owner не надасть і не зареєструє її реалізацію.
