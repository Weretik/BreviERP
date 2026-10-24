# Prerequisites and implementation tasks

The implementation and remaining gates are recorded in the [2026-10-25 checkpoint](implementation.md). The graph below preserves the planned dependencies. Focused tests follow the repository's [delivery rules](../../../standards/delivery-rules.md). No live import or database migration is authorized by this document.

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
| TS-007 | EN-001, TS-006, EN-003 | Design and dry-run the full model/article catalogue import in a non-production database, using `MOD-025`/`246`/`247` as the first reconciliation fixture; check ten material and twelve operation lines. No production import in this task. |

**First ready technical slice:** EN-002, then TS-001/TS-002. EN-001 is required before executing any source data import; EN-003 is required before cost parity, and may be resolved alongside domain design. Change the task graph if accepted business rules or current database shape differ.
