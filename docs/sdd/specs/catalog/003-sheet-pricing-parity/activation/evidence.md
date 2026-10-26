# Traceability and execution — 2026-10-26

Specification was saved before production implementation. TS-007 parity diagnostics covered representative Sewing/all imported PPE arithmetic but did not give one full strict-readiness inventory. TS-008 reuses strict evaluators without diagnostic approval overrides.

## Scenario status

| Scenarios | Tasks | Status / evidence |
| --- | --- | --- |
| SC-A01/02 | TS-008A/B | Verified: full fixture inventory and retained missing Article; local report has 367 rows with no missing identity. Existing draft IDs/flags remain unchanged. |
| SC-A03 | TS-008A/B | Verified: missing/invalid/numeric-zero source tests and separate Sewing cells; actual source issues identify SKU 724. |
| SC-A04 | TS-008A/B | Verified for this slice: unit classification separates ready/published/source review; local read reports no current publications. Existing publication storage/read contract is unchanged. |
| SC-A05 | TS-008A/B | Verified: nonlocal address rejection, mode/path process guards and isolated PostgreSQL no-write fixture with real draft rows. |
| SC-A06 | TS-008D | [Verified locally](execution-evidence.md): 248 Sewing published, 117 PPE current; exclusions preserved. |

## Fresh verification

- Initial test compilation exposed a missing UnitTests reference to the existing SewingPricing project; corrected, not claimed as Red.
- Valid Red: two source-completeness tests failed because expected H/I/J errors were missing (empty issue list). Green: source checker implemented. Final `dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter 'PricingReadinessTests|PpePricingParityTests' --verbosity quiet`: 7 passed (5 readiness + 2 existing parity tests).
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-build --no-restore --filter 'PricingReadinessTests|PpePricingVerificationTests' --logger 'console;verbosity=normal'`: 3 passed. The fixture includes unapproved rule/offer versions; the report retains their IDs/flags, supplier value and row counts without approval, saved calculations or publications. A renamed Article still produces its source inventory row with MissingArticle.
- `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --no-restore --filter JobDependencyTests --verbosity quiet`: 1 passed.
- Host.Seed and IntegrationTests focused project builds passed with 0 warnings/errors. Scoped formatter passed for new job files, runner/dispatcher and both test files; it emitted a workspace-load warning and returned success. The pre-existing UnitTests NU1903 OpenApi.YamlReader warning was not suppressed.
- Host.Seed process checks: combining readiness/PPE-verification modes returned the expected mode-conflict failure; omitting report returned the expected path failure. Both guards rejected before database access/normal seeding.
- Local read-only run: `DOTNET_ENVIRONMENT=Production dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-build --no-launch-profile -- --pricing-readiness true --report .local/pricing/ts008-readiness.json`: exit 0, 367 items, 0 calculation-ready, 1 source-issue item. In PowerShell set the environment variable as in [operations](../../../../operations/data/pricing-readiness.md).

## Environment recovery and local evidence

Initially postgresql-x64-18 was stopped. Integration fixture setup and the first explicit report attempt failed on 127.0.0.1:5432 before assertions/report generation. The current process could not start the Windows service; after the user started it, the integration filter and real report completed successfully. The blocker is resolved. No alternate database, normal seed or business migration was substituted.

The detailed ignored report and its aggregate/source evidence are recorded in [the readiness result](readiness-result.md). A successful report is not business approval. TS-008A/B are complete; TS-008C source review/DR-C01 is delivered with explicit exclusions; TS-008D confirmation/publication remains separate. See [review evidence](review-evidence.md). No full suite/browser/production build/deployment or commit was performed.

## Self-review

Only UnitTests adds a reference to the existing job project. Runtime modules/API do not call jobs. EF projections are read-only, PPE reads are bounded, Sewing contexts share repeatable-read and sequential strict evaluation. Unexpected adapter failures fail the report; source issues and missing identities stay explicit. No formula copy, transport model leak, approval override or writer is introduced. Every production type has one responsibility/topic file. Source capture/hash and selected version IDs remain in the report; it has no credentials or automatic activation authority. Documentation links and final diff checks passed.

Subsequent TS-008D confirmation/publication is [complete locally](execution-evidence.md). Earlier no-write statements above describe the readiness/report phase only.
