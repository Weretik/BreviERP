# Traceability and readiness

| Rule/scenario | Planned tasks | Evidence target | Status |
| --- | --- | --- | --- |
| R-001; SC-001 | TS-001/002/003/005 | Domain + API duplicate SKU and parent tests | Planned |
| R-002/003; SC-002 | TS-001/002/004/005/007 | Two Articles with separate fabrics and eight common lines | Planned |
| R-004; SC-003/006 | TS-001/004/005/007 | Twelve shared operations, review blocker and actor/time | Planned |
| R-005; SC-004 | TS-001/003/004/005 | Reference/unit validation and atomic failure | Planned |
| R-006/007; SC-005 | EN-001, TS-002/005/006 | Old Product request regression; row match report | Planned |
| R-008 | EN-003, TS-006 | Existing pricing tests remain green; no new price output | Planned |

## Readiness review

The [2026-10-25 implementation checkpoint](implementation.md) supersedes the planned statuses above for TS-001–006. Domain/application/HTTP code and additive mappings are implemented; focused tests cover their observable behavior. EN-002 is resolved through existing policies and Product-level optimistic concurrency. EN-001 and TS-007 remain blocked on a target database and frozen source; TS-002 still requires real PostgreSQL empty/upgrade verification. EN-003 remains an explicit OtherCost pricing blocker. No import or deployment acceptance is claimed.
