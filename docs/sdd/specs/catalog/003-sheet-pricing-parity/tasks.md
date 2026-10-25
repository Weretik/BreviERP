# Prerequisites and task graph

EN-001/002/003/004 and TS-001/002/003/004 are complete for the [initial sewing slice](sewing-implementation.md), with [local PostgreSQL/source parity evidence](sewing-evidence.md). The table preserves dependencies for subsequent publication, consumer cutover and PPE work. Follow the [delivery rules](../../../standards/delivery-rules.md).

| ID | Depends on | Work and evidence |
| --- | --- | --- |
| EN-001 | — | Freeze approved setting values, source formula text and at least one Article fixture per price path; record numeric precision and provenance. `MOD-025` values in [comparison](comparison.md) are the initial sewing fixture. |
| EN-002 | Article/composition SDD 002 | Provide Article BOM/operation read contract and non-inventory cost ownership. Evidence: validated input fixture without Google runtime access. |
| EN-003 | — | Decide Pricing storage owner and Reference cost-version contract; define EF schema and safe migration/rollback. |
| EN-004 | — | Backend auth/idempotency/concurrency and canonical OpenAPI implemented; clean-commit frontend pin belongs to TS-005. |
| TS-001 | EN-001/002 | Implement pure sewing component calculation using setting-driven minute cost, assembly, **multiplicative** cutting/master/manager/bonus and overhead. Tests: labor components for `MOD-025` and missing/divisor inputs. |
| TS-002 | TS-001, EN-003 | Implement versioned settings/rule reads and Article input resolution; tests for effective date, common/article lines, OP-155 and Other cost. |
| TS-003 | TS-002 | Implement three sewing tiers and typed blocker/readiness result; tests for 9/10/40/41, precision and `246`/`247` displayed parity. |
| TS-004 | TS-003, EN-003/004 | Persist calculation/published versions and expose agreed API; tests for stale publish, auth, idempotency and previous price retention. |
| TS-005 | TS-004 | Transition Product admin/list and dependent clients to Article price projection; contract synchronization and focused regression for old Product reads. |
| TS-006 | EN-001/003, TS-003 | Implement PPE rule scope, three markup tiers and optional external-price precedence; tests for fallback/override and missing/ambiguous rule. |
| TS-007 | TS-004/005/006 | Update feature/data-model maps and historical pricing SDD status with evidence; compare a representative catalogue sample and record unexplained differences. |

Next: TS-005 synchronizes the committed contract and integrates saved calculation/publication and Article price projection into frontend consumers. Backend EN-004/TS-004 and append-only cost/settings workflows are implemented; see [publication evidence](publication-evidence.md). PPE TS-006 remains separate. The current source and local database are available; no production migration/deployment is authorized by this document.
