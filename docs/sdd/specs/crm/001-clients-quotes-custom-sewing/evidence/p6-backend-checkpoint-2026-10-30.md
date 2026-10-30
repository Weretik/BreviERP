# P6 backend checkpoint — 2026-10-30

## Delivered scope

Three independent contributors and the coordinator implemented two backend slices: TS-017 email-template registry and TS-012 pinned production ZIP API. EN-004 adds Gmail readiness/transport boundaries but remains partial. TS-011 outbox/history and FE-009 are not delivered here. No Git commit, push or deployment was performed; previous P5 work is preserved.

- Registry: Admin creates immutable subject/body versions and explicitly activates one; Admin/Manager read bounded lists/detail/active. Plain-text placeholders are allowlisted and expansion is nonrecursive and bounded. Selection references validate actual registry versions; unchanged legacy references can be retained without authorizing delivery.
- Production: explicit export from exact issued quote/custom versions, saved operation norms and allowlisted wage fields. Private customer/sale/supplier/overhead fields are excluded. Canonical paths and pinned metadata/bytes/MIME/SHA are verified before a bounded deterministic ZIP is persisted. Current access precedes replay comparison; immutable artifact and actor request journal commit together. Archived quotes block new export requests, while existing exact replay and authorized historical reads remain available. No Workshop transport or sent-state mutation occurs.
- Gmail: fail-closed readiness, fixed-endpoint one-attempt adapter, stable Message-ID correlation and conservative lookup reconciliation. Configured test recipient intent is kept only in ignored local `.local/p6/gmail-settings.local.json`; this file is not automatically loaded by the host. No credentials were copied. The registered token source returns unavailable and readiness explicitly reports missing credentials. No live email was sent.

## Verification

- Focused integration run before archive correction: **41 passed, 0 failed, 0 skipped**, including unique disposable PostgreSQL databases. Final regression after the archive guard: **42 passed, 0 failed, 0 skipped**, duration 43 seconds. This includes six production PostgreSQL cases.
- `dotnet test tests/UnitTests/UnitTests.csproj --filter 'FullyQualifiedName~EmailTemplateVersionTests|FullyQualifiedName~ProductionPackageProjectionBuilderTests' --no-restore --nologo --verbosity quiet`: **7 passed**. The second class filter matched no tests; this was detected rather than counted as projection coverage.
- Corrected projection run with `FullyQualifiedName~ProductionPackageProjectionTests --no-build --no-restore`: **6 passed**, covering the final operation-total and aggregate production-file guards. Total focused unit coverage **13 passed**.
- `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --filter FullyQualifiedName~CrmDependencyTests --no-restore --nologo --verbosity quiet`: **1 passed**.
- Unit restore emitted existing NU1903 for Microsoft.OpenApi.YamlReader 2.7.5 / GHSA-7pxr-59rr-hqj2. Dependency remediation is outside this feature patch; the tests passed. No claim of a warning-free overall build is made.

Integration filter: `FullyQualifiedName~EmailTemplateRegistry|FullyQualifiedName~ProductionPackage|FullyQualifiedName~QuoteDeliveryReadinessPolicyTests|FullyQualifiedName~GmailQuoteDeliveryProviderTests|FullyQualifiedName~IntroductionTemplatePersistenceTests` through a wrapper that validates local host + database and keeps the connection string out of output. It exercises registry schema, immutable triggers, reference changes, exact issued source, replay/access/concurrency, archive policy, ZIP integrity/path safety, strict contracts and fake Gmail HTTP.

## Local migration

Verified target: **localhost / brevidb**. CRM-only Host.Seed plan reported exactly:

1. `20261030150013_AddEmailTemplateRegistry`
2. `20261030150923_AddProductionPackages`

Before apply, `pg_dump --format=custom` created `E:/Development/p6-20261030-before-crm.dump` (**752178 bytes**); `pg_restore --list` succeeded (**585 lines**). CRM-only apply reported both pending before and **pendingAfter=[]**. No unrelated seed/import/activation ran. Disposable PostgreSQL test fixtures independently apply/reapply the CRM schema. Down removes immutable history and is unsuitable after use; retain the verified backup and use coordinated restore/forward recovery.

## Remaining handoff

Frontend continues to consume its existing immutable backend SHA. A new user-authorized local backend commit is required before contract synchronization; no uncommitted SHA is presented as provenance. Frontend registry selection/administration and production download UI are still pending. EN-004 needs delegated Gmail OAuth, verified account/sender and a controlled sandbox round-trip. TS-011 must then pin provider/sender/template/PDF and implement the transactional outbox, leases, explicit send/history and unknown-outcome recovery.

Contributor details: [registry](p6-email-template-registry-2026-10-30.md), [production](ts-012-production-projection-2026-10-30.md), [Gmail](en-004-gmail-readiness-2026-10-30.md).

Final git -c core.safecrlf=false diff --check passed. Local Gmail configuration is confirmed ignored by Git.

Follow-up [functional audit and responsibility split](../code-audit/p6-functional-audit-2026-10-30.md) records the later Gmail IOException regression and final checks.
