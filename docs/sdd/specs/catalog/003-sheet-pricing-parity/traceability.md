# Traceability and readiness

| Rules/scenarios | Tasks | Evidence target | Status |
| --- | --- | --- | --- |
| R-001/002/003/004; SC-001/002/003 | EN-001/002, TS-001/002/003 | Article `246`/`247` component and tier parity | Verified for internal calculation; [evidence](sewing-evidence.md) |
| R-004; SC-004 | TS-003 | Boundary tests 9/10/40/41 | Verified |
| R-005; SC-005 | TS-001/002/003 | Named blockers, no zero-as-price | Verified, including strict review and version ambiguity |
| R-006 | TS-003/004 | Precision and published/displayed values | Backend calculation/storage/publication verified; frontend display cutover pending |
| R-007/008; SC-006 | TS-006 | PPE fallback/override cases | Planned |
| R-009/010; SC-007 | TS-004/005/007 | Price version and quote snapshot regression | Planned |
| R-011; SC-P01/02/03 | EN-004, TS-004 | Immutable snapshots, retries, stale protection, retained publication history | [Verified](publication-evidence.md) |
| R-011; SC-P04 | TS-004 | Atomic Reference revisions, settings/OtherCost concurrency, norm precision | [Verified](publication-evidence.md) |
| R-011; SC-P05 | EN-004, TS-004 | Protected HTTP routes, claim actor, complete settings and schema parity | [Verified](publication-evidence.md) |

**Readiness:** backend EN-004/TS-004 is implemented alongside the initial sewing calculation slice. Saved results, publication APIs and append-only input editing are available; see [publication evidence](publication-evidence.md). TS-005 frontend/consumer contract synchronization and PPE TS-006 remain open. Historical quote migration is outside this slice. No production deployment is claimed.
