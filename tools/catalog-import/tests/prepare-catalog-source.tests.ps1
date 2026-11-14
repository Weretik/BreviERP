$ErrorActionPreference = 'Stop'
$converter = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\prepare-catalog-source.ps1'))
Add-Type -AssemblyName System.IO.Compression.FileSystem
$fixtureRoot = Join-Path ([IO.Path]::GetTempPath()) ('brevi-source-converter-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixtureRoot | Out-Null
$passed = 0
function Assert-Condition([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
function Write-Fixture([string]$Name, [string]$SheetXml, [string]$ExtraName = '', [string]$ExtraContents = '') {
    $path = Join-Path $fixtureRoot ($Name + '.xlsx')
    $zip = [IO.Compression.ZipFile]::Open($path, [IO.Compression.ZipArchiveMode]::Create)
    try {
        $parts = @{
            'xl/workbook.xml' = '<workbook xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets><sheet name="Товари" r:id="rId1"/></sheets></workbook>'
            'xl/_rels/workbook.xml.rels' = '<Relationships><Relationship Id="rId1" Target="worksheets/sheet1.xml"/></Relationships>'
            'xl/worksheets/sheet1.xml' = $SheetXml
        }
        if ($ExtraName -ne '') { $parts[$ExtraName] = $ExtraContents }
        foreach ($entry in $parts.GetEnumerator()) {
            $writer = [IO.StreamWriter]::new($zip.CreateEntry($entry.Key).Open(), [Text.UTF8Encoding]::new($false))
            try { $writer.Write($entry.Value) } finally { $writer.Dispose() }
        }
    } finally { $zip.Dispose() }
    return $path
}
function Expect-Rejected([string]$Source, [string]$Label, [string]$Output = '') {
    if ($Output -eq '') { $Output = Join-Path $fixtureRoot ($Label + '.json') }
    $rejected = $false
    try { & $converter -SourceFile $Source -SpreadsheetId 'synthetic' -OutputFile $Output | Out-Null } catch { $rejected = $true }
    Assert-Condition $rejected ('Expected rejection: ' + $Label)
    $script:passed++
}
$valid = @'
<worksheet><sheetData>
<row r="1"><c r="A1" t="inlineStr"><is><t>Code</t></is></c></row>
<row r="3"><c r="A3" t="inlineStr"><is><t>MOD-TEST</t></is></c><c r="B3"><v>1.25</v></c><c r="F3" t="b"><v>0</v></c><c r="H3"><v>46273</v></c><c r="I3"><v>2.0</v></c><c r="J3"><v>1.0</v></c><c r="M3" t="inlineStr"><is><t>002</t></is></c><c r="K3" t="str"><f>HYPERLINK("https://brevi.com.ua/test","Open")</f><v>Open</v></c><c r="L3"><f>1+1</f><v>2</v></c></row>
</sheetData></worksheet>
'@
$source = Write-Fixture 'valid' $valid
$output = Join-Path $fixtureRoot 'valid.json'
& $converter -SourceFile $source -SpreadsheetId 'synthetic' -OutputFile $output | Out-Null
$result = Get-Content -LiteralPath $output -Raw | ConvertFrom-Json
Assert-Condition ($result.version -eq 1 -and @($result.sheets.PSObject.Properties).Count -eq 1) 'Snapshot schema or sheet count mismatch'
Assert-Condition ($result.sheets.'Товари'.Count -eq 3 -and $result.sheets.'Товари'[1].Count -eq 0) 'Row hole was lost'
Assert-Condition ($result.sheets.'Товари'[2][1] -eq 1.25 -and $result.sheets.'Товари'[2][5] -is [bool] -and $result.sheets.'Товари'[2][5] -eq $false) 'Numeric/boolean types were lost'
Assert-Condition ($result.sheets.'Товари'[2][7] -eq 46273 -and $result.formulas.'Товари'[2][11] -eq '=1+1') 'Date serial or literal formula was not preserved'
Assert-Condition ($result.modelWebsiteUrls.'MOD-TEST' -eq 'https://brevi.com.ua/test') 'Current hyperlink was not recovered'
$passed++
$document = [System.Text.Json.JsonDocument]::Parse([string](Get-Content -LiteralPath $output -Raw))
try {
    $cells = $document.RootElement.GetProperty('sheets').GetProperty('Товари')[2]
    Assert-Condition ($cells[8].GetRawText() -eq '2' -and $cells[9].GetRawText() -eq '1') 'Integral numeric source identities must have canonical integer JSON tokens'
    Assert-Condition ($cells[12].GetString() -eq '002') 'Leading zero string identity must remain untouched'
} finally { $document.Dispose() }
$passed++
Expect-Rejected $source 'overwrite' $output
Expect-Rejected (Write-Fixture 'dtd' ('<!DOCTYPE worksheet [<!ENTITY x SYSTEM "file:///unread-secret">]>' + $valid)) 'dtd'
Expect-Rejected (Write-Fixture 'macro' $valid 'xl/vbaProject.bin' 'synthetic') 'macro'
Expect-Rejected (Write-Fixture 'external' $valid 'xl/externalLinks/externalLink1.xml' '<externalLink/>') 'external'
Expect-Rejected (Write-Fixture 'bad-number' ($valid.Replace('<v>1.25</v>', '<v>1,25</v>'))) 'bad-number'
Expect-Rejected (Write-Fixture 'duplicate-cell' ($valid.Replace('<c r="B3">', '<c r="A3">'))) 'duplicate-cell'
Expect-Rejected (Write-Fixture 'bad-row' ($valid.Replace('<row r="3">', '<row r="2">'))) 'bad-row'
Expect-Rejected (Write-Fixture 'no-hyperlink' ($valid.Replace('HYPERLINK("https://brevi.com.ua/test","Open")', '1+1'))) 'no-hyperlink'
Expect-Rejected (Write-Fixture 'http-hyperlink' ($valid.Replace('https://brevi.com.ua/test', 'http://brevi.com.ua/test'))) 'http-hyperlink'
Expect-Rejected (Write-Fixture 'uncached-required' ($valid.Replace('<c r="B3"><v>1.25</v></c>', '<c r="B3"><f>1+1</f></c>'))) 'uncached-required'
$outsideIgnored = Join-Path ([IO.Path]::GetDirectoryName($converter)) ('synthetic-private-source-' + [guid]::NewGuid().ToString('N') + '.json')
Expect-Rejected $source 'git-output' $outsideIgnored
[ordered]@{ passed = $passed; failed = 0; fixtureRoot = $fixtureRoot } | ConvertTo-Json
