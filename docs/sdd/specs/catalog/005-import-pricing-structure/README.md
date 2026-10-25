# Import and pricing responsibility decomposition

The user requested an audit and implementation of structural fixes for the delivered sewing composition, catalogue import and pricing capabilities. This specification coordinates the follow-up across Catalog, Reference, jobs and Host.Seed. The [audit](audit.md) owns findings and verification.

**Status:** RM-001–006 verified. [Fresh commands and final structure](verification.md).

## Rules and compatibility

- Preserve formulas, units, blockers, source dispositions, messages, CLI flags, transaction isolation/lock order, import idempotency and public HTTP contracts.
- Keep aggregate invariants in Product; use named partial files for identity, composition, readiness and profile responsibilities.
- Group pricing types by inputs, results, settings and calculation. Keep existing Domain/Application CLR namespaces stable to preserve EF migration metadata and internal consumers.
- Give import identity/category/profile translation, composition writing and source validation separate owners. Keep transaction decisions in the use-case coordinator.
- Separate the sewing pricing operational project from the catalogue import project. Pricing may depend on import provenance/source adapters; Domain, Application and Host.Api must not depend on job projects.
- Keep generated migrations, snapshots, generated OpenAPI and imported private data unchanged. Do not run real catalogue import, bootstrap or normal Host.Seed migration/seed mode during this refactor.

## Tasks and dependencies

| Task | Findings | Work |
| --- | --- | --- |
| RM-001 | AF-S01/02 | Separate product identity/profile/category and composition persistence under Persistence/Catalog; group provenance/reference adapters by owner. |
| RM-002 | AF-S03 | Split catalogue validation into identity, materials, operations and reference-master rules; move source key conventions out of the planner. |
| RM-003 | AF-S04 | Extract each Host.Seed CLI job runner; Program owns host lifecycle and a dispatcher owns job selection. |
| RM-004 | AF-S05/06 | Extract src/Jobs/SewingPricing, separate parity comparison from diagnostic reads and bootstrap reconciliation from transaction flow. Depends on RM-002 source conventions. |
| RM-005 | AF-S07 | Organize Catalog/Reference pricing types and the Product partials without changing CLR model names or schema. |
| RM-006 | All | Update navigation, check focused characterization tests/builds/model snapshots and perform read-only local pricing parity verification. |

## Verification and recovery

This is behavior-preserving decomposition; existing tests characterize the selected behavior rather than introducing artificial Red tests. Run focused source/Domain/handler unit tests, catalogue and pricing PostgreSQL lifecycle tests against disposable databases, projection/architecture checks, Host.Api and Host.Seed builds, EF pending-model checks and read-only pricing verification. No full suite or browser run is part of this request.

Recovery reverts only this structural change set while preserving earlier work. There is no database rollback because persistent data/schema is not modified.
