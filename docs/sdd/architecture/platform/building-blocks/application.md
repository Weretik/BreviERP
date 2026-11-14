# BuildingBlocks.Application

## Structure

```text
BuildingBlocks.Application/
├── Behaviors/
│   ├── RequestLoggingBehavior
│   ├── ExceptionBehavior
│   ├── PerformanceBehavior + options
│   └── ValidationBehavior
├── DependencyInjection/
│   └── AddModuleValidators
├── Errors/
│   └── ErrorResults           Error → Ardalis.Result, the one mapping
├── Validation/
│   └── UkrainianValidationLanguage
├── Logging/                  stable structured log helpers
├── Helpers/PhoneNumberHelper.cs
└── ApplicationAssemblyMarker.cs
```

## Behaviors

The host registers RequestLogging, Performance, Validation, and Exception in exactly this order. Their precise semantics are described in [Mediator behaviors](../../runtime/mediator-behaviors.md).

## Logging and helper approach

The Logging folder contains stable structured log templates for requests, validation, performance, cancellation, and unhandled failures. Use them through existing behaviors; do not log raw sensitive request data.

`PhoneNumberHelper` is a shared utility for phone number normalization and validation. Reuse it instead of duplicating number parsing.

## What stays out

Only code that every module needs lives here (stage 3c, 2026-11-14). Removed then because no module used them: domain event dispatch (no module raised an event), `ICurrentUserService`/`IPermissionService` with their always-allow fake implementations, `ICacheService`, `IEmailService`, `IEnvironmentService`, and a duplicate Mediator pipeline list. `PricingInputChanges` moved to `Catalog.Contracts`, because it is a Catalog concept that Reference reports to. The current user comes from the module's own port (for example the `By` of a command filled by the Api from the claims), and access checks are host policies.
