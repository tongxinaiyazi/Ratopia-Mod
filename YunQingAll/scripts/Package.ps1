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
$resolvedRatopiaDir = [IO.Path]::GetFullPath(
    [Environment]::ExpandEnvironmentVariables($RatopiaDir))
$gameAssembly = Join-Path $resolvedRatopiaDir 'Ratopia_Data\Managed\Assembly-CSharp.dll'
$bepInExAssembly = Join-Path $resolvedRatopiaDir 'BepInEx\core\BepInEx.dll'
$harmonyAssembly = Join-Path $resolvedRatopiaDir 'BepInEx\core\0Harmony.dll'
$expectedGameHash = 'C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D'

foreach ($required in @($gameAssembly, $bepInExAssembly, $harmonyAssembly)) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "缺少构建所需文件：$required"
    }
}

$actualGameHash = (Get-FileHash -LiteralPath $gameAssembly -Algorithm SHA256).Hash
if (-not $actualGameHash.Equals($expectedGameHash, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Assembly-CSharp.dll 与已检查版本不一致。预期：$expectedGameHash；实际：$actualGameHash"
}

$bepInExVersion = [Reflection.AssemblyName]::GetAssemblyName($bepInExAssembly).Version
$harmonyVersion = [Reflection.AssemblyName]::GetAssemblyName($harmonyAssembly).Version
if ($bepInExVersion.ToString() -ne '5.4.23.5') {
    throw "BepInEx 版本不匹配。预期 5.4.23.5，实际 $bepInExVersion"
}
if ($harmonyVersion.ToString() -ne '2.9.0.0') {
    throw "Harmony 版本不匹配。预期 2.9.0.0，实际 $harmonyVersion"
}

$solution = Join-Path $projectRoot 'YunQingAll.sln'
$pluginProject = Join-Path $projectRoot 'src\YunQingAll\YunQingAll.csproj'
$msbuildRatopiaDir = "/p:RatopiaDir=$resolvedRatopiaDir"

& dotnet clean $solution -c Release $msbuildRatopiaDir
if ($LASTEXITCODE -ne 0) { throw 'Release 清理失败，已停止打包。' }

& dotnet test $solution -c Release $msbuildRatopiaDir /p:InstallAfterBuild=false
if ($LASTEXITCODE -ne 0) { throw 'Release 测试失败，已停止打包。' }

& dotnet build $pluginProject -c Release $msbuildRatopiaDir /p:InstallAfterBuild=false --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Release 构建失败，已停止打包。' }

$pluginOutputDir = Join-Path $projectRoot 'src\YunQingAll\bin\Release\net472'
$pluginDll = Join-Path $pluginOutputDir 'YunQingAll.dll'
$forbiddenOutputFiles = @(Get-ChildItem -LiteralPath $pluginOutputDir -File | Where-Object {
    $_.Extension -ieq '.pdb' -or
    $_.Name -ieq 'Assembly-CSharp.dll' -or
    $_.Name -ieq '0Harmony.dll' -or
    $_.Name -like 'UnityEngine*.dll' -or
    $_.Name -like 'BepInEx*.dll'
})
if ($forbiddenOutputFiles.Count -gt 0) {
    throw "Release 输出包含禁止文件：$($forbiddenOutputFiles.Name -join ', ')"
}

$distDir = Join-Path $projectRoot 'dist'
$stageDir = Join-Path $projectRoot 'artifacts\package'
$pluginStageDir = Join-Path $stageDir 'BepInEx\plugins\YunQingAll'
$zipPath = Join-Path $distDir 'YunQingAll-v2.2.0-BepInEx5.zip'

if (-not $stageDir.StartsWith($projectRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "暂存目录不在项目内，拒绝清理：$stageDir"
}
if (-not $distDir.StartsWith($projectRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "发布目录不在项目内，拒绝写入：$distDir"
}

if (Test-Path -LiteralPath $stageDir) {
    Remove-Item -LiteralPath $stageDir -Recurse -Force
}
New-Item -ItemType Directory -Path $pluginStageDir -Force | Out-Null
New-Item -ItemType Directory -Path $distDir -Force | Out-Null
Copy-Item -LiteralPath $pluginDll -Destination (Join-Path $pluginStageDir 'YunQingAll.dll')
Copy-Item -LiteralPath (Join-Path $projectRoot 'README.md') -Destination (Join-Path $stageDir 'README.md')

if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}
Compress-Archive `
    -LiteralPath (Join-Path $stageDir 'BepInEx'), (Join-Path $stageDir 'README.md') `
    -DestinationPath $zipPath `
    -CompressionLevel Optimal

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $entries = @($archive.Entries |
        Where-Object { -not [string]::IsNullOrEmpty($_.Name) } |
        ForEach-Object { $_.FullName.Replace('\', '/') })
    $expectedEntries = @(
        'BepInEx/plugins/YunQingAll/YunQingAll.dll',
        'README.md'
    )
    $missing = @($expectedEntries | Where-Object { $_ -notin $entries })
    $unexpected = @($entries | Where-Object { $_ -notin $expectedEntries })
    if ($missing.Count -gt 0 -or $unexpected.Count -gt 0) {
        throw "发布包结构不符合约定。缺少：$($missing -join ', ')；多余：$($unexpected -join ', ')"
    }
}
finally {
    $archive.Dispose()
}

$zipHash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash
$dllHash = (Get-FileHash -LiteralPath $pluginDll -Algorithm SHA256).Hash
Write-Host "发布包：$zipPath"
Write-Host "ZIP SHA-256：$zipHash"
Write-Host "DLL SHA-256：$dllHash"
