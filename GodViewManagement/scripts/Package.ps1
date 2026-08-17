[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$RatopiaDir = $env:RATOPIA_DIR
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RatopiaDir)) {
    throw '请通过 -RatopiaDir 或 RATOPIA_DIR 指定 Ratopia 游戏目录。'
}

$projectRoot = Split-Path -Parent $PSScriptRoot
$projectRoot = (Resolve-Path -LiteralPath $projectRoot).Path
$gameAssembly = Join-Path $RatopiaDir 'Ratopia_Data\Managed\Assembly-CSharp.dll'
$bepInExAssembly = Join-Path $RatopiaDir 'BepInEx\core\BepInEx.dll'
if (-not (Test-Path -LiteralPath $gameAssembly) -or -not (Test-Path -LiteralPath $bepInExAssembly)) {
    throw "RatopiaDir 无效或未安装 BepInEx 5：$RatopiaDir"
}

$env:RATOPIA_DIR = $RatopiaDir
$solution = Join-Path $projectRoot 'GodViewManagement.sln'
$pluginProject = Join-Path $projectRoot 'src\GodViewManagement\GodViewManagement.csproj'
& dotnet test $solution -c Release /p:InstallAfterBuild=false
if ($LASTEXITCODE -ne 0) { throw 'Release 测试失败，已停止打包。' }

& dotnet build $pluginProject -c Release /p:InstallAfterBuild=false --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Release 构建失败，已停止打包。' }

$distDir = Join-Path $projectRoot 'dist'
$stageDir = Join-Path $projectRoot 'artifacts\package'
$pluginStageDir = Join-Path $stageDir 'BepInEx\plugins\GodViewManagement'
$zipPath = Join-Path $distDir '上帝视角管理-v0.1.3-BepInEx5.zip'
$pluginDll = Join-Path $projectRoot 'src\GodViewManagement\bin\Release\net472\GodViewManagement.dll'

if ($stageDir.StartsWith($projectRoot, [System.StringComparison]::OrdinalIgnoreCase) -eq $false) {
    throw "暂存目录不在项目内，拒绝清理：$stageDir"
}

if (Test-Path -LiteralPath $stageDir) {
    Remove-Item -LiteralPath $stageDir -Recurse -Force
}
New-Item -ItemType Directory -Path $pluginStageDir -Force | Out-Null
New-Item -ItemType Directory -Path $distDir -Force | Out-Null
Copy-Item -LiteralPath $pluginDll -Destination (Join-Path $pluginStageDir 'GodViewManagement.dll')
Copy-Item -LiteralPath (Join-Path $projectRoot 'README.md') -Destination (Join-Path $stageDir 'README.md')

if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}
Compress-Archive -LiteralPath (Join-Path $stageDir 'BepInEx'), (Join-Path $stageDir 'README.md') -DestinationPath $zipPath -CompressionLevel Optimal

$entries = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $names = @($entries.Entries | ForEach-Object { $_.FullName.Replace('\', '/') } | Where-Object { -not $_.EndsWith('/') })
    $expected = @('BepInEx/plugins/GodViewManagement/GodViewManagement.dll', 'README.md')
    $unexpected = @($names | Where-Object { $_ -notin $expected })
    $missing = @($expected | Where-Object { $_ -notin $names })
    if ($unexpected.Count -gt 0 -or $missing.Count -gt 0) {
        throw "发布包内容不符合约定。缺少：$($missing -join ', ')；多余：$($unexpected -join ', ')"
    }
}
finally {
    $entries.Dispose()
}

$hash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash
Write-Host "发布包：$zipPath"
Write-Host "SHA-256：$hash"
