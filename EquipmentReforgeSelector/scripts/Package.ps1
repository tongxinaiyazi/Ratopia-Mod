[CmdletBinding()]
param(
    [switch]$Build,
    [switch]$Test,
    [string]$RatopiaDir = 'E:\steam\steamapps\common\Ratopia'
)

$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repositoryRoot 'src\EquipmentReforgeSelector\EquipmentReforgeSelector.csproj'
$solutionPath = Join-Path $repositoryRoot 'EquipmentReforgeSelector.sln'
$releaseDll = Join-Path $repositoryRoot 'src\EquipmentReforgeSelector\bin\Release\net472\EquipmentReforgeSelector.dll'
$distPath = Join-Path $repositoryRoot 'dist'
$stagePath = Join-Path $distPath 'package-staging'
$packageBaseName = -join [char[]](0x88C5, 0x5907, 0x91CD, 0x94F8, 0x81EA, 0x9009, 0x5C5E, 0x6027)
$archivePath = Join-Path $distPath ("$packageBaseName-v0.1.2-BepInEx5.zip")

if ($Test) {
    & dotnet test $solutionPath -c Release "/p:RatopiaDir=$RatopiaDir" '/p:InstallAfterBuild=false'
    if ($LASTEXITCODE -ne 0) {
        throw "Release tests failed with exit code $LASTEXITCODE."
    }
}

if ($Build) {
    & dotnet build $projectPath -c Release "/p:RatopiaDir=$RatopiaDir" '/p:InstallAfterBuild=false'
    if ($LASTEXITCODE -ne 0) {
        throw "Release build failed with exit code $LASTEXITCODE."
    }
}

if (-not (Test-Path -LiteralPath $releaseDll -PathType Leaf)) {
    throw "Release plugin DLL is missing: $releaseDll. Run this script with -Build after resolving the build failure."
}

New-Item -ItemType Directory -Path $distPath -Force | Out-Null
if (Test-Path -LiteralPath $stagePath) {
    Remove-Item -LiteralPath $stagePath -Recurse -Force
}
New-Item -ItemType Directory -Path $stagePath | Out-Null

$pluginDirectory = Join-Path $stagePath 'BepInEx\plugins\EquipmentReforgeSelector'
New-Item -ItemType Directory -Path $pluginDirectory -Force | Out-Null
Copy-Item -LiteralPath $releaseDll -Destination (Join-Path $pluginDirectory 'EquipmentReforgeSelector.dll')
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'README.md') -Destination (Join-Path $stagePath 'README.md')

$documentationDirectory = Join-Path $stagePath 'docs'
New-Item -ItemType Directory -Path $documentationDirectory -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'docs\TESTING.md') -Destination (Join-Path $documentationDirectory 'TESTING.md')

$entries = @(Get-ChildItem -LiteralPath $stagePath -Recurse -File | ForEach-Object {
    $_.FullName.Substring($stagePath.Length).TrimStart('\', '/').Replace('\', '/')
})
$allowedEntries = @(
    'BepInEx/plugins/EquipmentReforgeSelector/EquipmentReforgeSelector.dll',
    'README.md',
    'docs/TESTING.md'
)
$forbiddenEntries = @($entries | Where-Object {
    $entry = $_
    $fileName = Split-Path -Leaf $entry
    $entry -match '(?i)(^|/)(bin|obj|save|saves)(/|$)' -or
    $fileName -match '(?i)^(Assembly-CSharp(?:-firstpass)?|0Harmony|Mono.Cecil|BepInEx.*|UnityEngine.*|Harmony.*|.*test.*)\.dll$' -or
    $fileName -match '(?i)\.(pdb|log|sav)$'
})
$unexpectedEntries = @($entries | Where-Object { $_ -notin $allowedEntries })
if ($forbiddenEntries.Count -gt 0 -or $unexpectedEntries.Count -gt 0 -or $entries.Count -ne $allowedEntries.Count) {
    throw "Package staging validation failed. Forbidden: $($forbiddenEntries -join ', '); unexpected: $($unexpectedEntries -join ', ')"
}

if (Test-Path -LiteralPath $archivePath) {
    Remove-Item -LiteralPath $archivePath -Force
}
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory($stagePath, $archivePath, [IO.Compression.CompressionLevel]::Optimal, $false)

Write-Output "Created package: $archivePath"
Write-Output "Staging directory: $stagePath"
