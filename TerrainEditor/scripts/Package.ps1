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
$ratopiaRoot = (Resolve-Path -LiteralPath $RatopiaDir).Path
$gameAssembly = Join-Path $ratopiaRoot 'Ratopia_Data\Managed\Assembly-CSharp.dll'
$bepInExAssembly = Join-Path $ratopiaRoot 'BepInEx\core\BepInEx.dll'
if (-not (Test-Path -LiteralPath $gameAssembly) -or -not (Test-Path -LiteralPath $bepInExAssembly)) {
    throw "RatopiaDir 无效或未安装 BepInEx 5：$ratopiaRoot"
}

$env:RATOPIA_DIR = $ratopiaRoot
$solution = Join-Path $projectRoot 'TerrainEditor.sln'
$pluginProject = Join-Path $projectRoot 'src\TerrainEditor\TerrainEditor.csproj'
& dotnet test $solution -c Release /p:InstallAfterBuild=false
if ($LASTEXITCODE -ne 0) { throw 'Release 测试失败，已停止打包。' }

& dotnet build $pluginProject -c Release /p:InstallAfterBuild=false --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Release 构建失败，已停止打包。' }

$distDir = Join-Path $projectRoot 'dist'
$stageDir = Join-Path $projectRoot 'artifacts\package'
$pluginStageDir = Join-Path $stageDir 'BepInEx\plugins\TerrainEditor'
$zipPath = Join-Path $distDir '地形编辑器-v0.1.0-BepInEx5.zip'
$pluginDll = Join-Path $projectRoot 'src\TerrainEditor\bin\Release\net472\TerrainEditor.dll'

if (-not $stageDir.StartsWith($projectRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "暂存目录不在项目内，拒绝清理：$stageDir"
}
if (-not $zipPath.StartsWith($projectRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "发布包不在项目内，拒绝覆盖：$zipPath"
}

if (Test-Path -LiteralPath $stageDir) {
    Remove-Item -LiteralPath $stageDir -Recurse -Force
}
New-Item -ItemType Directory -Path $pluginStageDir -Force | Out-Null
New-Item -ItemType Directory -Path $distDir -Force | Out-Null
Copy-Item -LiteralPath $pluginDll -Destination (Join-Path $pluginStageDir 'TerrainEditor.dll')
Copy-Item -LiteralPath (Join-Path $projectRoot 'README.md') -Destination (Join-Path $stageDir 'README.md')

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
    $names = @($archive.Entries |
        Where-Object { -not [string]::IsNullOrEmpty($_.Name) } |
        ForEach-Object { $_.FullName.Replace('\', '/') })
    $expected = @(
        'BepInEx/plugins/TerrainEditor/TerrainEditor.dll',
        'README.md'
    )
    $unexpected = @($names | Where-Object { $_ -notin $expected })
    $missing = @($expected | Where-Object { $_ -notin $names })
    $forbidden = @($names | Where-Object {
        $name = [IO.Path]::GetFileName($_)
        $name -ieq 'Assembly-CSharp.dll' -or
        $name -ieq 'Assembly-CSharp-firstpass.dll' -or
        $name -ieq '0Harmony.dll' -or
        $name -like 'UnityEngine*.dll' -or
        $name -like 'BepInEx*.dll' -or
        $name -like '*.pdb'
    })

    if ($unexpected.Count -gt 0 -or $missing.Count -gt 0 -or $forbidden.Count -gt 0) {
        throw "发布包内容不符合约定。缺少：$($missing -join ', ')；多余：$($unexpected -join ', ')；禁止：$($forbidden -join ', ')"
    }
}
finally {
    $archive.Dispose()
}

$hash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash
Write-Host "发布包：$zipPath"
Write-Host "SHA-256：$hash"
