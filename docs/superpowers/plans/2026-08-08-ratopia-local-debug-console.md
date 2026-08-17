# Ratopia Local Debug Console Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Enable Ratopia v1.0.0600's built-in local debug tools without modifying the SharedWarehouse Mod or any game assembly.

**Architecture:** Use the game's existing `U_SaveData.LoadAll` activation path by creating `Ratopia_Data\Log\Admin.txt`. The game itself will set `Defines.IsPublicVersion=false` and `Defines.Cheat=true`; native keys remain F8 for the full Cheat/Palette panel, F3 for tile coordinates, and F4 for electrical ports.

**Tech Stack:** Windows filesystem, Ratopia v1.0.0600 built-in debug code, PowerShell verification.

## Global Constraints

- Do not modify `SharedWarehouse.dll`, SharedWarehouse source code, or `Assembly-CSharp.dll`.
- Create only `E:\steam\steamapps\common\Ratopia\Ratopia_Data\Log\Admin.txt` in the game installation.
- Preserve an existing `Admin.txt` without overwriting its contents.
- Do not touch or rewrite any save file.
- The game process must not be running while the activation file is created.
- Removing `Admin.txt` must fully disable this local activation path.

---

### Task 1: Enable and verify the native debug tools

**Files:**
- Create: `E:\steam\steamapps\common\Ratopia\Ratopia_Data\Log\Admin.txt`
- Verify unchanged: `E:\steam\steamapps\common\Ratopia\BepInEx\plugins\SharedWarehouse\SharedWarehouse.dll`
- Verify unchanged: `E:\steam\steamapps\common\Ratopia\Ratopia_Data\Managed\Assembly-CSharp.dll`

**Interfaces:**
- Consumes: `U_SaveData.LoadAll` checks `UnityEngine.Application.dataPath + "/Log/Admin.txt"`.
- Produces: On the next game launch, the existing game code sets `Defines.IsPublicVersion=false` and `Defines.Cheat=true`.

- [ ] **Step 1: Verify the game is stopped and capture protected-file hashes**

Run:

```powershell
$game = 'E:\steam\steamapps\common\Ratopia'
if (Get-Process -Name Ratopia -ErrorAction SilentlyContinue) { throw 'Ratopia is still running.' }
Get-FileHash -Algorithm SHA256 -LiteralPath `
  "$game\BepInEx\plugins\SharedWarehouse\SharedWarehouse.dll", `
  "$game\Ratopia_Data\Managed\Assembly-CSharp.dll"
```

Expected: no `Ratopia` process error; two SHA-256 hashes are printed.

- [ ] **Step 2: Verify the activation file state without overwriting it**

Run:

```powershell
$admin = 'E:\steam\steamapps\common\Ratopia\Ratopia_Data\Log\Admin.txt'
if (Test-Path -LiteralPath $admin) {
    Get-Item -LiteralPath $admin | Select-Object FullName, Length, LastWriteTime
} else {
    'ADMIN_FILE_ABSENT'
}
```

Expected: either `ADMIN_FILE_ABSENT`, or the existing file metadata is printed and the create step is skipped.

- [ ] **Step 3: Create the empty activation file when absent**

Apply this patch only if Step 2 returned `ADMIN_FILE_ABSENT`:

```diff
*** Begin Patch
*** Add File: E:\steam\steamapps\common\Ratopia\Ratopia_Data\Log\Admin.txt
*** End Patch
```

Expected: `Admin.txt` exists and is zero bytes. No Mod or assembly file changes.

- [ ] **Step 4: Verify the file and protected hashes**

Run:

```powershell
$game = 'E:\steam\steamapps\common\Ratopia'
$admin = "$game\Ratopia_Data\Log\Admin.txt"
$item = Get-Item -LiteralPath $admin
if ($item.Length -ne 0) { throw "Admin.txt was expected to be empty but is $($item.Length) bytes." }
Get-FileHash -Algorithm SHA256 -LiteralPath `
  "$game\BepInEx\plugins\SharedWarehouse\SharedWarehouse.dll", `
  "$game\Ratopia_Data\Managed\Assembly-CSharp.dll"
```

Expected: `Admin.txt` is zero bytes and both protected-file hashes exactly match Step 1.

- [ ] **Step 5: Perform the in-game acceptance check**

Launch Ratopia, enter a save, and verify:

1. `F8` opens and closes the complete Cheat/Palette panel.
2. `F3` toggles tile-coordinate debug display.
3. `F4` toggles electrical-port debug display.
4. `Player.log` contains no new exception caused by activation.

Expected: all three native key actions work; no new fatal or unhandled exception appears.

- [ ] **Step 6: Record rollback instructions**

To disable the local debug activation after the game is closed:

```powershell
Remove-Item -LiteralPath 'E:\steam\steamapps\common\Ratopia\Ratopia_Data\Log\Admin.txt'
```

Expected: the next launch uses the public-version path again. This rollback does not modify Mod files or saves.
