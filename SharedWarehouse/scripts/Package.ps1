param(
    [string]$RatopiaDir = 'E:\steam\steamapps\common\Ratopia'
)

$ErrorActionPreference = 'Stop'
$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$distDir = Join-Path $projectRoot 'dist'
$stageDir = Join-Path $distDir 'package'
$pluginStageDir = Join-Path $stageDir 'BepInEx\plugins\SharedWarehouse'
$archivePath = Join-Path $distDir '共享仓库-v0.1.0-BepInEx5.zip'
$projectPath = Join-Path $projectRoot 'src\SharedWarehouse\SharedWarehouse.csproj'
$builtDll = Join-Path $projectRoot 'src\SharedWarehouse\bin\Release\net472\SharedWarehouse.dll'

if (-not (Test-Path -LiteralPath (Join-Path $RatopiaDir 'Ratopia_Data\Managed\Assembly-CSharp.dll'))) {
    throw "RatopiaDir 不正确：$RatopiaDir"
}

& dotnet build $projectPath -c Release "/p:RatopiaDir=$RatopiaDir" '/p:InstallAfterBuild=false' --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    throw "Release 构建失败，退出码：$LASTEXITCODE"
}

if (-not (Test-Path -LiteralPath $builtDll)) {
    throw "找不到构建产物：$builtDll"
}

if (Test-Path -LiteralPath $stageDir) {
    Remove-Item -LiteralPath $stageDir -Recurse -Force
}

New-Item -ItemType Directory -Path $pluginStageDir -Force | Out-Null
Copy-Item -LiteralPath $builtDll -Destination (Join-Path $pluginStageDir 'SharedWarehouse.dll')
Copy-Item -LiteralPath (Join-Path $projectRoot 'README.md') -Destination (Join-Path $stageDir 'README.md')
Copy-Item -LiteralPath (Join-Path $projectRoot 'NEXUS_DESCRIPTION.md') -Destination (Join-Path $stageDir 'NEXUS_DESCRIPTION.md')
Copy-Item -LiteralPath (Join-Path $projectRoot 'Launch_SharedWarehouse.cmd') -Destination (Join-Path $stageDir 'Launch_SharedWarehouse.cmd')

if (Test-Path -LiteralPath $archivePath) {
    Remove-Item -LiteralPath $archivePath -Force
}

Compress-Archive -Path (Join-Path $stageDir '*') -DestinationPath $archivePath -CompressionLevel Optimal
Remove-Item -LiteralPath $stageDir -Recurse -Force
Write-Output $archivePath
