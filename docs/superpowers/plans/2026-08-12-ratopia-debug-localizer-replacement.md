# Ratopia Debug Localizer Replacement Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the local `Admin.txt` Ratopia debug activation with the supplied YunQingLocalizer 0.2.0 BepInEx 5 plugin while preserving every other plugin, game assembly, configuration file, and save.

**Architecture:** Perform one guarded, transactional filesystem installation while Ratopia is stopped. Back up and remove the exact zero-byte activation file, extract only the two approved ZIP entries into a previously absent plugin directory, verify SHA-256 values, and restore the old activation automatically if installation fails.

**Tech Stack:** Windows PowerShell, .NET `System.IO.Compression`, Ratopia v1.0.0600 Mono, BepInEx 5.4.23.5, SHA-256 verification.

## Global Constraints

- Execute all work sequentially in the primary agent; the Ratopia Mod workflow forbids subagent execution.
- Ratopia must not be running during any filesystem change.
- Source ZIP must remain `D:\QQ\plugins-bep5.zip` with SHA-256 `18900DE0D3FDC3B4155D97665050EDC973B9FB10903AD9B96F0353A12C4DA9DA`.
- Move only `E:\steam\steamapps\common\Ratopia\Ratopia_Data\Log\Admin.txt` out of the game directory.
- Install only `RatopiaMod.YunQing.Localizer.dll` and `CheatPanelChinese.json` under `E:\steam\steamapps\common\Ratopia\BepInEx\plugins\YunQingLocalizer`.
- Do not modify game assemblies, BepInEx configuration, other plugins, or saves.
- The current workspace is not a Git repository, so no commit step is available.

---

### Task 1: Guarded replacement and rollback protection

**Files:**
- Move: `E:\steam\steamapps\common\Ratopia\Ratopia_Data\Log\Admin.txt`
- Create: `D:\SOFTWARE\项目\鼠托邦mod\backups\ratopia-debug-replacement-yyyyMMdd-HHmmss\Admin.txt`
- Create: `E:\steam\steamapps\common\Ratopia\BepInEx\plugins\YunQingLocalizer\RatopiaMod.YunQing.Localizer.dll`
- Create: `E:\steam\steamapps\common\Ratopia\BepInEx\plugins\YunQingLocalizer\CheatPanelChinese.json`

**Interfaces:**
- Consumes: the exact two approved entries in `D:\QQ\plugins-bep5.zip` and the existing zero-byte `Admin.txt`.
- Produces: a recoverable backup directory and an installed BepInEx plugin directory whose two files match the ZIP entries byte-for-byte.

- [x] **Step 1: Run preflight checks without changing files**

Run a PowerShell guard that verifies:

```powershell
$game = 'E:\steam\steamapps\common\Ratopia'
$zipPath = 'D:\QQ\plugins-bep5.zip'
$adminPath = "$game\Ratopia_Data\Log\Admin.txt"
$targetDir = "$game\BepInEx\plugins\YunQingLocalizer"

if (Get-Process -Name Ratopia -ErrorAction SilentlyContinue) { throw 'Ratopia is still running.' }
if (-not (Test-Path -LiteralPath $zipPath -PathType Leaf)) { throw 'Source ZIP is missing.' }
if ((Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash -ne '18900DE0D3FDC3B4155D97665050EDC973B9FB10903AD9B96F0353A12C4DA9DA') { throw 'Source ZIP hash mismatch.' }
if (-not (Test-Path -LiteralPath $adminPath -PathType Leaf)) { throw 'Admin.txt is missing; replacement state is not the approved state.' }
if ((Get-Item -LiteralPath $adminPath).Length -ne 0) { throw 'Admin.txt is not the approved zero-byte file.' }
if ((Get-FileHash -LiteralPath $adminPath -Algorithm SHA256).Hash -ne 'E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855') { throw 'Admin.txt hash mismatch.' }
if (Test-Path -LiteralPath $targetDir) { throw 'YunQingLocalizer target already exists.' }
```

Expected: no exception and no changed filesystem state.

- [x] **Step 2: Snapshot protected files in memory**

Before installation, enumerate SHA-256 values for `Assembly-CSharp.dll` and all existing files under `BepInEx/plugins`. Exclude no existing path because `YunQingLocalizer` was proven absent.

```powershell
$protectedPaths = @("$game\Ratopia_Data\Managed\Assembly-CSharp.dll") + @(
    Get-ChildItem -LiteralPath "$game\BepInEx\plugins" -Recurse -File | ForEach-Object FullName
)
$beforeHashes = @{}
foreach ($path in $protectedPaths) {
    $beforeHashes[$path] = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
}
```

Expected: every protected path maps to one SHA-256 value; `Assembly-CSharp.dll` maps to `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`.

- [x] **Step 3: Back up the activation file and install only the approved entries**

Use a `try/catch` transaction. The backup directory uses the real execution time, not a fixed placeholder. ZIP entries are addressed by exact name, so additional or unsafe paths cannot be extracted.

```powershell
Add-Type -AssemblyName System.IO.Compression.FileSystem
$workspace = 'D:\SOFTWARE\项目\鼠托邦mod'
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$backupDir = Join-Path $workspace "backups\ratopia-debug-replacement-$stamp"
$backupAdmin = Join-Path $backupDir 'Admin.txt'
$dllPath = Join-Path $targetDir 'RatopiaMod.YunQing.Localizer.dll'
$jsonPath = Join-Path $targetDir 'CheatPanelChinese.json'
$createdTarget = $false
$adminMoved = $false

try {
    New-Item -ItemType Directory -Path $backupDir -ErrorAction Stop | Out-Null
    Move-Item -LiteralPath $adminPath -Destination $backupAdmin -ErrorAction Stop
    $adminMoved = $true

    New-Item -ItemType Directory -Path $targetDir -ErrorAction Stop | Out-Null
    $createdTarget = $true

    $archive = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
    try {
        $dllEntry = $archive.GetEntry('plugins/YunQingLocalizer/RatopiaMod.YunQing.Localizer.dll')
        $jsonEntry = $archive.GetEntry('plugins/YunQingLocalizer/CheatPanelChinese.json')
        if ($null -eq $dllEntry -or $null -eq $jsonEntry) { throw 'Approved ZIP entries are missing.' }
        [System.IO.Compression.ZipFileExtensions]::ExtractToFile($dllEntry, $dllPath, $false)
        [System.IO.Compression.ZipFileExtensions]::ExtractToFile($jsonEntry, $jsonPath, $false)
    }
    finally {
        if ($null -ne $archive) { $archive.Dispose() }
    }
}
catch {
    if (Test-Path -LiteralPath $dllPath -PathType Leaf) { Remove-Item -LiteralPath $dllPath -Force }
    if (Test-Path -LiteralPath $jsonPath -PathType Leaf) { Remove-Item -LiteralPath $jsonPath -Force }
    if ($createdTarget -and (Test-Path -LiteralPath $targetDir -PathType Container)) { Remove-Item -LiteralPath $targetDir -Force }
    if ($adminMoved -and -not (Test-Path -LiteralPath $adminPath)) { Move-Item -LiteralPath $backupAdmin -Destination $adminPath }
    throw
}
```

Expected: `Admin.txt` exists only in the timestamped backup directory, and the target plugin directory contains exactly the DLL and JSON.

- [x] **Step 4: Verify installed hashes and protected paths**

```powershell
try {
    $expectedInstalled = @{
        $dllPath  = '3EA66A55C3220374E061751F953DC9B9B13E32A284657C697FE5CC183A4E9B10'
        $jsonPath = '610D5B2946A32AB83EA9B56B2BD07CAAB466E76110F56033EDEA9F0AAEC10730'
    }
    foreach ($pair in $expectedInstalled.GetEnumerator()) {
        if (-not (Test-Path -LiteralPath $pair.Key -PathType Leaf)) { throw "Installed file missing: $($pair.Key)" }
        if ((Get-FileHash -LiteralPath $pair.Key -Algorithm SHA256).Hash -ne $pair.Value) { throw "Installed hash mismatch: $($pair.Key)" }
    }
    if (Test-Path -LiteralPath $adminPath) { throw 'Old Admin.txt activation still exists in the game directory.' }
    if ((Get-FileHash -LiteralPath $backupAdmin -Algorithm SHA256).Hash -ne 'E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855') { throw 'Backup Admin.txt hash mismatch.' }
    foreach ($pair in $beforeHashes.GetEnumerator()) {
        if (-not (Test-Path -LiteralPath $pair.Key -PathType Leaf)) { throw "Protected file missing: $($pair.Key)" }
        if ((Get-FileHash -LiteralPath $pair.Key -Algorithm SHA256).Hash -ne $pair.Value) { throw "Protected file changed: $($pair.Key)" }
    }
    $unexpected = @(Get-ChildItem -LiteralPath $targetDir -File | Where-Object Name -notin 'RatopiaMod.YunQing.Localizer.dll','CheatPanelChinese.json')
    if ($unexpected.Count -ne 0) { throw 'Unexpected files exist in the installed plugin directory.' }
}
catch {
    if (Test-Path -LiteralPath $dllPath -PathType Leaf) { Remove-Item -LiteralPath $dllPath -Force }
    if (Test-Path -LiteralPath $jsonPath -PathType Leaf) { Remove-Item -LiteralPath $jsonPath -Force }
    if (Test-Path -LiteralPath $targetDir -PathType Container) { Remove-Item -LiteralPath $targetDir -Force }
    if (-not (Test-Path -LiteralPath $adminPath) -and (Test-Path -LiteralPath $backupAdmin -PathType Leaf)) { Move-Item -LiteralPath $backupAdmin -Destination $adminPath }
    throw
}
```

Expected: all checks pass, exactly two new plugin files exist, no protected file changed, and the old activation path is absent.

---

### Task 2: Runtime acceptance handoff

**Files:**
- Inspect after user launch: `E:\steam\steamapps\common\Ratopia\BepInEx\LogOutput.log`
- Inspect after user launch: `C:\Users\ASUS\AppData\LocalLow\CasselGames\Ratopia\Player.log`

**Interfaces:**
- Consumes: the statically verified installed plugin from Task 1 and one user-started Ratopia session.
- Produces: separate evidence for plugin discovery, F3 behavior, Chinese localization, and runtime exceptions.

- [ ] **Step 1: Ask the user to launch Ratopia and enter a save**

The user should press F3 once to open the panel and once to close it. During acceptance, they should not click any resource, character, time, or world-changing cheat button.

Expected: F3 toggles the Cheat panel; mapped labels display Chinese. F8/F4 behavior previously unlocked by `Admin.txt` is outside the replacement design.

- [ ] **Step 2: Inspect fresh logs after the user test**

Run:

```powershell
Select-String -LiteralPath 'E:\steam\steamapps\common\Ratopia\BepInEx\LogOutput.log' -Pattern 'Loading \[YunQingLocalizer 0\.2\.0\]|YunQing|Exception|Harmony' -CaseSensitive:$false
Select-String -LiteralPath 'C:\Users\ASUS\AppData\LocalLow\CasselGames\Ratopia\Player.log' -Pattern 'YunQing|Exception|Harmony' -CaseSensitive:$false
```

Expected: BepInEx reports `Loading [YunQingLocalizer 0.2.0]`; neither fresh log contains a new exception attributable to YunQingLocalizer or its two Harmony patches.
