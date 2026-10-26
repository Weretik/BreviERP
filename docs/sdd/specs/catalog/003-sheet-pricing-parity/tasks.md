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
| TS-006 | EN-001/003, TS-003 | Implemented and verified locally: PPE rule scope, three tiers, external-price precedence, immutable input versions, explicit draft bootstrap and Admin consumer. [Scope](ppe-implementation.md), [evidence](ppe-evidence.md). |
| TS-007 | TS-004/005/006 | Implemented and verified locally: representative Sewing and complete imported PPE reconciliation, legacy arithmetic/consumer retirement, current Admin pricing and documentation. See [cutover evidence](cutover/evidence.md) and [source reconciliation](cutover/reconciliation.md). |

TS-007 is complete locally. The current Admin aggregate contract pin is `0529daf910b837eb808a662695706dda4beaf833`; earlier TS-005/006 checkpoints remain historical evidence. Next operational work is explicit review of input drafts and missing source data (PPE SKU 724), followed by the [frontend-first release procedure](../../../operations/data/pricing-cutover.md). Review/approval/publication is not automatic. Storefront API adoption and historical quote snapshots are separate future scope. No production deployment is claimed.

## TS-008 follow-up

The [readiness and controlled activation extension](activation/README.md) defines TS-008A/B implementation/report execution and the TS-008C/D gated data review and activation checklist. Follow its acceptance and evidence rather than treating TS-007 arithmetic parity as business approval.

TS-008C source review is delivered; see [results](activation/review-evidence.md) and [DR-C01/handoff](activation/review-decisions.md). Next: TS-008D explicit confirmation/save/publication for reviewed candidates, excluding 724/931. Original source outputs and accepted differences are preserved.

Current local status: TS-008D is [complete](activation/execution-evidence.md): 248 Sewing published and 117 PPE current; [724/931 decisions](activation/data-decisions.md) persist. No production release is claimed.
