# Standalone offline checks. No gcloud installation, cloud access or database required.
$ErrorActionPreference = 'Stop'
$scriptPath = Join-Path $PSScriptRoot '../import-catalog.ps1'
$temp = Join-Path ([IO.Path]::GetTempPath()) ('brevi-import-wrapper-tests-' + [guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($temp) | Out-Null
$source = Join-Path $temp 'source.json'
[IO.File]::WriteAllText($source, '{"version":1,"spreadsheetId":"synthetic","sheets":{"synthetic":[["header"]]}}')
$hash = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash.ToLowerInvariant()
$base = @{
    ImageUri = 'europe-central2-docker.pkg.dev/brevistoreapi/seed/image@sha256:' + ('a' * 64)
    DatabaseSecret = 'synthetic-db-secret'; TargetHost = 'db.example.internal'; TargetDatabase = 'synthetic'
    SourceFile = $source; SourceSha256 = $hash; Bucket = 'synthetic-private-bucket'
    ServiceAccount = 'import@brevistoreapi.iam.gserviceaccount.com'
    BackupReference = 'backup://synthetic/verified-before-import.dump'
}
$global:ImportWrapperCalls = [Collections.Generic.List[object]]::new()
$global:ImportWrapperFailure = $false
$global:ImportWrapperPublicBucket = $false
$global:ImportWrapperSource = $source
$global:ImportWrapperHash = $hash
$global:ImportWrapperMode = 'validate'
$global:ImportWrapperReportStatus = $null
function global:gcloud {
    $global:ImportWrapperCalls.Add(@($args))
    $global:LASTEXITCODE = 0
    if ($global:ImportWrapperFailure) { $global:LASTEXITCODE = 7; return 'synthetic suppressed failure' }
    if ($args[0] -eq 'storage' -and $args[1] -eq 'buckets') {
        if ($global:ImportWrapperPublicBucket) { return '{"public_access_prevention":"inherited","uniform_bucket_level_access":true}' }
        return '{"public_access_prevention":"enforced","uniform_bucket_level_access":true}'
    }
    if ($args[0] -eq 'run' -and $args[2] -eq 'list') { return '[]' }
    if ($args[0] -eq 'storage' -and $args[1] -eq 'cp' -and $args[2] -like 'gs://*') {
        if ($args[2] -like '*/reports/*') {
            $status = switch ($global:ImportWrapperMode) { 'validate' {'validated'}; 'trial' {'trial-rolled-back'}; 'apply' {'applied'} }
            if ($global:ImportWrapperReportStatus) { $status = $global:ImportWrapperReportStatus }
            @{SnapshotHash=$global:ImportWrapperHash;Mode=$global:ImportWrapperMode;Status=$status;Products=0;Articles=0;Materials=0;Operations=0} |
                ConvertTo-Json | Set-Content -LiteralPath $args[3]
        } else { Copy-Item -LiteralPath $global:ImportWrapperSource -Destination $args[3] }
    }
    return '{}'
}
function Assert-True([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
try {
    $plan = & $scriptPath @base | ConvertFrom-Json
    Assert-True ($plan.Mode -eq 'validate' -and -not $plan.CloudCommandsExecuted) 'Default must be a validate plan.'
    Assert-True ($global:ImportWrapperCalls.Count -eq 0) 'Plan invoked a cloud command.'
    $bad = $base.Clone(); $bad.SourceSha256 = '0' * 64
    $rejected = $false; try { & $scriptPath @bad | Out-Null } catch { $rejected = $true }
    Assert-True $rejected 'Hash mismatch was accepted.'
    Assert-True ($global:ImportWrapperCalls.Count -eq 0) 'Hash mismatch reached gcloud.'
    $bad = $base.Clone(); $bad.Remove('BackupReference')
    $rejected = $false; try { & $scriptPath @bad -Mode apply -Execute | Out-Null } catch { $rejected = $true }
    Assert-True $rejected 'Apply without backup reference was accepted.'
    Assert-True ($global:ImportWrapperCalls.Count -eq 0) 'Missing backup reference reached gcloud.'
    foreach ($mode in @('validate', 'trial', 'apply')) {
        $global:ImportWrapperCalls.Clear(); $global:ImportWrapperMode = $mode
        $result = & $scriptPath @base -Mode $mode -Execute | ConvertFrom-Json
        Assert-True ($result.CloudCommandsExecuted -and $result.Mode -eq $mode) 'Explicit mode was not preserved.'
        $deploy = @($global:ImportWrapperCalls | Where-Object { $_[0] -eq 'run' -and $_[2] -eq 'deploy' })
        Assert-True ($deploy.Count -eq 1) 'Expected only one dedicated deploy.'
        $joined = $deploy[0] -join ' '
        foreach ($required in @('--command=dotnet', 'Host.Seed.dll,--catalog-import=', "--mode=$mode", '--tasks=1', '--parallelism=1', '--max-retries=0', 'readonly=true', 'readonly=false', 'CatalogImport__TargetHost=', 'CatalogImport__SourceSha256=', '--set-secrets=ConnectionStrings__Default=synthetic-db-secret:latest')) {
            Assert-True ($joined.Contains($required)) "Missing job guard: $required"
        }
        Assert-True (-not $joined.Contains('--execute-now')) 'Deploy must not execute before source checks.'
        Assert-True ($result.ReportUri -like 'gs://*/reports/*.json') 'No persistent report URI.'
    }
    $global:ImportWrapperReportStatus = 'already-applied'
    foreach ($mode in @('validate', 'trial')) {
        $global:ImportWrapperMode = $mode
        $result = & $scriptPath @base -Mode $mode -Execute | ConvertFrom-Json
        Assert-True ($result.Status -eq 'already-applied' -and $result.Mode -eq $mode) 'Valid same-source replay report was rejected.'
    }
    $global:ImportWrapperReportStatus = 'unrecognized-outcome'
    $global:ImportWrapperMode = 'validate'
    $rejected = $false; try { & $scriptPath @base -Execute | Out-Null } catch { $rejected = $_.Exception.Message -like '*status does not confirm*' }
    Assert-True $rejected 'An unrecognized report status was accepted.'
    $global:ImportWrapperReportStatus = $null
    $global:ImportWrapperCalls.Clear(); $global:ImportWrapperPublicBucket = $true
    $rejected = $false; try { & $scriptPath @base -Execute | Out-Null } catch { $rejected = $true }
    Assert-True $rejected 'Bucket without enforced privacy was accepted.'
    Assert-True ($global:ImportWrapperCalls.Count -eq 1) 'Public bucket continued to mutation.'
    $global:ImportWrapperPublicBucket = $false; $global:ImportWrapperFailure = $true
    $rejected = $false; try { & $scriptPath @base -Execute | Out-Null } catch { $rejected = $_.Exception.Message.Contains('exit 7') }
    Assert-True $rejected 'External failure was ignored.'
    'Passed: default plan, hash preflight, validate/trial/apply command guards, persistent report, validate/trial already-applied replay, unknown status rejection, private bucket and external failure checks.'
} finally {
    Remove-Item Function:\gcloud -ErrorAction SilentlyContinue
    # Synthetic files remain in the test-specific OS Temp directory for inspection.
}
