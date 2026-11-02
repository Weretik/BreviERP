[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$SourceFile,
    [Parameter(Mandatory)][ValidatePattern('^[A-Za-z0-9_-]+$')][string]$SpreadsheetId,
    [Parameter(Mandatory)][string]$OutputFile,
    [DateTimeOffset]$CapturedAt = [DateTimeOffset]::UtcNow
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$sourcePath = (Resolve-Path -LiteralPath $SourceFile).Path
$outputPath = [IO.Path]::GetFullPath($OutputFile)
if ([IO.Path]::GetExtension($sourcePath) -ne '.xlsx' -or (Get-Item -LiteralPath $sourcePath).Length -gt 64MB) { throw 'A bounded native XLSX export is required.' }
if ([IO.Path]::GetExtension($outputPath) -ne '.json' -or (Test-Path -LiteralPath $outputPath)) { throw 'Output must be a new JSON file.' }
if (-not (Test-Path -LiteralPath ([IO.Path]::GetDirectoryName($outputPath)) -PathType Container)) { throw 'Output directory must already exist in private ignored storage.' }
$ancestor = [IO.DirectoryInfo]::new([IO.Path]::GetDirectoryName($outputPath))
while ($null -ne $ancestor) {
    if (Test-Path -LiteralPath (Join-Path $ancestor.FullName '.git')) {
        & git -C $ancestor.FullName check-ignore --quiet -- $outputPath 2>$null
        if ($LASTEXITCODE -ne 0) { throw 'Private source output must be outside Git or explicitly ignored.' }
        break
    }
    $ancestor = $ancestor.Parent
}
$archive = [IO.Compression.ZipFile]::OpenRead($sourcePath)
$temporary = $outputPath + '.' + [guid]::NewGuid().ToString('N') + '.tmp'

function Read-ArchiveXml([string]$Name) {
    $entry = $archive.GetEntry($Name)
    if ($null -eq $entry) { return $null }
    if ($entry.Length -gt 64MB) { throw 'XLSX XML entry exceeds the size limit.' }
    $settings = [Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $settings.MaxCharactersInDocument = 64MB
    $stream = $entry.Open()
    $reader = [Xml.XmlReader]::Create($stream, $settings)
    try {
        $xml = [Xml.XmlDocument]::new()
        $xml.XmlResolver = $null
        $xml.Load($reader)
        return ,$xml
    } finally { $reader.Dispose(); $stream.Dispose() }
}

function Get-CellColumn([string]$Address) {
    if ($Address -cnotmatch '^[A-Z]{1,3}[1-9][0-9]{0,6}$') { throw 'Invalid XLSX cell address.' }
    $number = 0
    foreach ($letter in $Address.ToCharArray()) {
        if ($letter -ge 'A' -and $letter -le 'Z') { $number = $number * 26 + ([int]$letter - 64) } else { break }
    }
    if ($number -gt 256) { throw 'XLSX column limit exceeded.' }
    return $number - 1
}

function Get-CellText($Node) {
    if ($null -eq $Node) { return '' }
    return (($Node.SelectNodes('.//*[local-name()="t"]') | ForEach-Object { $_.InnerText }) -join '')
}

function Resolve-EntryName([string]$Parent, [string]$Target) {
    if ($Target -match '[\\:]|(^|/)\.\.(/|$)' -or $Target.Contains('?') -or $Target.Contains('#')) { throw 'Unsupported XLSX part target.' }
    $name = if ($Target.StartsWith('/')) { $Target.TrimStart('/') } else { $Parent + $Target }
    if (-not $name.StartsWith('xl/', [StringComparison]::Ordinal)) { throw 'XLSX part escaped the workbook.' }
    return $name
}

function Add-ModelUrl([string]$Key, [string]$Url) {
    $uri = $null
    if (-not [Uri]::TryCreate($Url, [UriKind]::Absolute, [ref]$uri) -or $uri.Scheme -ne 'https' -or $uri.UserInfo -ne '') { throw 'Model hyperlink must be an absolute HTTPS URL without credentials.' }
    if ($modelUrls.Contains($Key) -and $modelUrls[$Key] -ne $uri.AbsoluteUri) { throw 'Conflicting model hyperlinks.' }
    $modelUrls[$Key] = $uri.AbsoluteUri
}

try {
    if ($archive.Entries.Count -gt 2048) { throw 'XLSX entry count exceeds the limit.' }
    $entryNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    [long]$totalLength = 0
    foreach ($entry in $archive.Entries) {
        if (-not $entryNames.Add($entry.FullName)) { throw 'Duplicate XLSX part.' }
        $totalLength += $entry.Length
        if ($totalLength -gt 256MB -or $entry.Length -gt 64MB) { throw 'XLSX expanded size exceeds the limit.' }
        if ($entry.FullName -match '(?i)vbaProject|xl/externalLinks/') { throw 'Macro or linked-workbook packages are not supported.' }
    }
    $shared = [Collections.Generic.List[string]]::new()
    $strings = Read-ArchiveXml 'xl/sharedStrings.xml'
    if ($null -ne $strings) { foreach ($item in $strings.SelectNodes('//*[local-name()="si"]')) { $shared.Add((Get-CellText $item)) } }
    $book = Read-ArchiveXml 'xl/workbook.xml'
    $relations = Read-ArchiveXml 'xl/_rels/workbook.xml.rels'
    if ($null -eq $book -or $null -eq $relations) { throw 'Workbook structure is missing.' }
    $targets = @{}
    foreach ($relation in $relations.DocumentElement.ChildNodes) {
        if ($targets.ContainsKey([string]$relation.Id) -or [string]$relation.TargetMode -eq 'External') { throw 'Invalid workbook part relationship.' }
        $targets[[string]$relation.Id] = Resolve-EntryName 'xl/' ([string]$relation.Target)
    }
    $sheets = [ordered]@{}
    $formulas = [ordered]@{}
    $modelUrls = [ordered]@{}
    $counts = [Collections.Generic.List[object]]::new()
    $missingFormulaCaches = 0
    foreach ($sheet in $book.SelectNodes('//*[local-name()="sheet"]')) {
        if ($sheets.Count -ge 64) { throw 'Sheet count exceeds the limit.' }
        $name = [string]$sheet.name
        if ($name -eq 'Инструкция новый товар') { $name = 'Инструкция: новый товар' }
        if ($sheets.Contains($name)) { throw 'Duplicate canonical worksheet name.' }
        $rid = $sheet.GetAttribute('id', 'http://schemas.openxmlformats.org/officeDocument/2006/relationships')
        if (-not $targets.ContainsKey($rid)) { throw 'Worksheet relationship is missing.' }
        $partName = $targets[$rid]
        $xml = Read-ArchiveXml $partName
        if ($null -eq $xml) { throw 'Worksheet part is missing.' }
        $matrix = [Collections.Generic.List[object]]::new()
        $formulaMatrix = [Collections.Generic.List[object]]::new()
        $previousRow = -1
        $formulaCount = 0
        foreach ($row in $xml.SelectNodes('//*[local-name()="sheetData"]/*[local-name()="row"]')) {
            $rowIndex = [int]::Parse([string]$row.r, [Globalization.CultureInfo]::InvariantCulture) - 1
            if ($rowIndex -le $previousRow -or $rowIndex -gt 100000) { throw 'Invalid worksheet row order or row limit.' }
            $previousRow = $rowIndex
            $values = [Collections.Generic.List[object]]::new()
            $formulaValues = [Collections.Generic.List[object]]::new()
            $hasData = $false
            $seenColumns = [Collections.Generic.HashSet[int]]::new()
            $uncachedColumns = [Collections.Generic.HashSet[int]]::new()
            foreach ($cell in $row.SelectNodes('./*[local-name()="c"]')) {
                $address = [string]$cell.r
                $column = Get-CellColumn $address
                if ([int]($address -replace '^[A-Z]+', '') -ne $rowIndex + 1 -or -not $seenColumns.Add($column)) { throw 'Invalid or duplicate worksheet cell.' }
                while ($values.Count -le $column) { $values.Add(''); $formulaValues.Add('') }
                $cached = $cell.SelectSingleNode('./*[local-name()="v"]')
                $formula = $cell.SelectSingleNode('./*[local-name()="f"]')
                $value = ''
                switch ([string]$cell.t) {
                    's' { if ($cached) { $index = [int]$cached.InnerText; if ($index -lt 0 -or $index -ge $shared.Count) { throw 'Invalid shared string index.' }; $value = $shared[$index] } }
                    'inlineStr' { $value = Get-CellText $cell }
                    'b' { if ($cached) { if ($cached.InnerText -notin @('0', '1')) { throw 'Invalid boolean cell.' }; $value = $cached.InnerText -eq '1' } }
                    'e' { throw 'Workbook contains a cached formula error; repair the source before import.' }
                    'str' { if ($cached) { $value = $cached.InnerText } }
                    default {
                        if ($cached -and $cached.InnerText -ne '') {
                            $number = [decimal]0
                            if (-not [decimal]::TryParse($cached.InnerText, [Globalization.NumberStyles]::Float, [Globalization.CultureInfo]::InvariantCulture, [ref]$number)) { throw 'Unsupported numeric cell.' }
                            # Numeric identifiers must serialize as 2, not 2.0: WorkbookRow.Text uses the raw JSON token.
                            # Only numeric cells are canonicalized; string identities retain leading zeroes exactly.
                            $value = if ([decimal]::Truncate($number) -eq $number -and $number -ge [long]::MinValue -and $number -le [long]::MaxValue) { [long]$number } else { $number }
                        }
                    }
                }
                $values[$column] = $value
                $formulaValues[$column] = $value
                if ($null -ne $formula) {
                    $formulaValues[$column] = '=' + $formula.InnerText
                    $formulaCount++
                    if ($null -eq $cached) { $missingFormulaCaches++; [void]$uncachedColumns.Add($column) }
                }
                if ($null -ne $formula -or $value -isnot [string] -or $value -ne '') { $hasData = $true }
            }
            if ($rowIndex -gt 0) {
                $keyColumn = if ($name -eq 'Трудовитрати виробів') { 1 } else { 0 }
                if ($values.Count -gt $keyColumn -and [string]$values[$keyColumn] -ne '') {
                    $critical = switch ($name) {
                        'Товари' { @(0, 1, 2, 3, 4) }
                        'Комплектація виробів' { @(0, 6, 8, 9) }
                        'Трудовитрати виробів' { @(1, 7) }
                        'Прайс на сайт' { @(0, 1, 2, 5) }
                        default { @() }
                    }
                    foreach ($column in $critical) { if ($uncachedColumns.Contains($column)) { throw 'A business-critical formula has no cached result.' } }
                }
            }
            if ($hasData) {
                while ($matrix.Count -lt $rowIndex) { $matrix.Add(@()); $formulaMatrix.Add(@()) }
                $matrix.Add($values.ToArray()); $formulaMatrix.Add($formulaValues.ToArray())
            }
        }
        $sheets[$name] = $matrix.ToArray()
        $formulas[$name] = $formulaMatrix.ToArray()
        if ($name -eq 'Товари') {
            for ($index = 1; $index -lt $matrix.Count; $index++) {
                $values = $matrix[$index]
                if ($values.Count -eq 0 -or [string]$values[0] -eq '') { continue }
                if ($formulaMatrix[$index].Count -gt 10) {
                    $literal = [string]$formulaMatrix[$index][10]
                    if ($literal -match '^(?i)=HYPERLINK\("((?:[^"]|"")+)"\s*[,;]') { Add-ModelUrl ([string]$values[0]) ($Matches[1].Replace('""', '"')) }
                }
            }
            $linkRelations = Read-ArchiveXml ([IO.Path]::GetDirectoryName($partName).Replace('\', '/') + '/_rels/' + [IO.Path]::GetFileName($partName) + '.rels')
            $linksById = @{}
            if ($linkRelations) { foreach ($link in $linkRelations.DocumentElement.ChildNodes) { $linksById[[string]$link.Id] = [string]$link.Target } }
            foreach ($link in $xml.SelectNodes('//*[local-name()="hyperlink"]')) {
                $reference = [string]$link.ref
                if ($reference -notmatch '^K([1-9][0-9]*)$') { continue }
                $index = [int]$Matches[1] - 1
                $id = $link.GetAttribute('id', 'http://schemas.openxmlformats.org/officeDocument/2006/relationships')
                if ($index -gt 0 -and $index -lt $matrix.Count -and $matrix[$index].Count -gt 0 -and [string]$matrix[$index][0] -ne '' -and $linksById.ContainsKey($id)) { Add-ModelUrl ([string]$matrix[$index][0]) $linksById[$id] }
            }
            foreach ($values in $matrix | Select-Object -Skip 1) { if ($values.Count -gt 0 -and [string]$values[0] -ne '' -and -not $modelUrls.Contains([string]$values[0])) { throw 'A source model has no recoverable current HTTPS website URL.' } }
        }
        $counts.Add([ordered]@{ sheet = $name; rows = $matrix.Count; formulaCells = $formulaCount })
    }
    $snapshot = [ordered]@{
        version = 1; spreadsheetId = $SpreadsheetId; capturedAt = $CapturedAt.ToString('o')
        sheets = $sheets; modelWebsiteUrls = $modelUrls; formulas = $formulas
        capture = [ordered]@{ method = 'Native XLSX cached values, literal formulas and current model hyperlinks'; sourceSha256 = (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash; transactionConsistent = $false; dates = 'Numeric Excel serials preserved as source evidence'; missingFormulaCaches = $missingFormulaCaches }
    }
    [IO.File]::WriteAllText($temporary, ($snapshot | ConvertTo-Json -Depth 40), [Text.UTF8Encoding]::new($false))
    [IO.File]::Move($temporary, $outputPath, $false)
    [ordered]@{ outputFile = $outputPath; bytes = (Get-Item -LiteralPath $outputPath).Length; sha256 = (Get-FileHash -LiteralPath $outputPath -Algorithm SHA256).Hash; sheets = $sheets.Count; modelWebsiteUrls = $modelUrls.Count; missingFormulaCaches = $missingFormulaCaches } | ConvertTo-Json
} finally {
    $archive.Dispose()
    if (Test-Path -LiteralPath $temporary) { Remove-Item -LiteralPath $temporary }
}
