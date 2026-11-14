# ADR-0002: Modules talk through ports implemented in the host

**Status:** superseded by [ADR-0006](0006-module-contracts.md) on 2026-11-14 · **Date:** 2026-11-13 (recorded from the current code)

## Context

Modules must not reference each other (ADR-0001), but real features cross module lines:

- An offer (Crm) needs product prices (Catalog) and fabric facts (Reference).
- Payroll (Production) needs pricing parameters (Reference).
- An invoice (Purchasing) needs AI recognition (Ai).

## Decision

A module that needs outside data declares a small interface (a port) in its own Application layer, in its own words. Example: `IPayrollParameterReader` in Production.

`Host.Api`, the composition root, implements the port. The adapter sends the other module's public query through Mediator and maps the result. Adapters live in folders such as `Host.Api/Crm` and `Host.Api/Production`.

## Why

- Modules stay independent and testable. A unit test replaces the port with a fake.
- The consuming module defines only what it needs, not the other module's full model.
- The rule is simple and the architecture tests check it.

## Trade-offs

- The host now knows every module and holds many adapters (13 for Crm alone). It is starting to become a "god host".
- Next step when this grows: give each module a small public contracts project (its exposed queries and DTOs), or use integration events for "after X happens" cases such as "offer accepted → create production order".
