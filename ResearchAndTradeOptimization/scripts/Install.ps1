param(
    [string]$GameDir = 'E:\steam\steamapps\common\Ratopia',
    [string]$PluginDll
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$projectRoot = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
if ([string]::IsNullOrWhiteSpace($PluginDll)) {
    $PluginDll = Join-Path $projectRoot 'src\ResearchAndTradeOptimization\bin\Release\net472\ResearchAndTradeOptimization.dll'
}

$ratopiaProcesses = @(Get-Process -Name 'Ratopia' -ErrorAction SilentlyContinue)
if ($ratopiaProcesses.Count -gt 0) {
    throw "Ratopia 仍在运行（PID：$($ratopiaProcesses.Id -join ', ')）。请正常退出游戏后再安装；脚本不会关闭游戏。"
}

if (-not (Test-Path -LiteralPath $PluginDll -PathType Leaf)) {
    throw "找不到待安装插件：$PluginDll"
}

$targetRoot = Join-Path $GameDir 'BepInEx\plugins\ResearchAndTradeOptimization'
$targetDll = Join-Path $targetRoot 'ResearchAndTradeOptimization.dll'
New-Item -ItemType Directory -Path $targetRoot -Force | Out-Null
Copy-Item -LiteralPath $PluginDll -Destination $targetDll -Force

$sourceHash = (Get-FileHash -LiteralPath $PluginDll -Algorithm SHA256).Hash
$targetHash = (Get-FileHash -LiteralPath $targetDll -Algorithm SHA256).Hash
if (-not $sourceHash.Equals($targetHash, [StringComparison]::OrdinalIgnoreCase)) {
    throw "安装后 DLL 哈希校验失败。源：$sourceHash；目标：$targetHash。"
}

[pscustomobject]@{
    Installed = $targetDll
    SHA256 = $targetHash
}
