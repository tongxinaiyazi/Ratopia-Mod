[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$ReleaseDir
)

$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
if ([string]::IsNullOrWhiteSpace($ReleaseDir)) {
    $ReleaseDir = Join-Path $projectRoot 'NexusRelease\v0.1.2'
}
$releaseRoot = [IO.Path]::GetFullPath($ReleaseDir)
$expectedVersion = '0.1.2'
$sourceZip = Join-Path $projectRoot 'dist\超级弓箭.zip'
$releaseZip = Join-Path $releaseRoot 'files\SuperBow-v0.1.2-BepInEx5.zip'
$coverPath = Join-Path $releaseRoot 'images\SuperBow-Cover-1280x720.png'
$iconPath = Join-Path $releaseRoot 'images\WoodBow-Original-100x100.png'
$metadataPath = Join-Path $releaseRoot 'metadata.json'
$hashManifestPath = Join-Path $releaseRoot 'SHA256SUMS.txt'
$buildDll = Join-Path $projectRoot 'src\SuperBow\bin\Release\net472\SuperBow.dll'

if (-not $releaseRoot.StartsWith(
        $projectRoot + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "Nexus 发布目录不在项目目录内：$releaseRoot"
}

$requiredRelativeFiles = @(
    'README.md',
    'metadata.json',
    'NEXUS_TITLE.txt',
    'NEXUS_SUMMARY.txt',
    'NEXUS_DESCRIPTION.txt',
    'FILE_DESCRIPTION.txt',
    'CHANGELOG.txt',
    'CREDITS_AND_PERMISSIONS.md',
    'UPLOAD_CHECKLIST.md',
    'SHA256SUMS.txt',
    'files\SuperBow-v0.1.2-BepInEx5.zip',
    'images\SuperBow-Cover-1280x720.png',
    'images\WoodBow-Original-100x100.png'
)

foreach ($relativePath in $requiredRelativeFiles) {
    $path = Join-Path $releaseRoot $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "缺少 Nexus 发布资源：$relativePath"
    }
    if ((Get-Item -LiteralPath $path).Length -le 0) {
        throw "Nexus 发布资源为空：$relativePath"
    }
}

$metadata = Get-Content -LiteralPath $metadataPath -Raw | ConvertFrom-Json
if ($metadata.version -ne $expectedVersion) {
    throw "metadata.json 版本不正确：$($metadata.version)"
}
if ($metadata.main_file -ne 'files/SuperBow-v0.1.2-BepInEx5.zip') {
    throw "metadata.json 主文件路径不正确：$($metadata.main_file)"
}
if ($metadata.cover -ne 'images/SuperBow-Cover-1280x720.png') {
    throw "metadata.json 封面路径不正确：$($metadata.cover)"
}
if ($metadata.ai_assistance_disclosure -ne $true) {
    throw 'metadata.json 必须记录 AI 协助披露。'
}
if ($metadata.nexus_2026_anniversary_event_eligible -ne $false) {
    throw '包含 AI 协助的发布资源不得标记为 2026 Nexus 周年活动适用。'
}

$title = (Get-Content -LiteralPath (Join-Path $releaseRoot 'NEXUS_TITLE.txt') -Raw).Trim()
$summary = (Get-Content -LiteralPath (Join-Path $releaseRoot 'NEXUS_SUMMARY.txt') -Raw).Trim()
$description = Get-Content -LiteralPath (Join-Path $releaseRoot 'NEXUS_DESCRIPTION.txt') -Raw
if ($title -ne $metadata.title) {
    throw '英文标题与 metadata.json 不一致。'
}
if ($summary -ne $metadata.summary) {
    throw '英文简介与 metadata.json 不一致。'
}
if ($summary.Length -gt 250) {
    throw "英文简介超过 250 字符：$($summary.Length)"
}
foreach ($requiredText in @(
        'Splash Damage',
        'Bleed',
        'AnimalBody',
        'MapObj',
        'EnemyNexus',
        'BepInEx/plugins/SuperBow/SuperBow.dll',
        'BloodDrain=3',
        'RangeAtk=1',
        '© Cassel Games')) {
    if (-not $description.Contains($requiredText)) {
        throw "Nexus 页面正文缺少必要内容：$requiredText"
    }
}

$textFiles = Get-ChildItem -LiteralPath $releaseRoot -File |
    Where-Object { $_.Extension -in @('.txt', '.md', '.json') }
foreach ($file in $textFiles) {
    $content = Get-Content -LiteralPath $file.FullName -Raw
    if ($content -match '0\.1\.1') {
        throw "发布文本包含旧版本号 0.1.1：$($file.Name)"
    }
    if ($content -match '(?im)\b(TBD|TODO)\b|<placeholder>') {
        throw "发布文本包含占位内容：$($file.Name)"
    }
    if ($content -match '(?i)[A-Z]:\\.*SaveFile|AppData\\LocalLow') {
        throw "发布文本包含本机存档路径：$($file.Name)"
    }
}

Add-Type -AssemblyName System.Drawing
function Assert-ImageDimensions {
    param(
        [string]$Path,
        [int]$Width,
        [int]$Height
    )
    $image = [Drawing.Image]::FromFile($Path)
    try {
        if ($image.Width -ne $Width -or $image.Height -ne $Height) {
            throw "图片尺寸不正确：$Path；实际 $($image.Width)x$($image.Height)，预期 ${Width}x${Height}"
        }
    }
    finally {
        $image.Dispose()
    }
}
Assert-ImageDimensions -Path $coverPath -Width 1280 -Height 720
Assert-ImageDimensions -Path $iconPath -Width 100 -Height 100

foreach ($path in @($sourceZip, $releaseZip, $buildDll)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "缺少哈希校验文件：$path"
    }
}
$sourceZipHash = (Get-FileHash -LiteralPath $sourceZip -Algorithm SHA256).Hash
$releaseZipHash = (Get-FileHash -LiteralPath $releaseZip -Algorithm SHA256).Hash
if ($sourceZipHash -ne $releaseZipHash) {
    throw "Nexus 主文件与 dist 原包哈希不一致：$sourceZipHash != $releaseZipHash"
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($releaseZip)
try {
    $entries = @($archive.Entries |
        Where-Object { -not [string]::IsNullOrEmpty($_.Name) } |
        ForEach-Object { $_.FullName.Replace('\', '/') } |
        Sort-Object)
}
finally {
    $archive.Dispose()
}
$expectedEntries = @(
    'BepInEx/plugins/SuperBow/SuperBow.dll',
    'README.md'
) | Sort-Object
if (($entries -join "`n") -ne ($expectedEntries -join "`n")) {
    throw "Nexus 主文件 ZIP 内容不正确：$($entries -join ', ')"
}

$manifest = Get-Content -LiteralPath $hashManifestPath -Raw
$hashPaths = @(
    $releaseZip,
    $buildDll,
    $coverPath,
    $iconPath
)
foreach ($path in $hashPaths) {
    $hash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
    if (-not $manifest.Contains($hash)) {
        throw "SHA256SUMS.txt 缺少哈希：$hash ($path)"
    }
}

Write-Host 'NEXUS_RELEASE_VALID=True'
Write-Host "VERSION=$expectedVersion"
Write-Host "TITLE=$title"
Write-Host "SUMMARY_LENGTH=$($summary.Length)"
Write-Host 'COVER_SIZE=1280x720'
Write-Host 'ICON_SIZE=100x100'
Write-Host "ZIP_SHA256=$releaseZipHash"
Write-Host "ZIP_ENTRIES=$($entries -join ';')"
