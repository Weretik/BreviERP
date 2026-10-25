# Prerequisites and implementation tasks

The implementation and remaining gates are recorded in the [checkpoint](implementation.md) and [local import evidence](catalog-import-evidence.md). Focused tests follow the repository's [delivery rules](../../../standards/delivery-rules.md). State-changing runs require direct user authorization; the current user-authorized local transfer is scoped by the [import revision](catalog-import.md).

| ID | Depends on | Work and evidence target |
| --- | --- | --- |
| EN-001 | — | Before import, verify the selected target has zero Product rows and inspect applied migrations. Match existing Reference data, freeze the source and record source-to-Brevi IDs. Evidence: preflight and reconciliation report. Domain/schema work does not depend on this. |
| EN-002 | — | Confirm admin write policy and concurrency/route design; update product OpenAPI with exact operation IDs, DTOs and errors. Evidence: contract review/diff. |
| EN-003 | — | Assign `OtherCost` reference ownership and unit semantics, including source line `FUR-032`. Evidence: documented Reference/Pricing decision; does not block Article identity but blocks full cost parity. |
| TS-001 | EN-002 | Add typed Article/composition IDs, domain rules R-001–R-004 and focused domain tests for duplicate SKU, scope, quantity and repeated operations. |
| TS-002 | TS-001 | Add EF mappings and additive migration; verify empty and existing-schema migrations, unique SKU and parent/article integrity. |
| TS-003 | TS-001, TS-002, EN-002 | Implement Article commands/queries and Reference validation; focused application tests for SC-001/004 and atomic rejection. |
| TS-004 | TS-003 | Implement composition commands/queries/readiness; focused tests for shared/article lines, review and stale update. |
| TS-005 | TS-003, TS-004, EN-002 | Implement HTTP routes and exact OpenAPI; focused API tests for 200/201/400/403/404/409 and old Product contract compatibility. |
| TS-006 | TS-005 | Update Catalog feature description and data-model map after implementation; run focused regression for existing Product create/replace/read and relevant Reference reads. |
| TS-007 | EN-001, TS-006 | Implement and execute the full local catalogue import under the [import revision](catalog-import.md): validate a frozen source, trial `MOD-025`/`246`/`247` with rollback, then load and reconcile all models/articles/PPE and composition lines. Pricing/EN-003 does not block this local transfer; no shared or production import. |
| EN-004 | TS-006 | Audit and decompose the implemented sewing capability by use case and responsibility, preserving behavior and public contracts. Split production files by primary type; separate write persistence from read projections and keep readiness policy outside Infrastructure. Evidence: code-audit artifact, focused regression commands/results, and updated feature/data-model maps if paths or ownership change. No schema, OpenAPI, or live database changes. |

**First ready technical slice:** EN-002, then TS-001/TS-002. EN-001 is required before executing any source data import; EN-003 is required before cost parity, and may be resolved alongside domain design. Change the task graph if accepted business rules or current database shape differ.
