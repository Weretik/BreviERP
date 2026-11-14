# ADR-0006: Modules talk through their own Contracts projects

**Status:** accepted · **Date:** 2026-11-14 · **Owner:** Виталий (project owner) · **Supersedes:** [ADR-0002](0002-cross-module-ports.md)

## Context

[ADR-0002](0002-cross-module-ports.md) let each consuming module declare a port and let `Host.Api` implement it. By 2026-11-14 the host held 54 such adapters (folders `Host.Api/Crm`, `Production`, `Purchasing`, `Warehouse`, `Content`, `Accounting` and `DependencyInjection/ServiceRegistration`). The host knew the data of every module, a change in one module broke adapters far from it, and the trade-off ADR-0002 named ("god host") had happened. KedrStore uses the same rules, so both backends should look alike.

## Decision

Every module gets a `<Module>.Contracts` project: the only part of a module another module may reference. It holds small interfaces in the provider's words (`IProductPriceReader`, `IStaffDirectory`), the plain DTO records they return, integration events, and shared constants. It references no project of any module.

- The provider implements its interfaces in `<Module>.Infrastructure/Contracts/<Feature>/` (or Application, for pure logic) and registers them in `Add<Module>Module()`.
- The consumer's Application (or Infrastructure, for an adapter into its own port) references the provider's `Contracts` and injects the interface.
- "After X happens" reactions are integration events declared in the provider's `Contracts`.
- The host only calls `Add<Module>Module()` for each module and configures cross-cutting concerns. It holds no readers, adapters or notifiers.

The full rule text is in the [module rules](../../standards/module-rules.md#communication-between-modules).

## Options considered

1. **Keep host adapters (ADR-0002).** Rejected: the host keeps growing and knows every module.
2. **Modules reference each other's Application.** Rejected: a consumer would see handlers and Domain types and break on any internal change.
3. **Contracts project per module (chosen).** One small, stable surface per module, checked by architecture tests.

## Consequences

- Modules: each gains a Contracts project; host adapters move to the provider's (or consumer's) Infrastructure. Behaviour and HTTP API do not change.
- Data: no schema change; modules still keep other modules' ids as values.
- Tests: architecture tests check that a module references only other modules' `Contracts`, that `Contracts` references no module project, and that the hosts contain no module ports.
- Rollout: in stages, one pull request each, listed in the [architecture overview](../README.md#structure-migration).
