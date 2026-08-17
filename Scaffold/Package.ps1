[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$RatopiaDir = $env:RATOPIA_DIR
)

$ErrorActionPreference = 'Stop'
$expectedGameHash = 'C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D'

if ([string]::IsNullOrWhiteSpace($RatopiaDir)) {
    throw '请通过 -RatopiaDir 或 RATOPIA_DIR 指定 Ratopia 游戏目录。'
}

$projectRoot = (Resolve-Path -LiteralPath $PSScriptRoot).Path
$resolvedRatopiaDir = (Resolve-Path -LiteralPath $RatopiaDir).Path
$gameAssembly = Join-Path $resolvedRatopiaDir 'Ratopia_Data\Managed\Assembly-CSharp.dll'
$bepInExAssembly = Join-Path $resolvedRatopiaDir 'BepInEx\core\BepInEx.dll'
$harmonyAssembly = Join-Path $resolvedRatopiaDir 'BepInEx\core\0Harmony.dll'
foreach ($required in @($gameAssembly, $bepInExAssembly, $harmonyAssembly)) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "环境检查失败，缺少文件：$required"
    }
}

$actualGameHash = (Get-FileHash -LiteralPath $gameAssembly -Algorithm SHA256).Hash
if (-not $actualGameHash.Equals($expectedGameHash, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Assembly-CSharp.dll 哈希不匹配。预期 $expectedGameHash，实际 $actualGameHash"
}

$solution = Join-Path $projectRoot 'Scaffold.sln'
$pluginProject = Join-Path $projectRoot 'src\Scaffold\Scaffold.csproj'
& dotnet test $solution -c Release /p:RatopiaDir="$resolvedRatopiaDir" /p:InstallAfterBuild=false
if ($LASTEXITCODE -ne 0) { throw 'Release 测试失败，已停止打包。' }

& dotnet build $pluginProject -c Release /p:RatopiaDir="$resolvedRatopiaDir" /p:InstallAfterBuild=false --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Release 构建失败，已停止打包。' }

$distDir = Join-Path $projectRoot 'dist'
$stageDir = Join-Path $projectRoot 'artifacts\package'
$pluginStageDir = Join-Path $stageDir 'BepInEx\plugins\Scaffold'
$dataStageDir = Join-Path $pluginStageDir 'Data'
$zipPath = Join-Path $distDir '脚手架-v0.1.0-BepInEx5.zip'
$pluginOutputDir = Join-Path $projectRoot 'src\Scaffold\bin\Release\net472'
$pluginDll = Join-Path $pluginOutputDir 'Scaffold.dll'
$dataSourceDir = Join-Path $projectRoot 'src\Scaffold\Data'

$resolvedStage = [System.IO.Path]::GetFullPath($stageDir)
$resolvedRootPrefix = [System.IO.Path]::GetFullPath($projectRoot).TrimEnd('\') + '\'
if (-not $resolvedStage.StartsWith($resolvedRootPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "暂存目录不在项目内，拒绝清理：$resolvedStage"
}

if (Test-Path -LiteralPath $resolvedStage) {
    Remove-Item -LiteralPath $resolvedStage -Recurse -Force
}

New-Item -ItemType Directory -Path $dataStageDir -Force | Out-Null
New-Item -ItemType Directory -Path $distDir -Force | Out-Null
Copy-Item -LiteralPath $pluginDll -Destination (Join-Path $pluginStageDir 'Scaffold.dll')
foreach ($asset in @('world.png', 'menu.png', 'blueprint.png')) {
    Copy-Item -LiteralPath (Join-Path $dataSourceDir $asset) -Destination (Join-Path $dataStageDir $asset)
}
Copy-Item -LiteralPath (Join-Path $projectRoot 'README.md') -Destination (Join-Path $stageDir 'README.md')

if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

Compress-Archive -LiteralPath @(
    (Join-Path $stageDir 'BepInEx'),
    (Join-Path $stageDir 'README.md')
) -DestinationPath $zipPath -CompressionLevel Optimal

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $names = @($archive.Entries |
        ForEach-Object { $_.FullName.Replace('\', '/') } |
        Where-Object { -not $_.EndsWith('/') })
    $expected = @(
        'BepInEx/plugins/Scaffold/Scaffold.dll',
        'BepInEx/plugins/Scaffold/Data/world.png',
        'BepInEx/plugins/Scaffold/Data/menu.png',
        'BepInEx/plugins/Scaffold/Data/blueprint.png',
        'README.md'
    )
    $unexpected = @($names | Where-Object { $_ -notin $expected })
    $missing = @($expected | Where-Object { $_ -notin $names })
    if ($unexpected.Count -gt 0 -or $missing.Count -gt 0) {
        throw "发布包内容不符合白名单。缺少：$($missing -join ', ')；多余：$($unexpected -join ', ')"
    }
}
finally {
    $archive.Dispose()
}

$pluginHash = (Get-FileHash -LiteralPath $pluginDll -Algorithm SHA256).Hash
$zipHash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash
Write-Host "游戏程序集 SHA-256：$actualGameHash"
Write-Host "插件 DLL SHA-256：$pluginHash"
Write-Host "发布包：$zipPath"
Write-Host "发布包 SHA-256：$zipHash"
