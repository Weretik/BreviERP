# TS-008C scoped structure audit — 2026-10-26

Authorized scope: refactor only the recently added frozen-source input review. Earlier TS-006/007/008A/B and unrelated working-tree changes are preserved. Rules: [one responsibility/type per file](../../../../standards/backend-rules.md), [focused verification](../../../../standards/testing-rules.md). No new business behavior is planned; existing focused regression replaces an artificial Red test for code extraction.

| ID | Severity / owner | Original evidence / impact | Target and task | Status |
| --- | --- | --- | --- | --- |
| AF-IR01 | Low / review composition | `src/Jobs/SewingPricing/Review/Reading/CompositionReviewReader.cs:52` read the aggregate and owned two independent field shapes for material and operation comparisons. A comparison-field change required editing the EF reader. | TS-008C: private MaterialLineReviewComparer and OperationLineReviewComparer in Comparison/Composition; reader orchestrates source mapping and checks. | Verified |
| AF-IR02 | Low / review inputs | `Reading/ReferenceInputReviewReader.cs:18` combined Catalog sewing-settings selection with Reference cost/norm comparison, despite their distinct owners and sources. | TS-008C: SettingsInputReviewReader and ReferenceInputReviewReader in Reading/Reference, called separately by the job. | Verified |
| AF-IR03 | Low / navigation | Reading held composition, PPE, settings and Reference input flows at one level. | TS-008C: capability folders Composition, Ppe and Reference. Keep public namespaces/report shapes unchanged. | Verified |

Keep decisions: the 51-line job owns the local guard, source compatibility and one shared read-only repeatable-read transaction; splitting its context lifetime would obscure the consistency boundary. PPE latest-record comparison and hold classification remain one read-review scenario. Recursive field comparison owns normalization and equality only; report types already have separate files. The command runner owns CLI/path/report/exit handling only. No additional project, interface, generic helper folder or dependency is justified. All types remain operational job types; runtime modules/API do not depend on them.

No arithmetic, Domain/Application rules, source values, DTO schema, approval, publication or database schema changes are authorized by this refactor. MOD-061's manual operation follows the same backend calculation as every other model; DR-C01 is source discrepancy evidence, not a special runtime pricing rule. [Fresh verification](review-structure-verification.md).
