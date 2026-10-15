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

## Поведінки

Хост реєструє RequestLogging, Performance, Validation, Exception і DomainEventDispatcher саме в такому порядку. Точну семантику описано в [поведінках Mediator](../../runtime/mediator-behaviors.md).

## Сповіщення

`IDomainEventContext` відкриває сутності з накопиченими подіями. `IDomainEventDispatcher` публікує події. `DomainEventNotification` є оболонкою сповіщення Mediator. Обробник сповіщень за замовчуванням навмисно порожній; обробники окремих модулів підписуються на шаблон сповіщень/подій, де це потрібно.

## Підхід до журналювання й допоміжних функцій

Папка Logging містить сталі шаблони структурованих записів для запитів, валідації, швидкодії, доменних подій, скасування й необроблених збоїв. Використовуйте їх через наявні поведінки; не журналюйте сирі конфіденційні дані запиту.

`PhoneNumberHelper` — спільна утиліта нормалізації й валідації номера телефону. Повторно використовуйте її замість дублювання розбору номерів.

## Доступність контрактів

`ICurrentUserService` та `IPermissionService` мають спільні реалізації в Infrastructure. Не впроваджуйте абстракцію Application у нову функціональність, доки власник модуля не надасть і не зареєструє її реалізацію.
