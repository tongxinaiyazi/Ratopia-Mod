[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$ReleaseDir
)

$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
if ([string]::IsNullOrWhiteSpace($ReleaseDir)) {
    $ReleaseDir = Join-Path $projectRoot 'release\NexusMods'
}

$releaseRoot = [IO.Path]::GetFullPath($ReleaseDir)
$expectedVersion = '0.1.0'
$expectedTitle = 'Stronger Work Distance'
$expectedChineseTitle = '更强大的工作距离'
$expectedInstallPath = 'BepInEx/plugins/StrongerWorkDistance/StrongerWorkDistance.dll'
$expectedZipHash = '015B22B2BE375EA3EE62D0E0698DFCED9DB61CA5531EDD18E686AFC6EFEA97F8'
$sourceZip = Join-Path $projectRoot 'dist\更强大的工作距离-v0.1.0-BepInEx5.zip'
$coverSvgPath = Join-Path $releaseRoot 'images\StrongerWorkDistance-cover-1600x900.svg'
$coverPngPath = Join-Path $releaseRoot 'images\StrongerWorkDistance-cover-1600x900.png'

if (-not $releaseRoot.StartsWith(
        $projectRoot + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "Nexus 发布目录不在项目目录内：$releaseRoot"
}

$requiredRelativeFiles = @(
    'NEXUS_TITLE.txt',
    'NEXUS_SUMMARY.txt',
    'NEXUS_DESCRIPTION.txt',
    'FILE_DESCRIPTION.txt',
    'CHANGELOG.txt',
    'UPLOAD_CHECKLIST.md',
    'images\StrongerWorkDistance-cover-1600x900.svg',
    'images\StrongerWorkDistance-cover-1600x900.png'
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

$title = (Get-Content -LiteralPath (Join-Path $releaseRoot 'NEXUS_TITLE.txt') -Raw).Trim()
$summary = (Get-Content -LiteralPath (Join-Path $releaseRoot 'NEXUS_SUMMARY.txt') -Raw).Trim()
$description = Get-Content -LiteralPath (Join-Path $releaseRoot 'NEXUS_DESCRIPTION.txt') -Raw
$fileDescription = Get-Content -LiteralPath (Join-Path $releaseRoot 'FILE_DESCRIPTION.txt') -Raw
$changelog = Get-Content -LiteralPath (Join-Path $releaseRoot 'CHANGELOG.txt') -Raw
$uploadChecklist = Get-Content -LiteralPath (Join-Path $releaseRoot 'UPLOAD_CHECKLIST.md') -Raw

if ($title -ne $expectedTitle) {
    throw "英文标题不正确：$title"
}
if ($summary.Length -gt 250) {
    throw "双语简介超过 250 字符：$($summary.Length)"
}
foreach ($requiredSummaryText in @('2 tiles', '4 tiles', '横向 2 格', '最高 4 格')) {
    if (-not $summary.Contains($requiredSummaryText)) {
        throw "双语简介缺少必要内容：$requiredSummaryText"
    }
}

foreach ($requiredDescriptionText in @(
        '[b]English[/b]',
        '[b]中文[/b]',
        '25-position',
        'BepInEx 5.4.23.5',
        $expectedInstallPath,
        'SystemMgr.List_WM_EnableArea',
        'SystemMgr.List_BP_Ld_EnableArea',
        '不修改女王操作距离',
        '不读取或写入存档字段')) {
    if (-not $description.Contains($requiredDescriptionText)) {
        throw "Nexus 页面正文缺少必要内容：$requiredDescriptionText"
    }
}

foreach ($requiredFileText in @($expectedVersion, $expectedInstallPath, 'BepInEx 5', '不包含')) {
    if (-not $fileDescription.Contains($requiredFileText)) {
        throw "主文件说明缺少必要内容：$requiredFileText"
    }
}
if (-not $changelog.Contains($expectedVersion) -or
    -not $changelog.Contains('25') -or
    -not $changelog.Contains('25 个')) {
    throw '更新日志缺少版本或双语范围说明。'
}

foreach ($requiredChecklistText in @(
        'dist/更强大的工作距离-v0.1.0-BepInEx5.zip',
        'images/StrongerWorkDistance-cover-1600x900.png',
        $expectedZipHash,
        '- [ ] 进入受控测试存档',
        '- [ ] 保存、退出并重新读档',
        '- [ ] 临时移除 Mod')) {
    if (-not $uploadChecklist.Contains($requiredChecklistText)) {
        throw "上传检查表缺少必要内容：$requiredChecklistText"
    }
}

$textFiles = Get-ChildItem -LiteralPath $releaseRoot -File |
    Where-Object { $_.Extension -in @('.txt', '.md') }
foreach ($file in $textFiles) {
    $content = Get-Content -LiteralPath $file.FullName -Raw
    if ($content -match '(?im)\b(TBD|TODO)\b|<placeholder>|待填写') {
        throw "发布文本包含占位内容：$($file.Name)"
    }
    if ($content -match '(?i)[A-Z]:\\.*SaveFile|AppData\\LocalLow') {
        throw "发布文本包含本机存档路径：$($file.Name)"
    }
    if ($content -match '0\.0\.\d+|0\.1\.[1-9]\d*') {
        throw "发布文本包含非目标版本号：$($file.Name)"
    }
}

foreach ($tag in @('center', 'size', 'b', 'i', 'list')) {
    $openCount = ([regex]::Matches($description, "\[$tag(?:=[^\]]+)?\]", 'IgnoreCase')).Count
    $closeCount = ([regex]::Matches($description, "\[/$tag\]", 'IgnoreCase')).Count
    if ($openCount -ne $closeCount) {
        throw "Nexus BBCode 标签未成对闭合：$tag ($openCount/$closeCount)"
    }
}

[xml]$svg = Get-Content -LiteralPath $coverSvgPath -Raw
$svgRoot = $svg.DocumentElement
if ($svgRoot.GetAttribute('width') -ne '1600' -or
    $svgRoot.GetAttribute('height') -ne '900' -or
    $svgRoot.GetAttribute('viewBox') -ne '0 0 1600 900') {
    throw 'SVG 尺寸或 viewBox 不正确。'
}
$visibleTitles = @($svg.SelectNodes('//*[local-name()="text"]') | ForEach-Object { $_.InnerText.Trim() })
$expectedVisibleTitles = @('STRONGER WORK DISTANCE', $expectedChineseTitle)
if (($visibleTitles -join "`n") -ne ($expectedVisibleTitles -join "`n")) {
    throw "SVG 可见文字不正确：$($visibleTitles -join ' | ')"
}

Add-Type -AssemblyName System.Drawing
$englishTitleNode = $svg.SelectSingleNode('//*[local-name()="text" and text()="STRONGER WORK DISTANCE"]')
$englishFontSize = [float]::Parse(
    $englishTitleNode.GetAttribute('font-size'),
    [Globalization.CultureInfo]::InvariantCulture)
$englishLetterSpacing = [float]::Parse(
    $englishTitleNode.GetAttribute('letter-spacing'),
    [Globalization.CultureInfo]::InvariantCulture)
$measurementBitmap = [Drawing.Bitmap]::new(1, 1)
$measurementGraphics = [Drawing.Graphics]::FromImage($measurementBitmap)
$measurementFont = [Drawing.Font]::new(
    'Segoe UI',
    $englishFontSize,
    [Drawing.FontStyle]::Bold,
    [Drawing.GraphicsUnit]::Pixel)
try {
    $glyphSize = $measurementGraphics.MeasureString(
        'STRONGER WORK DISTANCE',
        $measurementFont,
        [int]::MaxValue,
        [Drawing.StringFormat]::GenericTypographic)
    $estimatedTitleWidth = $glyphSize.Width + (21 * $englishLetterSpacing)
    if ($estimatedTitleWidth -gt 1450) {
        throw "英文标题超过安全宽度：$([Math]::Round($estimatedTitleWidth, 1))px > 1450px"
    }
}
finally {
    $measurementFont.Dispose()
    $measurementGraphics.Dispose()
    $measurementBitmap.Dispose()
}

$image = [Drawing.Image]::FromFile($coverPngPath)
try {
    if ($image.Width -ne 1600 -or $image.Height -ne 900) {
        throw "封面 PNG 尺寸不正确：$($image.Width)x$($image.Height)"
    }
}
finally {
    $image.Dispose()
}

if (-not (Test-Path -LiteralPath $sourceZip -PathType Leaf)) {
    throw "缺少 Mod 发布包：$sourceZip"
}
$zipHash = (Get-FileHash -LiteralPath $sourceZip -Algorithm SHA256).Hash
if ($zipHash -ne $expectedZipHash) {
    throw "Mod 发布包 SHA-256 已变化：$zipHash"
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($sourceZip)
try {
    $entries = @($archive.Entries |
        Where-Object { -not [string]::IsNullOrEmpty($_.Name) } |
        ForEach-Object { $_.FullName.Replace('\', '/') } |
        Sort-Object)
}
finally {
    $archive.Dispose()
}
$expectedEntries = @($expectedInstallPath, 'README.md') | Sort-Object
if (($entries -join "`n") -ne ($expectedEntries -join "`n")) {
    throw "Mod ZIP 内容不正确：$($entries -join ', ')"
}
foreach ($entry in $entries) {
    if ($entry -match '(?i)(Assembly-CSharp|UnityEngine|BepInEx|0Harmony)\.dll$|\.pdb$|LogOutput\.log$|SaveFile') {
        throw "Mod ZIP 包含禁止文件：$entry"
    }
}

$forbiddenLooseFiles = @(Get-ChildItem -LiteralPath $releaseRoot -Recurse -File |
    Where-Object { $_.Extension -in @('.dll', '.pdb', '.log', '.sav') })
if ($forbiddenLooseFiles.Count -gt 0) {
    throw "Nexus 发布资料包含禁止文件：$($forbiddenLooseFiles.FullName -join ', ')"
}

Write-Host 'NEXUS_RELEASE_VALID=True'
Write-Host "VERSION=$expectedVersion"
Write-Host "TITLE=$title"
Write-Host "SUMMARY_LENGTH=$($summary.Length)"
Write-Host 'COVER_SIZE=1600x900'
Write-Host "ZIP_SHA256=$zipHash"
Write-Host "ZIP_ENTRIES=$($entries -join ';')"
