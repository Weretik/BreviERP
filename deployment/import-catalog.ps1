# Requires PowerShell 7. No cloud command runs without -Execute.
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidatePattern('^\S+@sha256:[a-fA-F0-9]{64}$')][string]$ImageUri,
    [Parameter(Mandatory)][ValidatePattern('^[A-Za-z0-9_-]+$')][string]$DatabaseSecret,
    [Parameter(Mandatory)][ValidatePattern('^[A-Za-z0-9_./:-]+$')][string]$TargetHost,
    [Parameter(Mandatory)][ValidatePattern('^[A-Za-z0-9_-]+$')][string]$TargetDatabase,
    [Parameter(Mandatory)][string]$SourceFile,
    [Parameter(Mandatory)][ValidatePattern('^[a-fA-F0-9]{64}$')][string]$SourceSha256,
    [Parameter(Mandatory)][ValidatePattern('^[a-z0-9][a-z0-9._-]{1,220}[a-z0-9]$')][string]$Bucket,
    [Parameter(Mandatory)][ValidatePattern('^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.iam\.gserviceaccount\.com$')][string]$ServiceAccount,
    [ValidatePattern('^[a-z][a-z0-9-]{4,61}[a-z0-9]$')][string]$Project = 'brevistoreapi',
    [ValidatePattern('^[a-z]+-[a-z]+[0-9]$')][string]$Region = 'europe-central2',
    [ValidatePattern('^[a-z][a-z0-9-]*catalog-import(?:-[a-z0-9-]+)?$')][string]$Job = 'brevierp-catalog-import',
    [ValidateSet('validate', 'trial', 'apply')][string]$Mode = 'validate',
    [ValidatePattern('^[A-Za-z0-9_-]+:[a-z0-9-]+:[A-Za-z0-9_-]+$')][string]$CloudSqlInstance,
    [ValidatePattern('^[A-Za-z0-9_/-]+$')][string]$VpcConnector,
    [ValidateLength(1, 512)][string]$BackupReference,
    [switch]$Execute
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false

function Invoke-Gcloud([string]$Stage, [string[]]$Arguments) {
    # Capture native output: errors must never echo secret/configuration values.
    $result = & gcloud @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) { throw "Catalog import cloud stage failed: $Stage (exit $LASTEXITCODE)." }
    return ($result | Out-String).Trim()
}

$source = Get-Item -LiteralPath $SourceFile -ErrorAction Stop
if ($source.PSIsContainer -or $source.Length -gt 67108864 -or $source.Length -eq 0) {
    throw 'Source must be an existing version-1 JSON snapshot of at most 64 MiB.'
}
$sourceHash = (Get-FileHash -LiteralPath $source.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
if ($sourceHash -ne $SourceSha256.ToLowerInvariant()) { throw 'Frozen source SHA-256 mismatch.' }
try { $snapshot = Get-Content -LiteralPath $source.FullName -Raw | ConvertFrom-Json -AsHashtable } catch {
    throw 'Source is not a valid version-1 JSON snapshot.'
}
if ($snapshot.version -ne 1 -or $snapshot.sheets -isnot [System.Collections.IDictionary] -or
    [string]::IsNullOrWhiteSpace($snapshot.spreadsheetId)) {
    throw 'Source requires version=1, spreadsheetId and sheets.'
}
if ($Job -in @('brevierp-seed', 'brevierp-api')) { throw 'A dedicated catalogue-import job is required.' }
if ($Mode -eq 'apply' -and [string]::IsNullOrWhiteSpace($BackupReference)) {
    throw 'Apply requires an explicit reference to the independently verified pre-import backup.'
}
if ($BackupReference -match '[\x00-\x1f]') { throw 'Backup reference contains unsupported control characters.' }

$runId = [guid]::NewGuid().ToString('N')
$sourceObject = "catalog-import/$sourceHash/source.json"
$reportObject = "catalog-import/$sourceHash/reports/$runId.json"
$sourceUri = "gs://$Bucket/$sourceObject"
$reportUri = "gs://$Bucket/$reportObject"
$planUri = "gs://$Bucket/catalog-import/$sourceHash/reports/$runId.plan.json"
$reportPath = "/import-output/$runId.json"
$containerArgs = "Host.Seed.dll,--catalog-import=/import-source/$sourceObject,--mode=$Mode,--report=$reportPath"
$targetEnvironment = "CatalogImport__TargetHost=$TargetHost,CatalogImport__TargetDatabase=$TargetDatabase,CatalogImport__SourceSha256=$sourceHash"
$deployArgs = @(
    'run', 'jobs', 'deploy', $Job, "--project=$Project", "--region=$Region", "--image=$ImageUri",
    '--command=dotnet', "--args=$containerArgs", '--tasks=1', '--parallelism=1', '--max-retries=0',
    '--task-timeout=3600s', '--cpu=1', '--memory=1Gi', "--service-account=$ServiceAccount",
    "--set-env-vars=$targetEnvironment", "--set-secrets=ConnectionStrings__Default=${DatabaseSecret}:latest",
    '--clear-volumes', '--clear-volume-mounts',
    "--add-volume=name=source,type=cloud-storage,bucket=$Bucket,readonly=true",
    '--add-volume-mount=volume=source,mount-path=/import-source',
    "--add-volume=name=output,type=cloud-storage,bucket=$Bucket,readonly=false,mount-options=only-dir=catalog-import/$sourceHash/reports",
    '--add-volume-mount=volume=output,mount-path=/import-output',
    '--labels=managed-by=brevi-catalog-import', '--quiet', '--format=json'
)
if ($CloudSqlInstance) { $deployArgs += "--set-cloudsql-instances=$CloudSqlInstance" }
if ($VpcConnector) { $deployArgs += @("--vpc-connector=$VpcConnector", '--vpc-egress=private-ranges-only') }

$plan = [ordered]@{
    Project = $Project; Region = $Region; Job = $Job; ImageUri = $ImageUri; Mode = $Mode
    TargetHost = $TargetHost; TargetDatabase = $TargetDatabase; SourceSha256 = $sourceHash
    SourceUri = $sourceUri; ReportUri = $reportUri; CloudCommandsExecuted = $false
    ServiceAccount = $ServiceAccount; DatabaseSecretId = $DatabaseSecret; BackupReference = $BackupReference
    CloudSqlInstance = $CloudSqlInstance; VpcConnector = $VpcConnector; PlanUri = $planUri
}
if (-not $Execute) { $plan | ConvertTo-Json -Depth 5; return }
if (-not (Get-Command gcloud -ErrorAction SilentlyContinue)) { throw 'Google Cloud CLI is required for -Execute.' }

$bucketInfo = Invoke-Gcloud 'bucket-preflight' @('storage', 'buckets', 'describe', "gs://$Bucket", "--project=$Project", '--format=json') | ConvertFrom-Json
# gcloud storage describes bucket fields in snake_case. Deny inherited/unknown public-access settings.
if ($bucketInfo.public_access_prevention -ne 'enforced' -or $bucketInfo.uniform_bucket_level_access -ne $true) {
    throw 'The existing bucket must enforce public access prevention and uniform bucket-level access.'
}
$existingJobs = @(Invoke-Gcloud 'job-preflight' @('run', 'jobs', 'list', "--project=$Project", "--region=$Region", '--format=json') | ConvertFrom-Json)
foreach ($existing in $existingJobs) {
    if ($existing.metadata.name -eq $Job -and $existing.metadata.labels.'managed-by' -ne 'brevi-catalog-import') {
        throw 'Refusing to replace an existing job not managed as a dedicated catalogue import.'
    }
}

$localOutput = Join-Path ([IO.Path]::GetTempPath()) "brevi-cloud-catalog-import-$runId"
[IO.Directory]::CreateDirectory($localOutput) | Out-Null
$readback = Join-Path $localOutput 'source-readback.json'
$localPlan = Join-Path $localOutput 'plan.json'
$plan | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $localPlan -Encoding utf8NoBOM
Invoke-Gcloud 'upload-source' @('storage', 'cp', '--no-clobber', '--content-type=application/json', $source.FullName, $sourceUri, "--project=$Project", '--quiet') | Out-Null
Invoke-Gcloud 'verify-source-download' @('storage', 'cp', $sourceUri, $readback, "--project=$Project", '--quiet') | Out-Null
if ((Get-FileHash -LiteralPath $readback -Algorithm SHA256).Hash.ToLowerInvariant() -ne $sourceHash) {
    throw 'Remote content-addressed source does not match the frozen SHA-256; job was not deployed.'
}
Invoke-Gcloud 'persist-operation-plan' @('storage', 'cp', '--no-clobber', '--content-type=application/json', $localPlan, $planUri, "--project=$Project", '--quiet') | Out-Null
Invoke-Gcloud 'deploy-dedicated-job' $deployArgs | Out-Null
Invoke-Gcloud 'execute-dedicated-job' @('run', 'jobs', 'execute', $Job, "--project=$Project", "--region=$Region", '--wait', '--quiet', '--format=json') | Out-Null
$localReport = Join-Path $localOutput 'report.json'
Invoke-Gcloud 'download-report' @('storage', 'cp', $reportUri, $localReport, "--project=$Project", '--quiet') | Out-Null
try { $report = Get-Content -LiteralPath $localReport -Raw | ConvertFrom-Json } catch { throw 'Import report is not valid JSON.' }
if ($report.SnapshotHash -ne $sourceHash -or $report.Mode -ne $Mode) { throw 'Import report source or mode mismatch.' }
$acceptedStatuses = switch ($Mode) { 'validate' { @('validated', 'already-applied') }; 'trial' { @('trial-rolled-back', 'already-applied') }; 'apply' { @('applied', 'already-applied') } }
if ($report.Status -notin $acceptedStatuses) { throw 'Import report status does not confirm the requested operation.' }
# Detailed reports can contain business data; emit only safe counts and private artifact locations.
[ordered]@{
    Project = $Project; Job = $Job; Mode = $Mode; Status = $report.Status
    SourceSha256 = $sourceHash; Products = $report.Products; Articles = $report.Articles
    Materials = $report.Materials; Operations = $report.Operations
    ReportUri = $reportUri; LocalReport = $localReport; PlanUri = $planUri; CloudCommandsExecuted = $true
} | ConvertTo-Json -Depth 5
