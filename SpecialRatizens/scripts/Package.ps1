param(
    [string]$RatopiaDir = $env:RATOPIA_DIR,
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RatopiaDir)) {
    throw '请通过 -RatopiaDir 或 RATOPIA_DIR 指定《鼠托邦》游戏目录。'
}

$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$ratopiaRoot = [IO.Path]::GetFullPath([Environment]::ExpandEnvironmentVariables($RatopiaDir))
$gameAssembly = Join-Path $ratopiaRoot 'Ratopia_Data\Managed\Assembly-CSharp.dll'
$bepInExAssembly = Join-Path $ratopiaRoot 'BepInEx\core\BepInEx.dll'
$solutionPath = Join-Path $projectRoot 'SpecialRatizens.sln'
$projectPath = Join-Path $projectRoot 'src\SpecialRatizens\SpecialRatizens.csproj'
$builtDll = Join-Path $projectRoot 'src\SpecialRatizens\bin\Release\net472\SpecialRatizens.dll'
$dataRoot = Join-Path $projectRoot 'Data'
$readmePath = Join-Path $projectRoot 'README.md'
$distRoot = Join-Path $projectRoot 'dist'
$packageRoot = Join-Path $distRoot 'package'
$pluginRoot = Join-Path $packageRoot 'BepInEx\plugins\SpecialRatizens'
$archivePath = Join-Path $distRoot '特殊鼠鼠-v0.1.4-BepInEx5.zip'
$msbuildRatopiaDir = "/p:RatopiaDir=$ratopiaRoot"

function Assert-ProjectPath([string]$Path, [string]$Label) {
    $resolved = [IO.Path]::GetFullPath($Path)
    $prefix = $projectRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    if (-not $resolved.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "$Label 必须位于项目目录内：$resolved"
    }
}

if (-not (Test-Path -LiteralPath $gameAssembly -PathType Leaf)) {
    throw "找不到游戏程序集：$gameAssembly"
}
if (-not (Test-Path -LiteralPath $bepInExAssembly -PathType Leaf)) {
    throw "找不到 BepInEx 5：$bepInExAssembly"
}
if (-not (Test-Path -LiteralPath $dataRoot -PathType Container)) {
    throw "找不到发布数据目录：$dataRoot"
}

Assert-ProjectPath $distRoot '发布目录'
Assert-ProjectPath $packageRoot '暂存目录'
Assert-ProjectPath $archivePath '发布压缩包'

if (-not $SkipTests) {
    & dotnet test $solutionPath -c Release $msbuildRatopiaDir /p:InstallAfterBuild=false --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        throw "测试失败，退出代码：$LASTEXITCODE"
    }
}

& dotnet build $projectPath -c Release $msbuildRatopiaDir /p:InstallAfterBuild=false --no-restore --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    throw "构建失败，退出代码：$LASTEXITCODE"
}

if (-not (Test-Path -LiteralPath $builtDll -PathType Leaf)) {
    throw "构建未生成插件 DLL：$builtDll"
}

New-Item -ItemType Directory -Path $distRoot -Force | Out-Null
if (Test-Path -LiteralPath $packageRoot) {
    Remove-Item -LiteralPath $packageRoot -Recurse -Force
}
if (Test-Path -LiteralPath $archivePath) {
    Remove-Item -LiteralPath $archivePath -Force
}

New-Item -ItemType Directory -Path $pluginRoot -Force | Out-Null
Copy-Item -LiteralPath $builtDll -Destination (Join-Path $pluginRoot 'SpecialRatizens.dll')
Copy-Item -LiteralPath $dataRoot -Destination (Join-Path $pluginRoot 'Data') -Recurse
Copy-Item -LiteralPath $readmePath -Destination (Join-Path $packageRoot 'README.md')

Push-Location $packageRoot
try {
    Compress-Archive -Path 'BepInEx', 'README.md' -DestinationPath $archivePath -CompressionLevel Optimal
}
finally {
    Pop-Location
}

$hash = Get-FileHash -LiteralPath $archivePath -Algorithm SHA256
Write-Host "发布目录：$packageRoot"
Write-Host "发布压缩包：$archivePath"
Write-Host "压缩包 SHA-256：$($hash.Hash)"
