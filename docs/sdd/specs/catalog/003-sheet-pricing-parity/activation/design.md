# Design and boundaries

Extend the existing `SewingPricing` job project, which already references CatalogImport and Catalog/Reference Infrastructure. Do not create another project or reference from runtime modules back to jobs. Catalog owns price/readiness rules; Reference owns effective cost/norm versions. Host.Seed owns mode selection, path and serialization only.

| Path | Responsibility | State before implementation |
| --- | --- | --- |
| `src/Jobs/SewingPricing/Readiness/PricingReadinessJob.cs` | Orchestrate full report | Planned |
| `Readiness/Reading/ImportedPricingReadinessReader.cs` | Local guard and immutable source inventory | Planned |
| `Readiness/Reading/PpeReadinessReader.cs` | Bounded strict PPE reads/evaluation | Planned |
| `Readiness/Reading/SewingReadinessReader.cs` | Strict Article evaluation and current publication reads | Planned |
| `Readiness/Reporting/` | One report/item/blocker/source model per file; source checks and next-action classification | Planned |
| `src/Bootstrapper/Host.Seed/Jobs/Pricing/PricingReadinessCommandRunner.cs` | Explicit CLI report output | Planned |
| Existing pricing readers/evaluators/writers | Unchanged; reuse reads only | Existing |

PPE reads batches of at most 100 with the existing per-batch snapshot. Sewing Catalog/Reference reads share a repeatable-read transaction. Source inventory and the two kinds are not one atomic catalogue-wide snapshot: report timestamps and version IDs make this explicit. Concurrent changes require rerunning the report before any activation. Unexpected DB/evaluator failures fail the command rather than being disguised as ordinary data blockers.

Sewing evaluates Articles sequentially through the existing strict preview handler rather than parallelizing a shared DbContext or duplicating its rules. This explicit local inventory is not a runtime list endpoint; the full source size is recorded in execution evidence.

Source output checks inspect H/I/J for PPE and V/W/X for Sewing. They are source completeness evidence, not a second calculation engine. Settings/rule/offer IDs keep their existing identifier types. No migration, HTTP contract, frontend generation or persistence schema change is needed. Report files use ignored `.local/pricing/` paths; business data/secrets are not committed.

Recovery: readiness changes no business state; rerun or remove its local report. Later activation is append-only through existing APIs, never SQL updates or history deletion. A corrected input version replaces a mistaken latest input; a corrected publication references a new verified saved calculation. Back up the intended database before any later state-changing batch.
