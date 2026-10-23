# BuildingBlocks.Application

## Structure

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
├── Logging/                  stable structured log helpers
├── Helpers/PhoneNumberHelper.cs
└── ApplicationAssemblyMarker.cs
```

## Behaviors

The host registers RequestLogging, Performance, Validation, Exception, and DomainEventDispatcher in exactly this order. Their precise semantics are described in [Mediator behaviors](../../runtime/mediator-behaviors.md).

## Notifications

`IDomainEventContext` exposes entities with accumulated events. `IDomainEventDispatcher` publishes events. `DomainEventNotification` is a Mediator notification wrapper. The default notification handler is intentionally empty; individual module handlers subscribe to the notification/event pattern where needed.

## Logging and helper approach

The Logging folder contains stable structured log templates for requests, validation, performance, domain events, cancellation, and unhandled failures. Use them through existing behaviors; do not log raw sensitive request data.

`PhoneNumberHelper` is a shared utility for phone number normalization and validation. Reuse it instead of duplicating number parsing.

## Contract availability

`ICurrentUserService` and `IPermissionService` have shared implementations in Infrastructure. Do not introduce an Application abstraction into new functionality until the module owner provides and registers its implementation.
