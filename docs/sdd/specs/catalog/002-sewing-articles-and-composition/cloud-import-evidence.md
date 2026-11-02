# Dedicated cloud import verification — 2026-11-02

Scope: implement a separate Cloud Run catalogue-import job, prepare the available current Google workbook, and verify the existing importer with explicit remote-target/source approval. Three bounded agent packages owned source preparation, target guard and deployment wrapper; coordinator owned specification, Host.Seed integration, current-source proof and shared documentation. No new module/project/schema/HTTP operation.

## Delivered behavior

The dedicated brevierp-catalog-import job uses the existing Host.Seed image with explicit import arguments. The deployment wrapper pins an immutable image and frozen source, uploads/verifies a content-addressed source, requires a private bucket, service account and existing connection secret, sets one task/no retries, and persists operator plan/report. Source is mounted read-only; the writable report mount is scoped to its report prefix. Apply requires a backup reference; creation/verification of the actual backup is an operator prerequisite, not an invented script result. Ordinary seed/API deployment and recurring reminders are not modified.

The runtime retains local-only defaults. Explicit host/database/hash approval is checked before opening PostgreSQL; invalid/incomplete pins fail. Missing import mode with target hints fails before service resolution, rather than falling through to migrations. Reports cannot replace their source. Existing initial-empty/provenance guards, transaction, same-hash reconciliation and changed-snapshot rejection remain.

## Actual current source proof

All22 tabs and124 model hyperlinks were freshly derived from the native approved Google export. Corrected source SHA256=24EA80BD240F95D468396A1C1C87C7CFBA31058B422D0B4012CBB4825F5FF687;24309063bytes. Full payload stays in ignored private storage. Integral numeric cells are emitted as integer JSON tokens, fractional values retain decimal precision, and string keys/leading zeroes are preserved. This avoids mismatching numeric SKU2.0 with related string SKU2.

A real-source operational proof created a uniquely named isolated local PostgreSQL database, applied only its Reference/Catalog/import-provenance migrations, then used the actual importer with explicit target/source pins. Results: validated → trial-rolled-back (zero Products after trial) → applied → already-applied. Final accounting243Products,248Articles,1448material rows,1824operation rows. The temporary database was removed after contexts/pools were closed. Shared brevidb writes0; cloud writes0. This proves the importer/data path, not a live Cloud Run import.

## Red/Green and regression

- Target guard: validRed4failed/19passed, Green23passed; final targeted guard+existing source tests32passed/0failed/0skipped (28target,4source).
- Host integration: validRed3failed/4passed showed target hints falling through to normal service resolution. After dispatcher guard, command+PostgreSQL persistence regression8passed/0failed/0skipped. Trial/apply/replay and changed-source refusal use an isolated synthetic fixture database.
- Source converter:13synthetic cases passed, including raw integer token regression. The initial real-source proof failed identity/parent reconciliation on numeric2.0 tokens; after validRed/fix/Green, full native preparation and the real PostgreSQL proof passed. Earlier compile/path corrections are not counted as behavior Red.
- Cloud wrapper: offline mock checks passed for zero cloud calls by default, hash preflight, backup requirement, three modes, private bucket, external failure, durable reports and unknown-status rejection. A validRed reproduced rejecting already-applied in validate; Green accepts safe same-hash replay in validate/trial/apply.
- PowerShell syntax, changed documentation links and git diff --check passed. Source prep and private data remain outside tracked files. No full suite or container build was required: no image project/restore input changed; focused Host.Seed-dependent integration build succeeded.

Commands: dotnet test tests/UnitTests/UnitTests.csproj -c Release --no-restore -m:1 -p:BuildInParallel=false --filter CatalogImport; dotnet test tests/IntegrationTests/IntegrationTests.csproj -c Release --no-restore -m:1 -p:BuildInParallel=false --filter "FullyQualifiedName~CatalogImportCommandTests|FullyQualifiedName~CatalogImportPersistenceTests"; deployment/tests/import-catalog.Tests.ps1; deployment/tests/prepare-catalog-source.tests.ps1. The actual-source proof is a private temporary harness invoking the same public importer; no raw workbook is copied into test fixtures/Git.

## Initial deployment checkpoint

No cloud bucket/job/database mutation, credential change, migration against shared data, email, commit or push was executed. Google Cloud CLI is not available in this local shell; real image digest, bucket, database secret/host/name and service account have not been read from the cloud. The [runbook](../../../operations/data/catalog-import-cloud.md) and wrapper contain the concrete deployment/run procedure; local tests must not be represented as an already-created Cloud Run Job or populated cloud catalogue. Changed-snapshot merge, D1 historical import and price publication retain their separate workflows.

## Automatic Cloud Build provisioning follow-up

The owner clarified that a successful push/build must create the new job from project configuration. cloudbuild.yaml now includes a create/update step after API deployment; the renderer uses the current Host.Seed build image and the existing seed job's connectivity/account. The direct database secret reference is retained, while a second reference pins version1 for approval inside Cloud Run. Admin/bot credentials are excluded from the new job. No plaintext connection secret was extracted: automatic approval rejected saving one locally, and direct secret references were used instead.

The installed Google SDK was subsequently located outside PATH and its authenticated account/project access verified. A private brevistoreapi-catalog-import bucket was created with uniform access and public access prevention. The current24EA...F687 frozen source was uploaded and its SHA256 verified after download. The existing runtime service account received objectViewer/objectCreator on only that bucket. No business DB write or import execution occurred. Job provisioning itself remains pending a successful Cloud Build on the changed code; no commit/push was executed in this follow-up.

Fresh checks: renderer8/8passed (including dedicated1Gi memory; validRed1failed thenGreen), actual seed JSON rendering passed without secret-value reads; Cloud Build YAML safe-parsed and create-only/no-execute step assertions passed. Host command+isolated PostgreSQL regression18passed/0failed/0skipped, including secret-bound approval, missing-mode guards and7new execution-report cases. Report-path validRed6failed thenGreen. Previous32target/source cases remain unchanged. Existing standalone wrapper/source tests also remain applicable. .dockerignore now excludes private .local artifacts.
