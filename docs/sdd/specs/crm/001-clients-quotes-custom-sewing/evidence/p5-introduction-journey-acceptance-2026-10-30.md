# P5 saved-introduction backend journey acceptance — 2026-10-30

Scope: SC-I02/03 and SC-D01/02 persisted backend scenario, after the user's P6 deferral of new selection creation. The fixture seeds a saved selection inside its own disposable database; it does not enable or exercise deferred user creation of new selection templates.

## Scenario and result

`tests/IntegrationTests/Crm/Documents/P5IntroductionJourneyTests.cs` exercises general, summer and winter layouts through:

1. Versioned issuer/template creation and explicit activation in the isolated fixture.
2. Persisted featured mapping and owned saved selection, followed by a fresh `IntroductionTemplateService.ApplyAsync` check.
3. `IntroductionQuoteCreator` with the real price-basis store, replay reader, quote mutation store and `CreateQuoteCommandHandler`.
4. `PrepareQuoteCommandHandler` with real document numbering/settings lookup, a pinned issue and the ready transition.
5. `RenderQuotePdfCommandHandler`, the real private artifact store, hosted `QuotePdfWorker` and persisted render source.
6. Reopen the stored quote version and public projection after changing the current catalog source/price from 200 to 275. A new quote uses 275; the old snapshot and rebuilt projection still use 200. Repeated render returns the same artifact ID, content bytes and SHA256. Replaying the original application does not create an additional quote or issue.

The worker's captured public projection has one seasonal page or two general pages with four cards per season. The private price-basis sentinel `PRIVATE_COST_FORMULA_SENTINEL` does not occur in the customer projection.

Fourth case `Oversized_introduction_layout_blocks_ready_without_number_or_issue` uses an otherwise permitted 1001-character document opening. After the coordinator's early layout guard, prepare returns `Invalid` with `introduction_public_layout_too_large`, no issue, no number-sequence row and no assigned quote number. This prevents accepting ready work that the renderer cannot lay out under the public layout policy.

## Verification

The coordinator's final safe local PostgreSQL run rebuilt the integration project and selected:

```powershell
dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter 'FullyQualifiedName~P5IntroductionJourneyTests' --no-restore --nologo --verbosity quiet
```

**4 passed, 0 failed, 0 skipped** on the final integrated code, including early guard/text-group/CSS corrections. `CrmDatabaseFixture` validates loopback PostgreSQL, creates a unique `brevi_crm_test_<uuid>` database, migrates it twice, and drops only that database. No application database rows, mail, seed job or production data were changed by these tests.

Initial contributor compilation succeeded; without `BREVIERP_CRM_TEST_CONNECTION` its three original cases were skipped. That skip was not acceptance. The coordinator subsequently ran all four cases with the safe configured connection, without `--no-build`. Compiler warnings observed during initial compilation were existing Accounting CS0628 and Identity CS8604; no warning arose from the new test source.

This is acceptance coverage for an existing implementation, not a claim of a feature Red test before implementation. The oversized case is a regression for the defect found during the coordinator's layout acceptance.

## Explicit evidence boundary

Catalog availability/pricing ports use controlled current-source fixtures. The worker uses a recording renderer with PDF-signature fixture bytes so the test proves queue, scoped source, projection, persistence, hashing and retry identity independently of Chromium. It does **not** prove real PDF pagination, font rendering, visual quality, actual live Catalog arithmetic, deployed browser interaction or email delivery. Real Chromium outputs and PNG visual review are recorded in the separate ordinary and seasonal PDF acceptance evidence. Actual email delivery remains P6.

No runtime code, schema, shared configuration, commits or push were changed by this contributor.
