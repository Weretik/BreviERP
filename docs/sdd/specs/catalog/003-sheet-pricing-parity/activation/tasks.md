# Tasks and activation order

| Task | Dependency | Work | Completion evidence |
| --- | --- | --- | --- |
| TS-008A | TS-007 | Implement explicit local read-only readiness report, full source inventory, per-kind strict readers, source issues and independent publication state | SC-A01..05 focused unit/integration tests; types/build/format/dependency checks |
| TS-008B | TS-008A | Run on verified local imported catalogue, inspect counts/blocker groups/source gaps and record actual summary | Ignored detailed JSON; versioned summary and no-write test evidence |
| TS-008C | TS-008B, reviewed data decisions | Correct source gaps and review input drafts, one scoped group at a time | Before/after IDs, exact decision/source and manager actor; rerun readiness |
| TS-008D | TS-008C | Confirm PPE inputs/rules; calculate/save/publish ready Sewing; verify current consumer | SC-A06 evidence, publication/calculation IDs and final readiness/consumer checks |

## Subsequent activation checklist

1. Inspect every report item, including missing identities/inactive records and source issues. Do not replace missing values with zero. Obtain a verified source for PPE 724 or leave it blocked.
2. Review shared settings/rules before their dependent items. Source capture date and approval date are different facts. Record decisions for costs, norms, markups and optional external prices.
3. Apply accepted corrections through existing versioned editing workflows, preserving expected-version conflicts and request identities. Do not auto-approve all imported drafts.
4. Re-run strict readiness. Confirm only explicitly reviewed PPE rule/offer inputs as the authorized manager; verify resulting three-tier values/version IDs.
5. For each ready Sewing Article, calculate at the accepted instant, save the result, verify the saved snapshot and explicitly publish it. A retained earlier publication must remain visible if the new calculation is blocked.
6. Verify list/card price status and provenance, keep unresolved items blocked, and record acceptance. Production release follows [frontend-first order](../../../../operations/data/pricing-cutover.md).

TS-008C/D are gated follow-up work. Missing source data or absent review decisions must be recorded per item; they do not block implementing/running TS-008A/B. No new UI or approval API is planned because these workflows already exist.

## Current execution status

TS-008A: implemented and verified (unit, CLI, PostgreSQL integration and job boundaries). TS-008B: complete; 367 rows inventoried, 0 strictly ready, 1 source-issue item, no missing identities. TS-008C: source review complete, DR-C01 accepted, 724/931 excluded with explicit holds; no stored numeric correction needed. TS-008D: complete locally; 248 Sewing publications and 117 current PPE prices verified, with 724/931 preserved. [Execution evidence](execution-evidence.md). See [exact evidence](evidence.md) and [actual readiness groups](readiness-result.md).

TS-008C1/C2/C3 evidence: [source review](review-evidence.md) and [accepted decisions/handoff](review-decisions.md). Preserved full parity output contains two explicitly accepted source differences, not zero raw differences.

TS-008D SC-D01..06 are accepted locally: [execution and recovery](execution.md), [fresh evidence](execution-evidence.md). Production release/UI acceptance is outside this execution.
