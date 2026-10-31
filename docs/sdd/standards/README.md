# Backend engineering rules

This directory contains stable rules for every backend change. It does not contain frontend rules; the frontend is a separate project.

- [Backend rules](backend-rules.md) — layers, CQRS, DDD, Mediator, and code organization.
- [Module rules](module-rules.md) — owning module, when to propose a new module, module shape, schema per module, and FluentValidation.
- [Identifier strategy](identifier-strategy.md) — required rules for choosing IDs before creating or changing an entity.
- [API rules](api-rules.md) — HTTP contracts, validation, results, and errors.
- [Database rules](database-rules.md) — EF Core, reads, transactions, migrations, and data.
- [Security and observability rules](security-observability-rules.md) — authorization, privacy, Serilog, and diagnostics.
- [Testing rules](testing-rules.md) — scenario coverage, Red → Green → Refactor, test selection, exceptions, and required verification commands.
- [Delivery rules](delivery-rules.md) — behavior-first SDD workflow, traceability, boundaries, and completion criteria.

Feature specifications link to these rules rather than duplicating them.
