# 特殊鼠鼠存档回滚执行计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking. Ratopia 专用技能禁止委派子代理，因此只能由主代理顺序执行。

**Goal:** 在不删除、覆盖或重写任何游戏存档的前提下，完整备份当前真实存档并验证 `鼠托邦_103.zip` 是可供用户手动选择的干净恢复点。

**Architecture:** 文件层只执行只读检查和一次可恢复的完整复制；源 `SaveFile` 树始终原地保留。恢复点通过 ZIP 完整性、成员结构、SHA-256 和 BinaryFormatter 只读反序列化四层验证，实际游戏内回滚由用户在读档界面选择现有自动存档完成。

**Tech Stack:** Windows PowerShell、7-Zip 26.01、Windows PowerShell 5.1/.NET Framework BinaryFormatter、Ratopia 1.0.0600、BepInEx 5.4.23.5、SpecialRatizens 0.1.1。

## Global Constraints

- 游戏目录固定为 `E:\steam\steamapps\common\Ratopia`。
- 实际存档根目录固定为 `E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile`。
- 恢复点固定为 `E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile\PlayFile\鼠托邦_W00\AutoSave\鼠托邦_103.zip`。
- 不删除、不覆盖、不改写任何现有存档文件。
- 不启动鼠托邦，不自动保存游戏。
- 必须在 `Ratopia.exe` 未运行时创建备份。
- 保留 `SpecialRatizens.dll` 0.1.1.0，不修改其他模组或配置。
- 备份必须比较相对路径集合、文件数量和每个文件的 SHA-256。
- 当前目录不是 Git 仓库，因此计划不包含提交、合并或工作树清理操作。

---

## File Structure

- Read: `E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile\**\*` — 当前真实存档源树。
- Read: `E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile\PlayFile\鼠托邦_W00\AutoSave\鼠托邦_103.zip` — 已识别的干净恢复点。
- Read: `E:\steam\steamapps\common\Ratopia\BepInEx\plugins\SpecialRatizens\SpecialRatizens.dll` — 已安装版本门禁。
- Read: `D:\SOFTWARE\项目\鼠托邦mod\SpecialRatizens\diagnostics\save-skin-investigation\InspectSave.ps1` — 只读反序列化检查器。
- Create: `D:\SOFTWARE\项目\鼠托邦mod\SpecialRatizens\backups\pre-save-rollback-<运行时时间戳>\SaveFile\**\*` — 当前完整存档的离线副本；时间戳由 `Get-Date -Format 'yyyyMMdd-HHmmss'` 生成。
- Preserve: `D:\SOFTWARE\项目\鼠托邦mod\SpecialRatizens\dist\特殊鼠鼠-v0.1.1-BepInEx5.zip` — 现有模组包，不重新打包。

### Task 1: 安装与存档安全门禁

**Files:**
- Read: `E:\steam\steamapps\common\Ratopia\BepInEx\plugins\SpecialRatizens\SpecialRatizens.dll`
- Read: `E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile`
- Read: `E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile\PlayFile\鼠托邦_W00\AutoSave\鼠托邦_103.zip`

**Interfaces:**
- Consumes: 已批准的回滚规格和固定路径。
- Produces: 游戏关闭、v0.1.1 已安装、存档树与恢复点存在的门禁结果。

- [ ] **Step 1: 检查游戏进程、固定路径与已安装版本**

Run:

```powershell
$ErrorActionPreference = 'Stop'
$gameRoot = 'E:\steam\steamapps\common\Ratopia'
$saveRoot = Join-Path $gameRoot 'Ratopia_Data\SaveFile'
$recoveryZip = Join-Path $saveRoot 'PlayFile\鼠托邦_W00\AutoSave\鼠托邦_103.zip'
$installedDll = Join-Path $gameRoot 'BepInEx\plugins\SpecialRatizens\SpecialRatizens.dll'

if (Get-Process -Name 'Ratopia' -ErrorAction SilentlyContinue) {
    throw '鼠托邦仍在运行，禁止创建回滚前备份。'
}
foreach ($path in @($gameRoot, $saveRoot, $recoveryZip, $installedDll)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "缺少必要路径：$path"
    }
}
$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($installedDll).FileVersion
if ($version -ne '0.1.1.0') {
    throw "安装版本不符：$version"
}

[pscustomobject]@{
    RatopiaRunning = $false
    InstalledVersion = $version
    SaveFileCount = @(Get-ChildItem -LiteralPath $saveRoot -File -Recurse).Count
    RecoveryZipBytes = (Get-Item -LiteralPath $recoveryZip).Length
    RecoveryZipModified = (Get-Item -LiteralPath $recoveryZip).LastWriteTime
} | Format-List *
```

Expected:

- `RatopiaRunning : False`
- `InstalledVersion : 0.1.1.0`
- `SaveFileCount` 大于 0
- `RecoveryZipModified : 2026/8/8 23:09:39`

- [ ] **Step 2: 确认没有任何写入发生**

Run:

```powershell
Get-Process -Name 'Ratopia' -ErrorAction SilentlyContinue
```

Expected: 无输出，退出后仍没有鼠托邦进程。

### Task 2: 完整备份当前真实存档

**Files:**
- Read: `E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile\**\*`
- Create: `D:\SOFTWARE\项目\鼠托邦mod\SpecialRatizens\backups\pre-save-rollback-<运行时时间戳>\SaveFile\**\*`

**Interfaces:**
- Consumes: Task 1 的关闭进程和路径门禁。
- Produces: 与源 `SaveFile` 树逐文件 SHA-256 一致的完整备份目录。

- [ ] **Step 1: 复制前计算源存档树哈希并创建时间戳备份目录**

Run:

```powershell
$ErrorActionPreference = 'Stop'
$projectRoot = 'D:\SOFTWARE\项目\鼠托邦mod\SpecialRatizens'
$saveRoot = 'E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile'
$backupRoot = Join-Path $projectRoot ('backups\pre-save-rollback-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
$saveBackup = Join-Path $backupRoot 'SaveFile'

function Get-TreeHashMap([string]$root) {
    $map = @{}
    foreach ($file in Get-ChildItem -LiteralPath $root -File -Recurse) {
        $relative = $file.FullName.Substring($root.Length).TrimStart('\')
        $map[$relative] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    }
    return $map
}

if (Get-Process -Name 'Ratopia' -ErrorAction SilentlyContinue) {
    throw '鼠托邦已启动，已中止备份。'
}
$sourceMap = Get-TreeHashMap $saveRoot
New-Item -ItemType Directory -Path $saveBackup -Force | Out-Null
foreach ($item in Get-ChildItem -LiteralPath $saveRoot -Force) {
    Copy-Item -LiteralPath $item.FullName -Destination $saveBackup -Recurse -Force
}

$backupMap = Get-TreeHashMap $saveBackup
$differences = @(
    $sourceMap.Keys | Where-Object { -not $backupMap.ContainsKey($_) -or $backupMap[$_] -ne $sourceMap[$_] }
    $backupMap.Keys | Where-Object { -not $sourceMap.ContainsKey($_) }
) | Sort-Object -Unique
if ($differences.Count -ne 0) {
    throw "存档备份校验失败：$($differences -join ', ')"
}

[pscustomobject]@{
    BackupRoot = $backupRoot
    SourceFiles = $sourceMap.Count
    BackupFiles = $backupMap.Count
    HashDifferences = $differences.Count
    RatopiaRunning = [bool](Get-Process -Name 'Ratopia' -ErrorAction SilentlyContinue)
} | Format-List *
```

Expected:

- `SourceFiles` 与 `BackupFiles` 相等。
- `HashDifferences : 0`
- `RatopiaRunning : False`
- 输出一个实际存在的 `pre-save-rollback-YYYYMMDD-HHMMSS` 目录。

- [ ] **Step 2: 独立复查最新备份目录与源目录**

Run:

```powershell
$projectRoot = 'D:\SOFTWARE\项目\鼠托邦mod\SpecialRatizens'
$saveRoot = 'E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile'
$latestBackupRoot = Get-ChildItem -LiteralPath (Join-Path $projectRoot 'backups') -Directory |
    Where-Object Name -Like 'pre-save-rollback-*' |
    Sort-Object Name -Descending |
    Select-Object -First 1
$saveBackup = Join-Path $latestBackupRoot.FullName 'SaveFile'

[pscustomobject]@{
    BackupRoot = $latestBackupRoot.FullName
    SourceFiles = @(Get-ChildItem -LiteralPath $saveRoot -File -Recurse).Count
    BackupFiles = @(Get-ChildItem -LiteralPath $saveBackup -File -Recurse).Count
    SourceBytes = (Get-ChildItem -LiteralPath $saveRoot -File -Recurse | Measure-Object Length -Sum).Sum
    BackupBytes = (Get-ChildItem -LiteralPath $saveBackup -File -Recurse | Measure-Object Length -Sum).Sum
} | Format-List *
```

Expected: 文件数和总字节数分别相等。Task 2 Step 1 的 SHA-256 对比仍是最终完整性依据。

### Task 3: 验证干净恢复点

**Files:**
- Read: `E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile\PlayFile\鼠托邦_W00\AutoSave\鼠托邦_103.zip`
- Read: `D:\SOFTWARE\项目\鼠托邦mod\SpecialRatizens\diagnostics\save-skin-investigation\save103\鼠托邦_103.dat`
- Read: `D:\SOFTWARE\项目\鼠托邦mod\SpecialRatizens\diagnostics\save-skin-investigation\InspectSave.ps1`

**Interfaces:**
- Consumes: Task 2 的已验证完整备份。
- Produces: ZIP、成员、哈希和 99 名市民皮肤不变量的恢复点证据。

- [ ] **Step 1: 测试恢复 ZIP 完整性**

Run:

```powershell
& 'C:\Program Files\7-Zip\7z.exe' t 'E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile\PlayFile\鼠托邦_W00\AutoSave\鼠托邦_103.zip'
```

Expected: `Everything is Ok`，`Files: 3`。

- [ ] **Step 2: 验证 ZIP 成员结构和记录 SHA-256**

Run:

```powershell
$recoveryZip = 'E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile\PlayFile\鼠托邦_W00\AutoSave\鼠托邦_103.zip'
$listing = & 'C:\Program Files\7-Zip\7z.exe' l -slt $recoveryZip
$memberPaths = @($listing | Where-Object { $_ -match '^Path = .+\.(dat|json|png)$' })
$extensions = @($memberPaths | ForEach-Object {
    [IO.Path]::GetExtension(($_ -replace '^Path = ', '')).ToLowerInvariant()
} | Sort-Object)
if ($memberPaths.Count -ne 3 -or ($extensions -join ',') -ne '.dat,.json,.png') {
    throw "恢复 ZIP 成员不符：$($memberPaths -join ' | ')"
}
[pscustomobject]@{
    ArchiveSha256 = (Get-FileHash -LiteralPath $recoveryZip -Algorithm SHA256).Hash
    MemberCount = $memberPaths.Count
    Extensions = $extensions -join ', '
    MemberLines = $memberPaths
} | Format-List *
```

Expected: `MemberCount : 3` 且 `Extensions : .dat, .json, .png`。验证只依赖扩展名，因为 7-Zip 的 `-slt` 输出在当前控制台代码页会把中文成员名显示为乱码。

- [ ] **Step 3: 重新从 ZIP 提取只读诊断副本**

Run:

```powershell
$diagnosticRoot = 'D:\SOFTWARE\项目\鼠托邦mod\SpecialRatizens\diagnostics\save-skin-investigation\save103'
$recoveryZip = 'E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile\PlayFile\鼠托邦_W00\AutoSave\鼠托邦_103.zip'
& 'C:\Program Files\7-Zip\7z.exe' e $recoveryZip "-o$diagnosticRoot" -y
```

Expected: 提取成功；只覆盖项目内的诊断副本，不修改游戏存档 ZIP。

- [ ] **Step 4: 反序列化并验证每个必要皮肤类别**

Run:

```powershell
$output = powershell.exe -NoProfile -ExecutionPolicy Bypass `
    -File 'D:\SOFTWARE\项目\鼠托邦mod\SpecialRatizens\diagnostics\save-skin-investigation\InspectSave.ps1' `
    -Path 'D:\SOFTWARE\项目\鼠托邦mod\SpecialRatizens\diagnostics\save-skin-investigation\save103\鼠托邦_103.dat' `
    -SkinCategorySummary

$expected = @(
    'CATEGORY Skin PRESENT=99 NONEMPTY=99 TOTAL=99',
    'CATEGORY Face PRESENT=99 NONEMPTY=99 TOTAL=99',
    'CATEGORY Hair PRESENT=99 NONEMPTY=99 TOTAL=99',
    'CATEGORY Dress PRESENT=99 NONEMPTY=99 TOTAL=99'
)
foreach ($line in $expected) {
    if ($output -notcontains $line) {
        throw "恢复点皮肤不变量失败：$line"
    }
}
$output | Where-Object { $_ -match '^CATEGORY (Skin|Face|Hair|Dress) ' }
```

Expected: 四行都显示 `PRESENT=99 NONEMPTY=99 TOTAL=99`。

### Task 4: 最终无写入核验与人工回滚交付

**Files:**
- Read: `E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile\**\*`
- Read: `D:\SOFTWARE\项目\鼠托邦mod\SpecialRatizens\backups\pre-save-rollback-*\SaveFile\**\*`
- Read: `E:\steam\steamapps\common\Ratopia\BepInEx\plugins\SpecialRatizens\SpecialRatizens.dll`

**Interfaces:**
- Consumes: Task 2 的最新完整备份和 Task 3 的恢复点验证。
- Produces: 当前存档没有被离线流程改变、v0.1.1 仍安装、游戏未运行的最终证据，以及用户手动读档清单。

- [ ] **Step 1: 重新比较当前存档与本次备份的逐文件 SHA-256**

Run:

```powershell
$ErrorActionPreference = 'Stop'
$projectRoot = 'D:\SOFTWARE\项目\鼠托邦mod\SpecialRatizens'
$saveRoot = 'E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile'
$latestBackupRoot = Get-ChildItem -LiteralPath (Join-Path $projectRoot 'backups') -Directory |
    Where-Object Name -Like 'pre-save-rollback-*' |
    Sort-Object Name -Descending |
    Select-Object -First 1
$saveBackup = Join-Path $latestBackupRoot.FullName 'SaveFile'

function Get-TreeHashMap([string]$root) {
    $map = @{}
    foreach ($file in Get-ChildItem -LiteralPath $root -File -Recurse) {
        $relative = $file.FullName.Substring($root.Length).TrimStart('\')
        $map[$relative] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    }
    return $map
}

$expected = Get-TreeHashMap $saveBackup
$actual = Get-TreeHashMap $saveRoot
$differences = @(
    $expected.Keys | Where-Object { -not $actual.ContainsKey($_) -or $actual[$_] -ne $expected[$_] }
    $actual.Keys | Where-Object { -not $expected.ContainsKey($_) }
) | Sort-Object -Unique
if ($differences.Count -ne 0) {
    throw "当前存档在离线流程中发生变化：$($differences -join ', ')"
}

$installedDll = 'E:\steam\steamapps\common\Ratopia\BepInEx\plugins\SpecialRatizens\SpecialRatizens.dll'
[pscustomobject]@{
    BackupRoot = $latestBackupRoot.FullName
    SaveFilesVerified = $actual.Count
    SaveDifferences = $differences.Count
    InstalledVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($installedDll).FileVersion
    RatopiaRunning = [bool](Get-Process -Name 'Ratopia' -ErrorAction SilentlyContinue)
} | Format-List *
```

Expected:

- `SaveDifferences : 0`
- `InstalledVersion : 0.1.1.0`
- `RatopiaRunning : False`

- [ ] **Step 2: 向用户交付人工回滚清单**

交付内容必须明确写出：

1. 启动鼠托邦并进入读档界面。
2. 选择世界 `鼠托邦_W00`。
3. 选择自动存档 `鼠托邦_103`，时间为 `2026-08-08 23:09:39`。
4. 第一次进入后不要保存；检查身体、脸、头发、衣服和工作服切换。
5. 返回标题界面后再次读取 `鼠托邦_103`，重复检查。
6. 两轮正常后另存为新的手动存档，不覆盖 104–107。
7. 如仍异常，退出且不要保存，提供最新 `Player.log` 和 `BepInEx/LogOutput.log`。

Expected: 用户能够在不进行文件级覆盖的情况下选择干净恢复点，且所有较新存档仍可用于撤销。
