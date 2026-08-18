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
$rootPrefix = $projectRoot.TrimEnd('\') + '\'
$gameAssembly = Join-Path $RatopiaDir 'Ratopia_Data\Managed\Assembly-CSharp.dll'
$bepInExAssembly = Join-Path $RatopiaDir 'BepInEx\core\BepInEx.dll'
$harmonyAssembly = Join-Path $RatopiaDir 'BepInEx\core\0Harmony.dll'
$gamePluginDir = Join-Path $RatopiaDir 'BepInEx\plugins\RestroomBathFun'

foreach ($required in @($gameAssembly, $bepInExAssembly, $harmonyAssembly)) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "缺少构建引用：$required"
    }
}
if (Test-Path -LiteralPath $gamePluginDir) {
    throw "游戏目录已存在 RestroomBathFun；本任务禁止安装或覆盖：$gamePluginDir"
}

$solution = Join-Path $projectRoot 'RestroomBathFun.sln'
$pluginProject = Join-Path $projectRoot 'src\RestroomBathFun\RestroomBathFun.csproj'
$pluginDll = Join-Path $projectRoot 'src\RestroomBathFun\bin\Release\net472\RestroomBathFun.dll'
$readme = Join-Path $projectRoot 'README.md'
$releaseAssets = Join-Path $projectRoot 'release-assets'
$stageDir = Join-Path $projectRoot 'artifacts\package'
$pluginStageDir = Join-Path $stageDir 'BepInEx\plugins\RestroomBathFun'
$distDir = Join-Path $projectRoot 'dist'
$zipPath = Join-Path $distDir '卫生间澡堂加乐趣-v1.0.0-BepInEx5.zip'
$deliveryDir = Join-Path $projectRoot 'Nexus-发布资料-卫生间澡堂加乐趣-v1.0.0'

function Assert-ProjectPath([string]$Path) {
    $fullPath = [IO.Path]::GetFullPath($Path)
    if (-not $fullPath.StartsWith($rootPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "路径不在项目目录内，拒绝修改：$fullPath"
    }
    return $fullPath
}

foreach ($cleanTarget in @($stageDir, $deliveryDir)) {
    $safeTarget = Assert-ProjectPath $cleanTarget
    if (Test-Path -LiteralPath $safeTarget) {
        Remove-Item -LiteralPath $safeTarget -Recurse -Force
    }
}

& dotnet clean $solution -c Release /p:RatopiaDir="$RatopiaDir" /p:InstallAfterBuild=false --nologo
if ($LASTEXITCODE -ne 0) { throw 'Release 清理失败。' }

& dotnet test $solution -c Release /p:RatopiaDir="$RatopiaDir" /p:InstallAfterBuild=false --nologo
if ($LASTEXITCODE -ne 0) { throw 'Release 测试失败。' }

& dotnet build $pluginProject -c Release /p:RatopiaDir="$RatopiaDir" /p:InstallAfterBuild=false --no-restore --nologo
if ($LASTEXITCODE -ne 0) { throw 'Release 构建失败。' }

New-Item -ItemType Directory -Path $pluginStageDir -Force | Out-Null
New-Item -ItemType Directory -Path $distDir -Force | Out-Null
New-Item -ItemType Directory -Path $deliveryDir -Force | Out-Null
Copy-Item -LiteralPath $pluginDll -Destination (Join-Path $pluginStageDir 'RestroomBathFun.dll')
Copy-Item -LiteralPath $readme -Destination (Join-Path $stageDir 'README.md')

$unexpectedDlls = @(Get-ChildItem -LiteralPath $stageDir -Recurse -Filter '*.dll' | Where-Object {
    $_.Name -ne 'RestroomBathFun.dll'
})
$unexpectedPdbs = @(Get-ChildItem -LiteralPath $stageDir -Recurse -Filter '*.pdb')
if ($unexpectedDlls.Count -gt 0 -or $unexpectedPdbs.Count -gt 0) {
    throw '暂存包包含禁止文件。禁止：Assembly-CSharp.dll、0Harmony.dll、BepInEx.dll、UnityEngine.dll、UnityEngine.CoreModule.dll、*.pdb。'
}

if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath (Assert-ProjectPath $zipPath) -Force
}
Compress-Archive `
    -LiteralPath (Join-Path $stageDir 'BepInEx'), (Join-Path $stageDir 'README.md') `
    -DestinationPath $zipPath `
    -CompressionLevel Optimal

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $actualEntries = @($archive.Entries | Where-Object { $_.Name } | ForEach-Object {
        $_.FullName.Replace('\', '/')
    })
    $expectedEntries = @(
        'BepInEx/plugins/RestroomBathFun/RestroomBathFun.dll',
        'README.md'
    )
    $actualSignature = (($actualEntries | Sort-Object) -join "`n")
    $expectedSignature = (($expectedEntries | Sort-Object) -join "`n")
    if ($actualSignature -ne $expectedSignature) {
        throw "ZIP 结构不符合两文件合同：$($actualEntries -join ', ')"
    }
}
finally {
    $archive.Dispose()
}

Copy-Item -LiteralPath (Join-Path $releaseAssets '1-英文标题.txt') -Destination $deliveryDir
Copy-Item -LiteralPath (Join-Path $releaseAssets '2-简介.txt') -Destination $deliveryDir
Copy-Item -LiteralPath (Join-Path $releaseAssets '3-双语完整介绍.txt') -Destination $deliveryDir
Copy-Item -LiteralPath (Join-Path $releaseAssets '4-封面.png') -Destination $deliveryDir
Copy-Item -LiteralPath $zipPath -Destination (Join-Path $deliveryDir '5-卫生间澡堂加乐趣-v1.0.0-BepInEx5.zip')

$ratopiaPackageValidator = Join-Path $env:USERPROFILE '.codex\skills\developing-ratopia-mods\scripts\Test-RatopiaPackage.ps1'
$nexusValidator = Join-Path $env:USERPROFILE '.codex\skills\publishing-ratopia-nexus-mods\scripts\Test-RatopiaNexusDeliverables.ps1'
& $ratopiaPackageValidator -Path $zipPath -ExpectedPluginName 'RestroomBathFun'
if ($LASTEXITCODE -ne 0) { throw 'Test-RatopiaPackage.ps1 校验失败。' }
& $nexusValidator -Path $deliveryDir -ModName 'RestroomBathFun' -Version '1.0.0'
if ($LASTEXITCODE -ne 0) { throw 'Test-RatopiaNexusDeliverables.ps1 校验失败。' }

if (Test-Path -LiteralPath $gamePluginDir) {
    throw "检测到意外安装，已停止交付：$gamePluginDir"
}

$hash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash
Write-Host "FINAL_DELIVERY=$deliveryDir"
Write-Host "MOD_ZIP_SHA256=$hash"
