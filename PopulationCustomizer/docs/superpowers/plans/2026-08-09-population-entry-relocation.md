# Population Entry Relocation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Ratopia work must remain in the primary agent; do not dispatch subagents.

**Goal:** Move the “人口自定义” entry from the clipped HUD population container to a native-style button immediately left of the search button in the Statistics Citizen List screen.

**Architecture:** Patch `CasselGames.UI.StatisticsCitizenListUI.Initialize()` and hand the initialized list UI to `PopulationUiController`. `PopulationSettingsPanel` clones the original filter button inside the title layout, strips cloned listeners/icon graphics, adds the “上限” label, and keeps the existing root-canvas settings modal and save behavior unchanged.

**Tech Stack:** BepInEx 5.4.23.5, Harmony 2.9.0, Unity UI/TMP, Mono/.NET Framework 4.7.2, xUnit, Mono.Cecil, PowerShell packaging.

## Global Constraints

- Target `Assembly-CSharp.dll` SHA-256 remains `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`.
- Target plugin version is `0.1.1`; ZIP name is `人口自定义-v0.1.1-BepInEx5.zip`.
- Preserve `cn.ratopia.populationcustomizer.settings` and the `v1|...` per-save format.
- Never overwrite the installed DLL while Ratopia is running.
- Do not use subagents for planning, implementation, review, testing, packaging, installation, or runtime validation.
- The workspace is not a Git repository; record RED/GREEN evidence in SDD reports and do not create commits.

---

### Task 1: Lock the Statistics Citizen List contract and release identity

**Files:**
- Modify: `tests/PopulationCustomizer.Tests/GameContractTests.cs`
- Modify: `tests/PopulationCustomizer.Tests/PluginContractTests.cs`
- Modify: `tests/PopulationCustomizer.Tests/DocumentationContractTests.cs`
- Modify: `tests/PopulationCustomizer.Tests/PackagingContractTests.cs`

**Interfaces:**
- Consumes: `CasselGames.UI.StatisticsCitizenListUI.Initialize(): void`, private fields `_filterBtn` and `_searchBtn` of type `UnityEngine.UI.Button`.
- Produces: failing contracts for `StatisticsCitizenListUiPatch`, `Plugin.AttachStatisticsCitizenListUi(StatisticsCitizenListUI)`, removal of `CitizenUiPatch`, and version `0.1.1`.

- [ ] **Step 1: Add failing game and plugin contracts**

Add exact assertions equivalent to:

```csharp
AssertMethod(module, "CasselGames.UI.StatisticsCitizenListUI", "Initialize", "System.Void");
AssertField(module, "CasselGames.UI.StatisticsCitizenListUI", "_filterBtn", "UnityEngine.UI.Button");
AssertField(module, "CasselGames.UI.StatisticsCitizenListUI", "_searchBtn", "UnityEngine.UI.Button");
```

Change the required lifecycle patch type to `PopulationCustomizer.Patches.StatisticsCitizenListUiPatch`, assert its Postfix calls `Plugin.AttachStatisticsCitizenListUi`, and assert no compiled `CitizenUiPatch` remains. Update metadata/document/package expectations from `0.1.0` to `0.1.1`.

- [ ] **Step 2: Run targeted contracts and verify RED**

Run:

```powershell
$env:RATOPIA_DIR='E:\steam\steamapps\common\Ratopia'
dotnet test .\PopulationCustomizer.sln -c Release /p:InstallAfterBuild=false --filter "FullyQualifiedName~GameContractTests|FullyQualifiedName~PluginContractTests|FullyQualifiedName~DocumentationContractTests|FullyQualifiedName~PackagingContractTests"
```

Expected: game type/field assertions pass, while plugin lifecycle and `0.1.1` expectations fail because production code and release files still use the old HUD entry and `0.1.0`.

### Task 2: Implement the native Statistics screen entry

**Files:**
- Modify: `src/PopulationCustomizer/Patches/LifecyclePatches.cs`
- Modify: `src/PopulationCustomizer/Plugin.cs`
- Modify: `src/PopulationCustomizer/Runtime/PopulationUiController.cs`
- Modify: `src/PopulationCustomizer/Runtime/PopulationSettingsPanel.cs`
- Test: `tests/PopulationCustomizer.Tests/PluginContractTests.cs`

**Interfaces:**
- Consumes: `StatisticsCitizenListUI`, `_filterBtn`, `_searchBtn`, `ApplySettingsDelegate`, `RestoreSettingsDelegate`.
- Produces: `Plugin.AttachStatisticsCitizenListUi(StatisticsCitizenListUI)`, `PopulationUiController.Attach(StatisticsCitizenListUI)`, and `PopulationSettingsPanel.TryCreate(StatisticsCitizenListUI, ...)`.

- [ ] **Step 1: Replace the lifecycle patch**

Implement the minimal Postfix:

```csharp
[HarmonyPatch(typeof(StatisticsCitizenListUI), "Initialize")]
internal static class StatisticsCitizenListUiPatch
{
    private static void Postfix(StatisticsCitizenListUI __instance)
    {
        Plugin.AttachStatisticsCitizenListUi(__instance);
    }
}
```

Remove the `CitizenUI.Awake()` patch and change the plugin/controller entry points to accept `StatisticsCitizenListUI`.

- [ ] **Step 2: Clone and sanitize the native button**

In `PopulationSettingsPanel.TryCreate`, resolve `_filterBtn` and `_searchBtn` through cached `AccessTools.Field` references. Return `null` if either button, their common parent, or a TMP font is unavailable.

Clone `filterButton.gameObject` under the same parent, name it `PopulationCustomizer.Entry`, call `button.onClick.RemoveAllListeners()`, disable descendant `Image` components except the clone root, disable existing descendant TMP labels, create a stretched TMP label with text `上限`, add the Mod click listener, and set the clone sibling index to the current search-button index. Force a layout rebuild on the parent `RectTransform`.

- [ ] **Step 3: Keep the modal and cleanup behavior unchanged**

Keep the overlay as a root `ScreenSpaceOverlay` Canvas. `Dispose()` must destroy only the cloned entry and Mod overlay, restore the Action Map, and never alter `_filterBtn` or `_searchBtn`.

- [ ] **Step 4: Run runtime and game contract tests and verify GREEN**

Run the Task 1 command filtered to `GameContractTests|PluginContractTests`. Expected: all selected runtime and game contracts pass. Documentation and packaging contracts remain RED until Task 3 updates their production artifacts.

- [ ] **Step 5: Run the full Release suite**

Run:

```powershell
dotnet test .\PopulationCustomizer.sln -c Release /p:InstallAfterBuild=false
```

Expected at this intermediate checkpoint: runtime, core, and game contracts pass; only the intentionally deferred documentation and packaging version contracts may remain RED. The final full-suite GREEN gate is Task 3 Step 2.

### Task 3: Update documentation, package, install, and validate

**Files:**
- Modify: `src/PopulationCustomizer/PopulationCustomizer.csproj`
- Modify: `README.md`
- Modify: `docs/TESTING.md`
- Modify: `scripts/Package.ps1`
- Create: `.superpowers/sdd/population-entry-relocation-report.md`

**Interfaces:**
- Consumes: tested `PopulationCustomizer.dll` version `0.1.1.0`.
- Produces: `dist/人口自定义-v0.1.1-BepInEx5.zip` and the installed `PopulationCustomizer.dll`.

- [ ] **Step 1: Update version and usage documentation**

Set project/plugin/file versions to `0.1.1`. Replace instructions that mention a HUD-side button with: click the original population count, then click “上限” immediately left of the search button in the citizen list header. Update the manual checklist to test one-button-only behavior across repeated screen opens.

- [ ] **Step 2: Update and run packaging**

Change the ZIP filename and package contracts to `人口自定义-v0.1.1-BepInEx5.zip`, then run:

```powershell
.\scripts\Package.ps1 -RatopiaDir 'E:\steam\steamapps\common\Ratopia'
& 'C:\Users\ASUS\.codex\skills\developing-ratopia-mods\scripts\Test-RatopiaPackage.ps1' -Path '.\dist\人口自定义-v0.1.1-BepInEx5.zip' -ExpectedPluginName 'PopulationCustomizer'
```

Expected: Release tests/build pass; forbidden, unexpected, and error collections are empty.

- [ ] **Step 3: Install only after Ratopia exits**

Check the exact Ratopia process. If it is running, stop and ask the user to save and exit. After it exits, back up the installed DLL to `backups/pre-update-<timestamp>/PopulationCustomizer.dll`, copy the new DLL, and verify source/installed SHA-256 and assembly version `0.1.1.0` match.

- [ ] **Step 4: Runtime acceptance**

Start Ratopia, enter a save, open the population Statistics screen, and verify the “上限” button is left of search, appears once after repeated opens, opens/closes the existing panel, preserves input focus, and produces no PopulationCustomizer errors. Verify applying a harmless test value updates effective limits; only perform save/reload testing when the user permits writing the active save.

- [ ] **Step 5: Record evidence and final verification**

Record RED/GREEN results, test count, package SHA-256, DLL SHA-256, backup path, installation path, runtime logs, and any manual-only acceptance items in `.superpowers/sdd/population-entry-relocation-report.md`. Re-run the full Release suite and confirm Ratopia is no longer running before reporting completion.
