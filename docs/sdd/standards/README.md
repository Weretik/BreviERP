# Backend engineering rules

This directory contains stable rules for every backend change. It does not contain frontend rules; the frontend is a separate project.

- [Backend rules](backend-rules.md) — layers, CQRS, DDD, Mediator, and code organization.
- [Module rules](module-rules.md) — owning module, when to propose a new module, module shape with `Contracts`, communication between modules, folders inside a module, test structure, architecture tests, hosts and jobs, schema per module, and FluentValidation.
- [Identifier strategy](identifier-strategy.md) — required rules for choosing IDs before creating or changing an entity.
- [API rules](api-rules.md) — HTTP contracts, validation, results, and errors.
- [Database rules](database-rules.md) — queries and N+1, indexes and search, transactions, concurrency, migrations; Brevi specifics and open deviations.
- [Idempotency rules](idempotency-rules.md) — where a repeated request or message must not do the work twice, the `requestId` pattern, at-least-once handlers (outbox, Telegram, scheduler, imports), tests, and the 2026-11-15 audit of every write operation.
- [Security and observability rules](security-observability-rules.md) — authorization, privacy, Serilog, and diagnostics.
- [Testing rules](testing-rules.md) — scenario coverage, Red → Green → Refactor, test selection, exceptions, and required verification commands.
- [Delivery rules](delivery-rules.md) — behavior-first SDD workflow, traceability, boundaries, and completion criteria.

Feature specifications link to these rules rather than duplicating them.
