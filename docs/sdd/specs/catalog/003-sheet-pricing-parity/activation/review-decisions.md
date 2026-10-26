# TS-008C decisions and version-aware handoff

## DR-C01 — accepted manual operation for MOD-061

Accepted by the human user in this task on 2026-10-26: keep the 4-minute manual operation and use the prices that include it; retain the question/history for possible rollback. This records the source-review decision and intended outputs, not an authenticated manager signature or an executed publication.

Frozen source: `Трудовитрати виробів!1820`. Local identity at review: Product 33, composition version 4, operation line 1410, operation reference 400, quantity 1, manual minutes 4. The two dependent Articles are 452 and 453. These identities are snapshot evidence and must be revalidated before a write.

| Article | Frozen retail / wholesale / large | Accepted calculated retail / wholesale / large |
| --- | --- | --- |
| 452 | 1086.96 / 925.67 / 876.58 | 1121.80 / 955.34 / 904.68 |
| 453 | 983.73 / 837.76 / 793.33 | 1018.57 / 867.43 / 821.43 |

Preserve the complete original workbook and raw difference report. Full Sewing parity therefore remains 246 exact matches plus 2 explicitly accepted differences, not 248 exact source matches. No other Article, formula, quantity or rule is covered by this decision. If source hash, composition version, manual quantity/minutes or selected pricing inputs change, recalculate and review the decision again.

The ignored local `.local/pricing/ts008-review-decisions.json` records the source/report hashes, precise scope and intended outputs with CanApply=false. It is review evidence, never an approval token. The current backend already includes this manual operation, so no formula or composition rewrite is needed to implement the chosen arithmetic.

### Recovery / rollback

This read-only review did not change the database, so no database rollback is needed now. Before TS-008D writes, take the required backup and retain the prior composition/input/saved calculation/publication identities. If a later business decision declares the operation obsolete, append an explicit composition correction at the then-current expected version, calculate/save/publish its result, and preserve previous history. Do not delete rows/publication history, silently restore cached prices, or remove the operation from the global engine. The human's possible-rollback instruction does not request executing a rollback now.

## Remaining holds

- 724: no verified supplier cost/complete price outputs in the frozen source. Preserve the missing values and block activation; obtain a separate source decision.
- 931: preserve inactive state and exclude from activation. No reactivation was requested.

## TS-008D handoff

Candidates for explicit input confirmation are 248 Sewing Articles (246 exact numerical matches plus DR-C01's two accepted differences) and 117 active PPE Products without the 724 hold. Candidate means source-reviewed data, not current strict readiness: real approval/review flags still block calculations until the existing manager workflows are used. Confirm shared/reference/composition data in dependency order, re-read all expected versions and rerun strict readiness. Save/publish Sewing only after it is ready; PPE follows its current-input path. Retain 724/931 exclusions. See [activation tasks](tasks.md).

## Subsequent authorized activation

TS-008D has now completed locally under the human confirmation: [execution evidence](execution-evidence.md). The historical source-review decision/proof is retained; 724/931 remain excluded. Prices were calculated/saved/published through common backend workflows, not manually assigned from this document.
