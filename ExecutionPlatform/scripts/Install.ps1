param(
    [string]$RatopiaDir = $env:RATOPIA_DIR,
    [string]$PluginPath,
    [Parameter(Mandatory = $true)]
    [string]$TestSavePath
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RatopiaDir)) {
    throw '请通过 -RatopiaDir 或 RATOPIA_DIR 指定 Ratopia 游戏目录。'
}

$projectRoot = [System.IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$ratopiaRoot = [System.IO.Path]::GetFullPath($RatopiaDir)
if ([string]::IsNullOrWhiteSpace($PluginPath)) {
    $PluginPath = Join-Path $projectRoot 'src\ExecutionPlatform\bin\Release\net472\ExecutionPlatform.dll'
}
$sourcePlugin = [System.IO.Path]::GetFullPath($PluginPath)
$targetPlugin = Join-Path $ratopiaRoot 'BepInEx\plugins\ExecutionPlatform\ExecutionPlatform.dll'
$playFileRoot = Join-Path $ratopiaRoot 'Ratopia_Data\SaveFile\PlayFile'
$testSave = [System.IO.Path]::GetFullPath($TestSavePath)
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss-fff'
$backupDir = Join-Path $projectRoot "backups\pre-install-$timestamp"
$saveBackupDir = Join-Path $backupDir 'TestSave'
$pluginBackupDir = Join-Path $backupDir 'Plugin'

function Assert-ChildPath([string]$Path, [string]$Parent) {
    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $fullParent = [System.IO.Path]::GetFullPath($Parent).TrimEnd('\') + '\'
    if (-not $fullPath.StartsWith($fullParent, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "路径不在预期目录内：$fullPath"
    }
}

$runningGame = Get-Process -Name 'Ratopia' -ErrorAction SilentlyContinue | Where-Object {
    $_.Path -and [System.IO.Path]::GetFullPath($_.Path).StartsWith(
        $ratopiaRoot.TrimEnd('\') + '\',
        [System.StringComparison]::OrdinalIgnoreCase)
}
if ($runningGame) {
    throw "Ratopia 仍在运行（PID：$($runningGame.Id -join ', ')）。请从游戏菜单正常退出后再安装。"
}

if (-not (Test-Path -LiteralPath $sourcePlugin -PathType Leaf)) {
    throw "找不到待安装插件：$sourcePlugin"
}
if (-not (Test-Path -LiteralPath $testSave)) {
    throw "找不到专用测试存档：$testSave"
}
Assert-ChildPath -Path $testSave -Parent $playFileRoot
Assert-ChildPath -Path $backupDir -Parent $projectRoot
Assert-ChildPath -Path $targetPlugin -Parent $ratopiaRoot

New-Item -ItemType Directory -Path $saveBackupDir -Force | Out-Null
Copy-Item -LiteralPath $testSave -Destination $saveBackupDir -Recurse -Force

if (Test-Path -LiteralPath $targetPlugin -PathType Leaf) {
    New-Item -ItemType Directory -Path $pluginBackupDir -Force | Out-Null
    Copy-Item -LiteralPath $targetPlugin -Destination (Join-Path $pluginBackupDir 'ExecutionPlatform.dll')
}

$targetDir = Split-Path -Parent $targetPlugin
New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
Copy-Item -LiteralPath $sourcePlugin -Destination $targetPlugin -Force

$sourceHash = (Get-FileHash -LiteralPath $sourcePlugin -Algorithm SHA256).Hash
$installedHash = (Get-FileHash -LiteralPath $targetPlugin -Algorithm SHA256).Hash
if ($sourceHash -ne $installedHash) {
    throw "安装后的 SHA-256 不匹配。源文件：$sourceHash；安装文件：$installedHash。"
}

Write-Host "专用测试存档和旧插件备份：$backupDir"
Write-Host "插件已安装：$targetPlugin"
Write-Host "SHA-256：$installedHash"
