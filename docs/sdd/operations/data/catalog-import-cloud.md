# Dedicated Cloud Run catalogue import

[Specification](../../specs/catalog/002-sewing-articles-and-composition/cloud-import.md) · [Importer](catalog-import.md)

The separate job is brevierp-catalog-import in brevistoreapi/europe-central2 by default. It reuses the Host.Seed image, always selects --catalog-import, and does not invoke the normal seed/migration flow. No API endpoint or scheduled automatic import is added. The existing brevierp-seed/API jobs are not modified. The wrapper checks the managed-job label before replacing an existing job.

## Automatic job creation after push

The [deploy workflow](../deployment/cloud-run-deploy.md) creates or updates brevierp-catalog-import after the API deployment using deployment/render-catalog-import-job.py. It uses the new Host.Seed image from the same run, inherits the seed job's network/service account, and references its existing database secret directly. CatalogImport:ApprovedConnection binds the same secret at the configured numeric version so host/database approval is derived only inside Cloud Run; no secret value is read by the workflow or saved locally. The direct wrapper's explicit host/database pins remain supported as an alternative.

The source bucket, frozen SHA256 and approved secret version are explicit workflow `env` values. The current frozen source has been uploaded privately and verified by readback; the job's service account has object-viewer/object-creator access to that bucket. Deployment configures the job but does not execute it. Its default mode is validate. An actual apply remains an explicit execution after target backup; a code deployment is not a data-import receipt.

Each execution writes its own <commit>-{execution}.json report. Host.Seed substitutes the validated CLOUD_RUN_EXECUTION value. Private .local data is excluded from Git and Docker build context. Standalone operator invocation below remains available, but is not required just to create the Job after push.
## Prepare the source

Export the approved Google workbook as native XLSX and keep it in ignored private storage. Convert it without evaluating formulas or contacting embedded links:

```powershell
./deployment/prepare-catalog-source.ps1 -SourceFile <private-workbook.xlsx> -SpreadsheetId <calculator-spreadsheet-id> -OutputFile <new-private-source.json>
```

The converter preserves all22 sheet values and formula evidence and derives model website URLs from the actual workbook. Source row positions, blank cells and native cached value types are retained. It rejects unsupported/malformed XML, macros/external workbook relationships, oversized input and output inside Git unless explicitly ignored. The resulting SHA256 identifies the exact source; no XLSX or source rows enter the code image/Git. Captured dates/cached formulas are source evidence, not a transactional Google backup or approved price publication.

## Configure the dedicated job

Use the immutable Host.Seed image URI ending in @sha256:<digest>, an existing database-connection Secret Manager secret, a dedicated service account, and an existing Cloud Storage bucket with enforced public access prevention and uniform bucket-level access. The account needs read access to source objects, write access to report objects, access to the named database secret and access to the target PostgreSQL network. Cloud SQL targets may additionally pass CloudSqlInstance; private-IP targets may pass VpcConnector. Do not invent the actual database host or secret from the API URL.

The target host/database must exactly correspond to the connection secret; the runtime checks them and the frozen source SHA before opening PostgreSQL. No generic allow-remote bypass exists. Apply additionally requires a reference to a real backup of that target; make and verify the backup before selecting apply. The reference records an operator prerequisite, not proof that the script made a backup.

Build a common parameter map locally without connection strings:

```powershell
$import = @{
  ImageUri = '<immutable-host-seed-image@sha256:digest>'
  DatabaseSecret = '<existing-secret-name>'
  TargetHost = '<database-host-or-cloudsql-socket>'
  TargetDatabase = '<database-name>'
  SourceFile = '<private-source.json>'
  SourceSha256 = '<converter-sha256>'
  Bucket = '<private-bucket>'
  ServiceAccount = '<job-account>@brevistoreapi.iam.gserviceaccount.com'
}
./deployment/import-catalog.ps1 @import -Mode validate
```

Without -Execute this emits a plan and makes no cloud calls. After configuring real values, execute each stage separately, inspect its report and stop on a failure:

```powershell
./deployment/import-catalog.ps1 @import -Mode validate -Execute
./deployment/import-catalog.ps1 @import -Mode trial -Execute
./deployment/import-catalog.ps1 @import -Mode apply -BackupReference '<verified-target-backup-reference>' -Execute
```

The wrapper uploads a content-addressed source with no-clobber and verifies a downloaded copy before deploying. The job has one task, parallelism1 and retries0. A read-only source mount and a separate writable report mount persist the report at a unique gs://bucket/catalog-import/<hash>/reports/<run>.json path. It downloads the report, verifies source/mode/status and prints only safe counts and private artifact locations. GCS-FUSE report directories are created by the runner. Detailed business data stays in the private report and database provenance.

## Completion and recovery

The initial import requires an empty Products catalogue and no prior import batch; existing supported Reference values are reconciled under the same transaction. Trial rolls back all business/provenance writes (sequences may advance). Apply commits after reconciliation. The same hash rerun in any mode checks existing imported values and returns already-applied without duplicates; a changed hash against a nonempty catalogue is rejected. This procedure is not a changed-snapshot merge, CRM history transfer, automatic user import or price activation.

Confirm the cloud list GET /api/v1/products?page=1&pageSize=20&sortBy=name&sortDirection=asc and the persisted source mappings/counts. The admin page /references/products reads that same backend. An exported file, successful local test or script plan is not evidence of a deployed/imported cloud catalogue. Recovery uses the verified target backup in an explicit restore operation; no automatic delete/reset/Down migration is performed.

## Focused checks

```powershell
./deployment/tests/import-catalog.Tests.ps1
dotnet test tests/UnitTests/UnitTests.csproj -c Release --no-restore -m:1 -p:BuildInParallel=false --filter CatalogImport
dotnet test tests/IntegrationTests/IntegrationTests.csproj -c Release --no-restore -m:1 -p:BuildInParallel=false --filter 'FullyQualifiedName~CatalogImportCommandTests|FullyQualifiedName~CatalogImportPersistenceTests'
```

The PostgreSQL tests need BREVIERP_CATALOG_TEST_CONNECTION and create their own isolated database. Script checks mock gcloud and perform no cloud writes. See the dated evidence for actual results rather than assuming the commands were run.
