[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$RatopiaDir = $env:RATOPIA_DIR
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RatopiaDir)) {
    throw '请通过 -RatopiaDir 或 RATOPIA_DIR 指定 Ratopia 游戏目录。'
}

$projectRoot = (Resolve-Path -LiteralPath (Split-Path -Parent $PSScriptRoot)).Path
$gameAssembly = Join-Path $RatopiaDir 'Ratopia_Data\Managed\Assembly-CSharp.dll'
$bepInExAssembly = Join-Path $RatopiaDir 'BepInEx\core\BepInEx.dll'
$harmonyAssembly = Join-Path $RatopiaDir 'BepInEx\core\0Harmony.dll'
if (-not (Test-Path -LiteralPath $gameAssembly) -or
    -not (Test-Path -LiteralPath $bepInExAssembly) -or
    -not (Test-Path -LiteralPath $harmonyAssembly)) {
    throw "RatopiaDir 无效，或没有完整安装 BepInEx 5：$RatopiaDir"
}

$env:RATOPIA_DIR = $RatopiaDir
$solution = Join-Path $projectRoot 'BroadcastStationGlobalCoverage.sln'
$pluginProject = Join-Path $projectRoot 'src\BroadcastStationGlobalCoverage\BroadcastStationGlobalCoverage.csproj'

& dotnet clean $solution -c Release /p:InstallAfterBuild=false --nologo
if ($LASTEXITCODE -ne 0) {
    throw 'Release 清理失败，已停止打包。'
}

& dotnet test $solution -c Release /p:InstallAfterBuild=false --nologo
if ($LASTEXITCODE -ne 0) {
    throw 'Release 测试失败，已停止打包。'
}

& dotnet build $pluginProject -c Release /p:InstallAfterBuild=false --no-restore --nologo
if ($LASTEXITCODE -ne 0) {
    throw 'Release 构建失败，已停止打包。'
}

$distDir = Join-Path $projectRoot 'dist'
$stageDir = Join-Path $projectRoot 'artifacts\package'
$pluginStageDir = Join-Path $stageDir 'BepInEx\plugins\BroadcastStationGlobalCoverage'
$zipPath = Join-Path $distDir '广播站信号覆盖全图-v0.1.1-BepInEx5.zip'
$pluginDll = Join-Path $projectRoot 'src\BroadcastStationGlobalCoverage\bin\Release\net472\BroadcastStationGlobalCoverage.dll'
$readme = Join-Path $projectRoot 'README.md'

$rootPrefix = $projectRoot.TrimEnd('\') + '\'
$stageFullPath = [System.IO.Path]::GetFullPath($stageDir)
if (-not $stageFullPath.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "暂存目录不在项目内，拒绝清理：$stageFullPath"
}

if (Test-Path -LiteralPath $stageFullPath) {
    Remove-Item -LiteralPath $stageFullPath -Recurse -Force
}

New-Item -ItemType Directory -Path $pluginStageDir -Force | Out-Null
New-Item -ItemType Directory -Path $distDir -Force | Out-Null
Copy-Item -LiteralPath $pluginDll -Destination (Join-Path $pluginStageDir 'BroadcastStationGlobalCoverage.dll')
Copy-Item -LiteralPath $readme -Destination (Join-Path $stageDir 'README.md')

if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

Compress-Archive `
    -LiteralPath (Join-Path $stageDir 'BepInEx'), (Join-Path $stageDir 'README.md') `
    -DestinationPath $zipPath `
    -CompressionLevel Optimal

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $actualEntries = @(
        $archive.Entries |
            ForEach-Object { $_.FullName.Replace('\', '/') } |
            Where-Object { -not $_.EndsWith('/') }
    )
    $expectedEntries = @(
        'BepInEx/plugins/BroadcastStationGlobalCoverage/BroadcastStationGlobalCoverage.dll',
        'README.md'
    )
    $unexpected = @($actualEntries | Where-Object { $_ -notin $expectedEntries })
    $missing = @($expectedEntries | Where-Object { $_ -notin $actualEntries })
    if ($unexpected.Count -gt 0 -or $missing.Count -gt 0) {
        throw "发布包结构错误。缺少：$($missing -join ', ')；多余：$($unexpected -join ', ')"
    }

    $forbiddenDlls = @(
        'Assembly-CSharp.dll',
        '0Harmony.dll',
        'BepInEx.dll',
        'UnityEngine.dll',
        'UnityEngine.CoreModule.dll'
    )
    $forbiddenFound = @(
        $actualEntries |
            Where-Object { [System.IO.Path]::GetFileName($_) -in $forbiddenDlls }
    )
    if ($forbiddenFound.Count -gt 0) {
        throw "发布包包含禁止分发的游戏或加载器 DLL：$($forbiddenFound -join ', ')"
    }
}
finally {
    $archive.Dispose()
}

$hash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash
Write-Host "发布包：$zipPath"
Write-Host "SHA-256：$hash"
