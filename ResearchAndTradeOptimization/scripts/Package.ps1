param(
    [string]$GameDir = 'E:\steam\steamapps\common\Ratopia'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$expectedAssemblyHash = 'C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D'
$archiveName = '研究与贸易优化-v0.3.0-BepInEx5.zip'
$projectRoot = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$solutionPath = Join-Path $projectRoot 'ResearchAndTradeOptimization.sln'
$testProjectPath = Join-Path $projectRoot 'tests\ResearchAndTradeOptimization.Tests\ResearchAndTradeOptimization.Tests.csproj'
$assemblyPath = Join-Path $GameDir 'Ratopia_Data\Managed\Assembly-CSharp.dll'
$pluginBuildPath = Join-Path $projectRoot 'src\ResearchAndTradeOptimization\bin\Release\net472\ResearchAndTradeOptimization.dll'
$distRoot = Join-Path $projectRoot 'dist'
$stageRoot = Join-Path $distRoot '.stage-ResearchAndTradeOptimization'
$stagePluginRoot = Join-Path $stageRoot 'BepInEx\plugins\ResearchAndTradeOptimization'
$archivePath = Join-Path $distRoot $archiveName
$resolvedRootPrefix = $projectRoot.TrimEnd('\') + '\'
$resolvedStageRoot = [IO.Path]::GetFullPath($stageRoot)
$resolvedArchivePath = [IO.Path]::GetFullPath($archivePath)

if (-not $resolvedStageRoot.StartsWith($resolvedRootPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw "拒绝清理项目目录之外的暂存路径：$resolvedStageRoot"
}
if (-not $resolvedArchivePath.StartsWith($resolvedRootPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw "拒绝清理项目目录之外的发布包：$resolvedArchivePath"
}

if (-not (Test-Path -LiteralPath $assemblyPath -PathType Leaf)) {
    throw "找不到游戏程序集：$assemblyPath"
}

$actualAssemblyHash = (Get-FileHash -LiteralPath $assemblyPath -Algorithm SHA256).Hash
if (-not $actualAssemblyHash.Equals($expectedAssemblyHash, [StringComparison]::OrdinalIgnoreCase)) {
    throw "游戏程序集哈希不匹配。期望 $expectedAssemblyHash，实际 $actualAssemblyHash。"
}

# Remove stale release output before tests so the archive/build identity contract
# cannot compare the current build with a previous package.
if (Test-Path -LiteralPath $stageRoot) {
    Remove-Item -LiteralPath $stageRoot -Recurse -Force
}
if (Test-Path -LiteralPath $archivePath) {
    Remove-Item -LiteralPath $archivePath -Force
}

& dotnet test $solutionPath -c Release "/p:RatopiaDir=$GameDir" /p:InstallAfterBuild=false --nologo
if ($LASTEXITCODE -ne 0) {
    throw "Release 测试失败，退出码 $LASTEXITCODE。"
}

& dotnet build $solutionPath -c Release "/p:RatopiaDir=$GameDir" /p:InstallAfterBuild=false --no-restore --nologo
if ($LASTEXITCODE -ne 0) {
    throw "Release 构建失败，退出码 $LASTEXITCODE。"
}

if (-not (Test-Path -LiteralPath $pluginBuildPath -PathType Leaf)) {
    throw "构建未生成插件 DLL：$pluginBuildPath"
}

New-Item -ItemType Directory -Path $distRoot -Force | Out-Null

try {
    New-Item -ItemType Directory -Path $stagePluginRoot -Force | Out-Null
    Copy-Item -LiteralPath $pluginBuildPath -Destination $stagePluginRoot
    Copy-Item -LiteralPath (Join-Path $projectRoot 'README.md') -Destination $stageRoot
    Compress-Archive -Path (Join-Path $stageRoot '*') -DestinationPath $archivePath -CompressionLevel Optimal
}
finally {
    if (Test-Path -LiteralPath $stageRoot) {
        Remove-Item -LiteralPath $stageRoot -Recurse -Force
    }
}

& dotnet test $testProjectPath -c Release "/p:RatopiaDir=$GameDir" /p:InstallAfterBuild=false `
    --no-build --no-restore --nologo `
    --filter 'FullyQualifiedName~VersionedReleaseArchiveContainsOnlyThePluginAndReadmeWhenPresent'
if ($LASTEXITCODE -ne 0) {
    throw "发布包内容与构建产物一致性测试失败，退出码 $LASTEXITCODE。"
}

[pscustomobject]@{
    Archive = $archivePath
    SHA256 = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash
    Plugin = $pluginBuildPath
}
