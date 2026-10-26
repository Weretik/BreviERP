# TS-008C review evidence — 2026-10-26

Specification: [source review](review.md). The implementation and local source review are complete; fresh regression and operational evidence are recorded below. No database input, review flag, approval, quantity, active state or publication was changed.

## Actual source comparison

Explicit `pricing-input-review` command completed with 3,836 matching subjects, 0 differing subjects and 2 held offers. Groups: 124 composition counts, 1,448 material lines, 1,822 operation lines, 10 PPE rules, 119 PPE offers, 1 settings selection, 312 effective reference inputs. Every check retains field, expected/current value, source key and current version; composition checks retain Product/composition/line identity. Reports are ignored local files, not approval tokens.

## Full Sewing arithmetic regression

The existing diagnostic was expanded to all 248 imported Sewing Articles: 246 match all 11 values; 452 and 453 differ in 10 values each (materials match). The command correctly exits nonzero after saving `.local/pricing/ts008-sewing-all.json`. This is an identified source inconsistency. The user explicitly accepted retaining the manual operation and the resulting prices in DR-C01; preserve the raw differences rather than claiming exact parity.

Both Articles belong to MOD-061. `Трудовитрати виробів!1820` contains a manual operation named `З’єднання розрізаних деталей штанів по горизонталі`, quantity 1, manual minutes 4. The imported composition matches it exactly. Total minutes are 72.44 including that row, or 68.44 without it. Source seamstress value 152.14684959349594 equals `68.44 * 17500 / (20.5 * 480) * 1.25`; backend value 161.03912601626016 equals the same calculation with 72.44. This demonstrates the omission in frozen output; it does not establish whether the source formula was stale, deliberately excluded the row or the row is obsolete.

| Article | Source retail / wholesale / large | Current diagnostic retail / wholesale / large |
| --- | --- | --- |
| 452 | 1086.96 / 925.67 / 876.58 | 1121.80 / 955.34 / 904.68 |
| 453 | 983.73 / 837.76 / 793.33 | 1018.57 / 867.43 / 821.43 |

Do not delete the operation or change the global engine to force this one model to match cached prices. A catalogue decision must choose retaining the operation and accepting corrected calculated outputs, or explicitly removing it as obsolete through the composition workflow. The user selected retaining the operation and accepting its calculated outputs; [DR-C01](review-decisions.md) records exact scope and recovery. They can proceed to the explicit TS-008D confirmation workflow after version revalidation.

## Review decisions and TS-008D handoff

- Preserve matching imported values; no numeric correction is warranted for the 3,836 checked subjects.
- SKU 724: hold for a verified supplier-cost/source-output decision; do not synthesize zero. Its absence matches the source but is not sufficient for calculation.
- SKU 931: preserve inactive state; exclude from activation unless the owner explicitly changes that decision.
- Sewing 452/453: DR-C01 accepts the 4-minute operation and calculated outputs. Other 246 passed exact numerical parity. All still require explicit review confirmation/save/publication in TS-008D.
- PPE rules/offers: exact frozen-source matches; confirmation remains separate. Rule absence for external-price paths is not repaired by inventing a rule.

TS-008C source review is delivered. No numeric correction to stored inputs is needed: they match their sources. The 724 gap and inactive 931 are explicit exclusions; no claim of 248 exact numerical source matches or completed activation is made. Every future write must re-read expected versions, use the existing manager workflow and retain history.

## Fresh verification

- Valid Red: Numeric_scale_is_not_a_source_difference_including_nested_markups failed on 1.0 versus 1.00 and nested .50 versus .5. Green normalizes decimal representations without changing amounts.
- `dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter 'PricingInputReviewTests|PricingReadinessTests|PpePricingParityTests' --verbosity quiet`: 10 passed.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter 'PricingInputReviewTests|PricingReadinessTests|PpePricingVerificationTests' --verbosity quiet`: 4 passed, including baseline matches, changed/deleted line detection, unchanged real draft flags and read-only transaction execution.
- JobDependencyTests: 1 passed. Host.Seed build: passed, 0 warnings/errors. Scoped dotnet format whitespace: passed; a workspace-load warning was emitted, and the pre-existing UnitTests NU1903 warning was not suppressed.
- Explicit input-review/readiness mode conflict and missing settings-source path were rejected before database work.
- Final local input review: exit 0, 3,836 matched, 0 different, 2 held. Full Sewing parity: expected nonzero exit with the 20 retained differences for DR-C01 only. No unexplained numerical differences remain in this snapshot; the exception is scoped and revalidation is required.
- No migration, import, approval, price publication, full suite, browser, production deployment, commit or push was performed.

Final input-review SHA-256: `5b58db0ee98484d7c40540539dfa02341816daf36e651bbc4b048b9647a16fa1`. Full Sewing parity SHA-256: `38809488fdc3d2d771334423f7e69f6630db52c7d2c16a09853e0d7fc7a003e7`. All detailed reports/decision snapshots are ignored under `.local/pricing/`.

Post-review strict readiness recheck: 367 rows, 0 calculation-ready, 1 source-issue item, 0 approved offers and 0 current Sewing publications. Source verification did not change any approval/publication state. Detailed output: `.local/pricing/ts008-post-review-readiness.json`.

## Subsequent authorized activation

TS-008D has now completed locally under the human confirmation: [execution evidence](execution-evidence.md). The historical source-review decision/proof is retained; 724/931 remain excluded. Prices were calculated/saved/published through common backend workflows, not manually assigned from this document.
