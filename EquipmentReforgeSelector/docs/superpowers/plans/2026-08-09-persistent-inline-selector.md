# Persistent Inline Reforge Selector Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Preserve the exact reforge choice across vanilla tooltip refreshes and make every candidate selectable through a full-width row or a number key.

**Architecture:** `PanelStateCoordinator` owns a selection session independently from the disposable Unity tooltip view. `RuntimeController` suspends the view for unrelated tooltip content and clears state only when the reforge detail context truly ends. `InlineReforgeSelectorView` renders reusable, full-width sibling hit areas over the original beige list and translates local input into candidate indexes.

**Tech Stack:** C# `net472`, BepInEx 5.4.23.5, Harmony 2.9.0.0, Unity 2021.3 UI/TMPro, xUnit, Mono.Cecil, PowerShell 5-compatible packaging.

## Global Constraints

- Game path is `E:\steam\steamapps\common\Ratopia`; target is Mono/BepInEx 5 and `net472`.
- Compatibility baseline remains `Assembly-CSharp.dll` SHA-256 `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`.
- `SimpleToolTipList.EnhanceEffect` is the only tooltip content whose original rows may be modified.
- A non-`EnhanceEffect` tooltip refresh must preserve the current exact `(AbilityId, Value)` selection for the same item and level.
- Item change, level change, detail close, scene change, plugin disable, and completed reforge must reset stale selection state.
- Preserve original candidate rules, values, material cost, achievement behavior, and `T_Queen.Dic_ItemPlusEffect` save format.
- Game, Unity, BepInEx, Harmony, Mono.Cecil, and test assemblies must not be copied into the package.
- Bugfix version is `0.1.2`; package name is `装备重铸自选属性-v0.1.2-BepInEx5.zip`.
- Build and tests must use `/p:InstallAfterBuild=false`; install only while Ratopia is closed and after backing up the installed DLL.
- The Ratopia skill prohibits subagents; execute and review every task sequentially in the primary agent.

---

### Task 1: Separate view attachment from selection lifetime

**Files:**
- Modify: `tests/EquipmentReforgeSelector.Tests/PanelStateCoordinatorTests.cs`
- Modify: `src/EquipmentReforgeSelector/PanelStateCoordinator.cs`

**Interfaces:**
- Consumes: existing `SelectionSession.Update(int, int, IReadOnlyList<ReforgeCandidate>)`.
- Produces: `PanelStateCoordinator.Detach(object)` that preserves session state and `PanelStateCoordinator.ResetSession()` that explicitly clears it.

- [ ] **Step 1: Write a failing suspend-and-rebind test**

```csharp
[Fact]
public void Detaching_a_view_then_rebinding_the_same_context_preserves_the_exact_selection()
{
    var coordinator = new PanelStateCoordinator();
    var first = new RecordingPanelSink();
    var second = new RecordingPanelSink();
    var candidates = new[] { new ReforgeCandidate(10, 1f), new ReforgeCandidate(11, 2f) };
    coordinator.Attach(first);
    coordinator.Refresh(4, 1, candidates, first);
    Assert.True(coordinator.TrySelect(1));

    Assert.True(coordinator.Detach(first));
    coordinator.Attach(second);
    coordinator.Refresh(4, 1, candidates, second);

    Assert.Equal(new ReforgeCandidate(11, 2f), coordinator.CurrentSelection);
}
```

- [ ] **Step 2: Run the focused test and verify RED**

Run:

```powershell
dotnet test .\tests\EquipmentReforgeSelector.Tests\EquipmentReforgeSelector.Tests.csproj -c Release `
  --filter FullyQualifiedName~PanelStateCoordinatorTests `
  /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false
```

Expected: the new test fails because `Detach` currently calls `ResetSelection` and the rebound view defaults to candidate zero.

- [ ] **Step 3: Implement explicit reset semantics**

Change `Attach` and `Detach` to modify only `CurrentPanel`; add:

```csharp
public void ResetSession()
{
    ResetSelection();
}
```

Keep `Clear()` as `CurrentPanel = null; ResetSelection();`.

- [ ] **Step 4: Replace the old close/reopen test with explicit reset coverage and verify GREEN**

Add assertions that `ResetSession()` clears candidates and selection, and that `Clear()` also removes the current panel. Run the focused command and require all coordinator tests to pass.

- [ ] **Step 5: Commit the state-lifetime change**

```powershell
git add src/EquipmentReforgeSelector/PanelStateCoordinator.cs tests/EquipmentReforgeSelector.Tests/PanelStateCoordinatorTests.cs
git commit -m "fix: preserve reforge choice across view refreshes"
```

---

### Task 2: Suspend tooltip views without clearing the session

**Files:**
- Modify: `tests/EquipmentReforgeSelector.Tests/PluginContractTests.cs`
- Modify: `src/EquipmentReforgeSelector/SimpleToolTipPatch.cs`
- Modify: `src/EquipmentReforgeSelector/RuntimeController.cs`

**Interfaces:**
- Consumes: `PanelStateCoordinator.Detach` and `ResetSession` from Task 1.
- Produces: `RuntimeController.SuspendInlineSelector()` and context-aware view release.

- [ ] **Step 1: Write failing lifecycle source contracts**

Require `SimpleToolTipPatch` to call `SuspendInlineSelector()` for non-`EnhanceEffect` values and forbid `CloseInlineSelector()` there. Require `RuntimeController.RefreshAfterReforge()` to call `PanelState.ResetSession()` before refreshing.

- [ ] **Step 2: Run the focused contract and verify RED**

Run:

```powershell
dotnet test .\tests\EquipmentReforgeSelector.Tests\EquipmentReforgeSelector.Tests.csproj -c Release `
  --filter FullyQualifiedName~PluginContractTests `
  /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false
```

Expected: failure because the production patch still calls `CloseInlineSelector()` and successful reforge does not reset a hidden view's session.

- [ ] **Step 3: Implement the minimal suspend path**

Add `SuspendInlineSelector()` to close the current `InlineReforgeSelectorView` and call `PanelState.Detach(view)` without clearing `_host`, `_item`, `_level`, candidates, or selection. Keep `Clear()` as the only full-context reset.

- [ ] **Step 4: Make view release distinguish tooltip refresh from detail close**

When a view disables while `_host.Obj_Main.activeInHierarchy` is true, detach and preserve the session. When the detail host is absent or inactive, call `Clear()` so reopening the same item starts a new session.

- [ ] **Step 5: Reset stale state after completed reforge and verify GREEN**

`RefreshAfterReforge()` calls `PanelState.ResetSession()` and rerenders only if a current view exists. Run coordinator and plugin contract tests, then the full suite; require all to pass.

- [ ] **Step 6: Commit the runtime lifecycle change**

```powershell
git add src/EquipmentReforgeSelector/SimpleToolTipPatch.cs src/EquipmentReforgeSelector/RuntimeController.cs tests/EquipmentReforgeSelector.Tests/PluginContractTests.cs
git commit -m "fix: suspend selector during vanilla tooltip changes"
```

---

### Task 3: Add full-width rows and direct number shortcuts

**Files:**
- Create: `src/EquipmentReforgeSelector/CandidateShortcut.cs`
- Create: `tests/EquipmentReforgeSelector.Tests/CandidateShortcutTests.cs`
- Modify: `tests/EquipmentReforgeSelector.Tests/PluginContractTests.cs`
- Modify: `src/EquipmentReforgeSelector/InlineReforgeSelectorView.cs`

**Interfaces:**
- Produces: `CandidateShortcut.TryResolveDigit(int digit, int candidateCount, out int candidateIndex)`.
- Consumes: `RuntimeController.SelectCandidate(int)` and original `Batch_ResEffect.Txt_Value` rows.

- [ ] **Step 1: Write failing pure shortcut tests**

```csharp
[Theory]
[InlineData(1, 3, 0)]
[InlineData(2, 3, 1)]
[InlineData(3, 3, 2)]
public void Visible_digit_maps_to_zero_based_candidate(int digit, int count, int expected)
{
    Assert.True(CandidateShortcut.TryResolveDigit(digit, count, out var actual));
    Assert.Equal(expected, actual);
}

[Theory]
[InlineData(0, 3)]
[InlineData(4, 3)]
[InlineData(1, 0)]
public void Out_of_range_digit_is_ignored(int digit, int count)
{
    Assert.False(CandidateShortcut.TryResolveDigit(digit, count, out _));
}
```

- [ ] **Step 2: Run shortcut tests and verify RED**

Expected: compilation fails because `CandidateShortcut` does not exist.

- [ ] **Step 3: Implement the minimal pure mapping and verify GREEN**

```csharp
internal static class CandidateShortcut
{
    public static bool TryResolveDigit(int digit, int candidateCount, out int candidateIndex)
    {
        candidateIndex = digit - 1;
        return digit >= 1 && candidateCount > 0 && candidateIndex < candidateCount;
    }
}
```

- [ ] **Step 4: Write failing full-row source contracts**

Require the view to create an `Image`, `LayoutElement` with `ignoreLayout = true`, horizontally stretched hit-area anchors, `InlineReforgeButton`, and numbered formatting. Require `text.raycastTarget = false`, `KeyCode.Alpha1`, `KeyCode.Keypad1`, and `CandidateShortcut.TryResolveDigit`.

- [ ] **Step 5: Replace text-only buttons with reusable sibling hit areas**

For each text row, create a sibling `RectTransform` named `EquipmentReforgeSelectorHitArea`, copy its vertical anchors/offsets, set horizontal anchors to `0f..1f`, and add a transparent `Image`, ignored `LayoutElement`, and Mod-owned button. Keep it behind the text, disable text raycasts, and configure normal/highlighted/pressed colors. Disable and reuse hit areas during same-frame tooltip refreshes.

- [ ] **Step 6: Add visible and keyboard feedback**

Format rows as `[1] 属性文本`; selected rows retain the vanilla green arrow and add `（已选择）`. In `Update`, map alpha-row and keypad digits to the pure shortcut and call `RuntimeController.SelectCandidate` only when the view is current.

- [ ] **Step 7: Run focused and full tests, then commit**

```powershell
dotnet test .\EquipmentReforgeSelector.sln -c Release `
  /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false
git add src/EquipmentReforgeSelector tests/EquipmentReforgeSelector.Tests
git commit -m "feat: add full-row reforge selection controls"
```

---

### Task 4: Release metadata and package version `0.1.2`

**Files:**
- Modify: `tests/EquipmentReforgeSelector.Tests/PluginContractTests.cs`
- Modify: `tests/EquipmentReforgeSelector.Tests/ReleaseArtifactContractTests.cs`
- Modify: `src/EquipmentReforgeSelector/Plugin.cs`
- Modify: `README.md`
- Modify: `docs/TESTING.md`
- Modify: `scripts/Package.ps1`

**Interfaces:**
- Produces: plugin version `0.1.2` and `dist/装备重铸自选属性-v0.1.2-BepInEx5.zip`.

- [ ] **Step 1: Change release contracts to `0.1.2` and verify RED**

Require the plugin constant, README, testing guide, packaging script and exact archive name to use `0.1.2`. Run the release contract filters and expect failures while production metadata is still `0.1.1`.

- [ ] **Step 2: Update release metadata and user instructions**

Document full-row clicking, number keys, persistent selection across the four effect-cell hovers, reset conditions, installation, rollback and log locations.

- [ ] **Step 3: Run release contracts and verify GREEN**

Require the package staging structure to contain only the plugin DLL, README and testing guide, with no forbidden runtime dependencies.

- [ ] **Step 4: Commit release metadata**

```powershell
git add src/EquipmentReforgeSelector/Plugin.cs README.md docs/TESTING.md scripts/Package.ps1 tests/EquipmentReforgeSelector.Tests
git commit -m "docs: release persistent selector version 0.1.2"
```

---

### Task 5: Verify, package, install, and collect runtime evidence

**Files:**
- Output: `dist/装备重铸自选属性-v0.1.2-BepInEx5.zip`
- Install: `E:\steam\steamapps\common\Ratopia\BepInEx\plugins\EquipmentReforgeSelector\EquipmentReforgeSelector.dll`
- Backup: `backups/<timestamp>/previous-plugin/EquipmentReforgeSelector.dll`

**Interfaces:**
- Consumes: completed `0.1.2` source and contracts.
- Produces: validator-clean package and hash-matched installed DLL.

- [ ] **Step 1: Run fresh Release verification without installation**

```powershell
dotnet test .\EquipmentReforgeSelector.sln -c Release `
  /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false
dotnet build .\src\EquipmentReforgeSelector\EquipmentReforgeSelector.csproj -c Release `
  /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false
git diff --check
```

Expected: all tests pass; build reports zero warnings and zero errors; diff check is empty.

- [ ] **Step 2: Build and validate the exact package**

```powershell
.\scripts\Package.ps1
& 'C:\Users\ASUS\.codex\skills\developing-ratopia-mods\scripts\Test-RatopiaPackage.ps1' `
  -Path '.\dist\装备重铸自选属性-v0.1.2-BepInEx5.zip' `
  -ExpectedPluginName EquipmentReforgeSelector
```

Expected: forbidden files, unexpected files, and errors are all empty.

- [ ] **Step 3: Install only while Ratopia is closed**

Confirm `Get-Process Ratopia -ErrorAction SilentlyContinue` returns nothing. Copy the installed `0.1.1` DLL to a timestamped `previous-plugin` directory, copy only the Release DLL, and require identical SHA-256 values for the Release, ZIP entry and installed file.

- [ ] **Step 4: Verify fresh startup evidence**

Start Ratopia, record PID and log timestamp, and require new log lines for `Loading [装备重铸自选属性 0.1.2]` plus successful installation of `ItemDetailOpenPatch`, `ItemEnhancePatch`, and `SimpleToolTipPatch`. Require no new Mod Error/Exception/disable markers, then close Ratopia.

- [ ] **Step 5: Hand off the exact in-game regression matrix**

Verify in Royal level 1 and HellAnvil level 2: choose the second candidate; hover all four effect/reforge cells; return to the candidate list and confirm the same arrow and “已选择” row; click text, icon and blank row space; select with number keys; perform reforge and confirm exact type/value. Save/reload behavior remains covered by the unchanged vanilla save format and the existing backup baseline.
