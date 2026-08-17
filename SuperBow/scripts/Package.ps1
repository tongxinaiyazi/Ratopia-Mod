[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$RatopiaDir = 'E:\steam\steamapps\common\Ratopia'
)

$ErrorActionPreference = 'Stop'

$projectRoot = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$resolvedRatopiaDir = [IO.Path]::GetFullPath(
    [Environment]::ExpandEnvironmentVariables($RatopiaDir))
$gameAssembly = Join-Path $resolvedRatopiaDir 'Ratopia_Data\Managed\Assembly-CSharp.dll'
$itemAssets = Join-Path $resolvedRatopiaDir 'Ratopia_Data\sharedassets2.assets'
$bepInExAssembly = Join-Path $resolvedRatopiaDir 'BepInEx\core\BepInEx.dll'
$harmonyAssembly = Join-Path $resolvedRatopiaDir 'BepInEx\core\0Harmony.dll'
$expectedGameHash = 'C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D'
$expectedItemAssetsHash = '847D342FF36CD479790B39B6BA0D4159076C9995126E509FDE93961999A016C0'

foreach ($required in @($gameAssembly, $itemAssets, $bepInExAssembly, $harmonyAssembly)) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "缺少构建所需文件：$required"
    }
}

$actualGameHash = (Get-FileHash -LiteralPath $gameAssembly -Algorithm SHA256).Hash
if (-not $actualGameHash.Equals($expectedGameHash, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Assembly-CSharp.dll 与已检查版本不一致。预期：$expectedGameHash；实际：$actualGameHash"
}

$actualItemAssetsHash = (Get-FileHash -LiteralPath $itemAssets -Algorithm SHA256).Hash
if (-not $actualItemAssetsHash.Equals($expectedItemAssetsHash, [StringComparison]::OrdinalIgnoreCase)) {
    throw "sharedassets2.assets 与已检查版本不一致。预期：$expectedItemAssetsHash；实际：$actualItemAssetsHash"
}

$bepInExVersion = [Reflection.AssemblyName]::GetAssemblyName($bepInExAssembly).Version
$harmonyVersion = [Reflection.AssemblyName]::GetAssemblyName($harmonyAssembly).Version
if ($bepInExVersion.ToString() -ne '5.4.23.5') {
    throw "BepInEx 版本不匹配。预期 5.4.23.5，实际 $bepInExVersion"
}
if ($harmonyVersion.ToString() -ne '2.9.0.0') {
    throw "Harmony 版本不匹配。预期 2.9.0.0，实际 $harmonyVersion"
}

$solution = Join-Path $projectRoot 'SuperBow.sln'
$pluginProject = Join-Path $projectRoot 'src\SuperBow\SuperBow.csproj'
$msbuildRatopiaDir = "/p:RatopiaDir=$resolvedRatopiaDir"

& dotnet clean $solution -c Release $msbuildRatopiaDir
if ($LASTEXITCODE -ne 0) { throw 'Release 清理失败，已停止打包。' }

& dotnet test $solution -c Release $msbuildRatopiaDir /p:InstallAfterBuild=false --nologo
if ($LASTEXITCODE -ne 0) { throw 'Release 测试失败，已停止打包。' }

& dotnet build $pluginProject -c Release $msbuildRatopiaDir /p:InstallAfterBuild=false --no-restore --nologo
if ($LASTEXITCODE -ne 0) { throw 'Release 构建失败，已停止打包。' }

$pluginOutputDir = Join-Path $projectRoot 'src\SuperBow\bin\Release\net472'
$pluginDll = Join-Path $pluginOutputDir 'SuperBow.dll'
if (-not (Test-Path -LiteralPath $pluginDll -PathType Leaf)) {
    throw "Release 插件 DLL 不存在：$pluginDll"
}

$forbiddenOutputFiles = @(Get-ChildItem -LiteralPath $pluginOutputDir -File | Where-Object {
    $_.Name -like '*.pdb' -or
    $_.Name -ieq 'Assembly-CSharp.dll' -or
    $_.Name -ieq '0Harmony.dll' -or
    $_.Name -ieq 'Mono.Cecil.dll' -or
    $_.Name -like 'UnityEngine*.dll' -or
    $_.Name -like 'BepInEx*.dll'
})
if ($forbiddenOutputFiles.Count -gt 0) {
    throw "Release 输出包含禁止文件：$($forbiddenOutputFiles.Name -join ', ')"
}

$artifactsDir = Join-Path $projectRoot 'artifacts'
$stageDir = Join-Path $artifactsDir 'package'
$distDir = Join-Path $projectRoot 'dist'
$pluginStageDir = Join-Path $stageDir 'BepInEx\plugins\SuperBow'
$zipPath = Join-Path $distDir 'SuperBow-v0.1.2-BepInEx5.zip'

foreach ($target in @($stageDir, $distDir)) {
    $resolvedTarget = [IO.Path]::GetFullPath($target)
    if (-not $resolvedTarget.StartsWith(
            $projectRoot + [IO.Path]::DirectorySeparatorChar,
            [StringComparison]::OrdinalIgnoreCase)) {
        throw "发布路径不在项目目录内：$resolvedTarget"
    }
}

if (Test-Path -LiteralPath $stageDir) {
    Remove-Item -LiteralPath $stageDir -Recurse -Force
}
New-Item -ItemType Directory -Path $pluginStageDir -Force | Out-Null
New-Item -ItemType Directory -Path $distDir -Force | Out-Null
Copy-Item -LiteralPath $pluginDll -Destination (Join-Path $pluginStageDir 'SuperBow.dll')
Copy-Item -LiteralPath (Join-Path $projectRoot 'README.md') -Destination (Join-Path $stageDir 'README.md')

$allowedEntries = @(
    'BepInEx/plugins/SuperBow/SuperBow.dll',
    'README.md'
)
$stagedEntries = @(Get-ChildItem -LiteralPath $stageDir -Recurse -File | ForEach-Object {
    $_.FullName.Substring($stageDir.Length).TrimStart('\', '/').Replace('\', '/')
})
$missingStaged = @($allowedEntries | Where-Object { $_ -notin $stagedEntries })
$unexpectedStaged = @($stagedEntries | Where-Object { $_ -notin $allowedEntries })
if ($missingStaged.Count -gt 0 -or $unexpectedStaged.Count -gt 0) {
    throw "暂存结构不符合约定。缺少：$($missingStaged -join ', ')；多余：$($unexpectedStaged -join ', ')"
}

if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory(
    $stageDir,
    $zipPath,
    [IO.Compression.CompressionLevel]::Optimal,
    $false)

$archive = [IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $archiveEntries = @($archive.Entries |
        Where-Object { -not [string]::IsNullOrEmpty($_.Name) } |
        ForEach-Object { $_.FullName.Replace('\', '/') })
    $missingArchive = @($allowedEntries | Where-Object { $_ -notin $archiveEntries })
    $unexpectedArchive = @($archiveEntries | Where-Object { $_ -notin $allowedEntries })
    if ($missingArchive.Count -gt 0 -or $unexpectedArchive.Count -gt 0) {
        throw "发布包结构不符合约定。缺少：$($missingArchive -join ', ')；多余：$($unexpectedArchive -join ', ')"
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
