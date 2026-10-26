# TS-008D local execution evidence — 2026-10-26

[Specification](execution.md) was saved before implementation. The human explicitly authorized the reviewed 2026-10-25 data, input confirmation and local price publication, retaining [724/931 exclusions](data-decisions.md). DR-C01 retains the manual operation and uses the common backend rules; no runtime SKU-specific price override was introduced.

## Accepted result

| Criterion | Result |
| --- | --- |
| SC-D01 plan | Immutable plan `4a13c20d-4c1d-4e45-87dd-a8a4bd735adc`: 124 models / 248 Sewing Articles, 10 rules, 117 PPE offers; exact source and input versions/request UUIDs retained. |
| SC-D02 target/actor/backup | Local brevidb only; existing unlocked Admin user ID 1 validated. Explicit confirmed apply with plan/backup hashes. No account/role created and no HTTP authentication fabricated. |
| SC-D03 confirmations | 1,448 material and 1,822 operation lines reviewed through existing handlers; 10 new approved rule and 117 offer versions appended. Numeric values preserved. |
| SC-D04 publication | 248 saved calculations / 248 current publications reopened via existing read ports. Calculation identities and all expected backend-calculated tiers match the plan. |
| SC-D05 exclusions | 724/931 retain the same original offer version IDs and remain unapproved. 724 is blocked by missing supplier data; 931 remains inactive. |
| SC-D06 repeat/safety | Isolated PostgreSQL integration confirms unchanged-input replay creates no duplicate history, stale values are rejected before writes, unusable progress path fails before confirmation, and legitimate replacement line IDs preserve source equivalence. |

The local execution journal is Complete with 499 steps: 10 RuleConfirmed, 117 OfferConfirmed, 124 CompositionConfirmed and 248 Published. Final strict readiness is 365/367: 248 Sewing and 117 PPE. Remaining item blockers are UnapprovedOffer/MissingSupplierCost for 724 and InactiveProduct for 931. Exactly 248 current Sewing publication IDs are present. Reopened-port verification independently reports 248 Sewing / 117 PPE; it does not rely on journal totals.

Final source review: 3,836 matching subjects, 0 differing subjects, 2 held offers. Original catalogue source hash remains `9C8079C1259377BE1ECEE6F85FF50874BFB9B2819EA1B5FFF8ECF6CC48DCBD28`. Reference/settings versions were already approved and were not rewritten. Common backend handlers recreate composition line IDs and advance versions; immutable import mappings remain original provenance and semantic replacement lookup keeps current review valid.

Real local database readback (total / attributed to existing user 1): rules 20/10; offers 236/117; saved calculations 248/248; publications 248/248; material lines 1448/1448 reviewed; operation lines 1822/1822 reviewed. Original PPE drafts remain alongside approved versions. For 452 prices are 1121.80 / 955.34 / 904.68; for 453 they are 1018.57 / 867.43 / 821.43, computed from ordinary backend inputs including the retained 4-minute operation.

## Backup and retained reports

Backup: `.local/pricing/before-ts008d-20261026T101308Z.dump`, 449305 bytes, SHA-256 `D895F4E69FD0F2FB3A0B2EBE69CD480D44C6A876F0D6D7296ABDA3E1E4A0BD9A`. Custom PGDMP header, pg_restore listing and checksum verified before apply. Backup contains private data and remains ignored locally. Restore was not run. The actor/source plan and progress/reopen/readiness reports are also ignored; no credentials are documented.

| Report under `.local/pricing/` | SHA-256 |
| --- | --- |
| `ts008d-activation-plan.json` | `763e6824eb51fde310dbfd6de44a295018d6eb82708e7d0710989b171b188ffb` |
| `ts008d-activation-report.json` | `ddf6de81eb7f304cecc1732b68f490f233281c683cde8ec7141340b2df359c24` |
| `ts008d-final-readiness.json` | `cbb0b3ae4966774986a563c234e0e71dbdd27cdf57b0c431fa573a4aee0a5eb5` |
| `ts008d-final-input-review.json` | `791a48f7eda2f5ecc9b22d9dcb67a7ca6b714d9348ae00a1f6dbac6d4a2476c7` |
| `ts008d-reopened-prices.json` | `6725c1da578d4c3f01c56b7c45543e2b17c6f3cfdf7c562f27d6097d8f160d6b` |

## Fresh verification and execution

- Unit filter `PricingActivationTests|PricingInputReviewTests|PricingReadinessTests|PpePricing`: 39 passed.
- Integration filter `PricingActivationTests|PricingInputReviewTests|PricingReadinessTests|SewingPricingPublicationTests|SewingPricingConcurrencyTests|PpePricingPersistenceTests`: 7 passed in isolated databases. The updated activation scenario was rerun after adding reopened-price verification and passed.
- JobDependencyTests: 1 passed. Host.Seed builds passed with 0 warnings/errors; scoped formatter returned success. The existing UnitTests NU1903 OpenApi.YamlReader warning and formatter workspace-load warning were not suppressed.
- Valid Red: an unavailable journal still allowed a rule append (expected 1 row, observed 2) in the isolated fixture. Green: initial progress persistence now happens before any confirmation. Another regression caught the existing common handler's two version increments (ReplaceComposition plus SetModelCode); resume now recognizes exactly these existing backend increments without weakening the version check.
- An initial incomplete test source was rejected by the existing source-issue guard. Automatic review rejected removing that guard; it stayed intact. A separate complete source fixture was added instead, preserving normal fixture behavior and all production safeguards.
- Unknown actor and incorrect backup checksum were rejected by actual CLI checks before application. Host.Seed plan mode completed without business-data writes; explicit apply completed only after checks/backup.
- Apply used `--pricing-activate apply --confirmed true --actor-id 1`, the immutable plan/hash and verified backup/hash. Subsequent `--pricing-activate verify` reopened every result; readiness/input-review modes completed successfully. See [exact command forms](../../../../operations/data/pricing-activation.md).
- No schema change/migration, normal seed/import, full suite, browser/E2E, production deployment, commit or push was performed. This evidence proves local data/backend read results; it is not a visual UI or production acceptance claim.

## Recovery and remaining work

The authorized local TS-008D is complete. Keep 724/931 decisions until explicitly revised. Use normal version-aware manager editing for future cost/norm/rule changes and normal save/publish for new Sewing prices. If a later correction is needed, retain history and append a correction; do not silently replace prices with old cached Google outputs or auto-restore the backup. Any production release and UI acceptance remain separate, following the frontend-first consumer transition.
