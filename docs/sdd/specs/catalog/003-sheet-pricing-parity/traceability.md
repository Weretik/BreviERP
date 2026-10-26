# Traceability and readiness

| Rules/scenarios | Tasks | Evidence target | Status |
| --- | --- | --- | --- |
| R-001/002/003/004; SC-001/002/003 | EN-001/002, TS-001/002/003 | Article `246`/`247` component and tier parity | Verified for internal calculation; [evidence](sewing-evidence.md) |
| R-004; SC-004 | TS-003 | Boundary tests 9/10/40/41 | Verified |
| R-005; SC-005 | TS-001/002/003 | Named blockers, no zero-as-price | Verified, including strict review and version ambiguity |
| R-006 | TS-003/004 | Precision and published/displayed values | Backend calculation/storage/publication and TS-005 Admin display cutover verified |
| R-007/008; SC-006 | TS-006 | PPE fallback/override, approval, replay, migration and Admin integration | [Verified locally](ppe-evidence.md) |
| R-010; SC-R01/02/03/04 | TS-005/007 | Current Admin consumers, representative source parity and deprecated metadata fields | [Verified locally](cutover/evidence.md) |
| R-009; SC-007 | Separate future quote scope | Historical quote snapshots | Deferred; explicitly outside this SDD |
| R-011; SC-P01/02/03 | EN-004, TS-004 | Immutable snapshots, retries, stale protection, retained publication history | [Verified](publication-evidence.md) |
| R-011; SC-P04 | TS-004 | Atomic Reference revisions, settings/OtherCost concurrency, norm precision | [Verified](publication-evidence.md) |
| R-011; SC-P05 | EN-004, TS-004 | Protected HTTP routes, claim actor, complete settings and schema parity | [Verified](publication-evidence.md) |

**Readiness:** backend EN-004/TS-004 is implemented alongside the initial sewing calculation slice. Saved results, publication APIs and append-only input editing are available; see [publication evidence](publication-evidence.md). TS-005 Admin integration and TS-006 PPE are verified locally. TS-007 catalogue comparison and legacy retirement are verified locally; see [reconciliation](cutover/reconciliation.md). Historical quote migration is outside this slice. No production deployment is claimed.

TS-008 SC-A01..05 / TS-008A/B: [verified locally](activation/evidence.md), full inventory/readiness report. SC-A06 / TS-008C/D: gated reviewed-data activation, not executed. [Current readiness result](activation/readiness-result.md).

TS-008C SC-C01..04: [verified source review](activation/review-evidence.md), 246 exact Sewing parity matches and 2 [accepted DR-C01 differences](activation/review-decisions.md). TS-008D confirmation/publication remains separate with 724/931 exclusions.

TS-008D / SC-D01..06 and SC-A06: [verified local confirmation/publication](activation/execution-evidence.md); 365 ready, 2 explicitly excluded. The preceding gated status was the pre-authorization state.
