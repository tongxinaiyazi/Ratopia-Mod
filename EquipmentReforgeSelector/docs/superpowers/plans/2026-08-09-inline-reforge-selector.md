# Inline Reforge Selector Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the obscured right-side selector with clickable candidate rows embedded in Ratopia's original beige `EnhanceEffect` panel.

**Architecture:** `ItemDetail_Open` keeps the current equipment context. A new Postfix on `SimpleToolTip.SimpleToolTipSet` binds a short-lived Unity adapter to `Batch_ResEffect.Txt_Value`; the adapter renders the existing pure selection state into original text rows and adds temporary `Button` components. The existing `T_Queen.ItemEnhance` scoped candidate override remains unchanged.

**Tech Stack:** C# `net472`, BepInEx 5.4.23.5, Harmony 2.9.0.0, Unity 2021.3 UI/TMPro, xUnit, Mono.Cecil, PowerShell 5-compatible packaging.

## Global Constraints

- Game path is `E:\steam\steamapps\common\Ratopia`; target is Mono/BepInEx 5 and `net472`.
- Compatibility baseline remains `Assembly-CSharp.dll` SHA-256 `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`.
- Only `SimpleToolTipList.EnhanceEffect` may be modified; every other tooltip must remain vanilla.
- Reuse `Batch_ResEffect.Txt_Value`; do not create a new Canvas, side panel, or fixed screen-space anchor.
- Preserve original candidate rules, values, material cost, achievement behavior, and `T_Queen.Dic_ItemPlusEffect` save format.
- Game, Unity, BepInEx, Harmony, Mono.Cecil, and test assemblies must not be copied into the package.
- Bugfix version is `0.1.1`; package name is `装备重铸自选属性-v0.1.1-BepInEx5.zip`.
- Build and tests must use `/p:InstallAfterBuild=false`; install only while Ratopia is closed and after backing up the installed DLL.

---

### Task 1: Pure inline row presentation

**Files:**
- Create: `src/EquipmentReforgeSelector/InlineCandidateRow.cs`
- Create: `src/EquipmentReforgeSelector/InlineCandidatePlan.cs`
- Create: `tests/EquipmentReforgeSelector.Tests/InlineCandidatePlanTests.cs`

**Interfaces:**
- Consumes: `ReforgeCandidate` and exact candidate equality.
- Produces: `InlineCandidatePlan.Create(IReadOnlyList<ReforgeCandidate>, ReforgeCandidate?)` and rows exposing `CandidateIndex`, `Candidate`, and `IsSelected`.

- [ ] **Step 1: Write the failing duplicate-ability presentation test**

```csharp
[Fact]
public void Exact_duplicate_ability_value_is_the_only_selected_inline_row()
{
    var candidates = new[] { new ReforgeCandidate(11, 2f), new ReforgeCandidate(11, 3f) };

    var plan = InlineCandidatePlan.Create(candidates, new ReforgeCandidate(11, 3f));

    Assert.False(plan.Rows[0].IsSelected);
    Assert.True(plan.Rows[1].IsSelected);
    Assert.Equal(1, plan.Rows[1].CandidateIndex);
}
```

- [ ] **Step 2: Run the focused test and verify RED**

Run:

```powershell
dotnet test .\tests\EquipmentReforgeSelector.Tests\EquipmentReforgeSelector.Tests.csproj -c Release `
  --filter FullyQualifiedName~InlineCandidatePlanTests `
  /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false
```

Expected: compilation fails because `InlineCandidatePlan` does not exist.

- [ ] **Step 3: Implement the minimal immutable row plan**

```csharp
public readonly struct InlineCandidateRow
{
    public InlineCandidateRow(int candidateIndex, ReforgeCandidate candidate, bool isSelected)
    {
        CandidateIndex = candidateIndex;
        Candidate = candidate;
        IsSelected = isSelected;
    }

    public int CandidateIndex { get; }
    public ReforgeCandidate Candidate { get; }
    public bool IsSelected { get; }
}

public sealed class InlineCandidatePlan
{
    private InlineCandidatePlan(IReadOnlyList<InlineCandidateRow> rows) => Rows = rows;
    public IReadOnlyList<InlineCandidateRow> Rows { get; }

    public static InlineCandidatePlan Create(
        IReadOnlyList<ReforgeCandidate> candidates,
        ReforgeCandidate? selected)
    {
        if (candidates == null) throw new ArgumentNullException(nameof(candidates));
        var rows = new InlineCandidateRow[candidates.Count];
        for (var index = 0; index < candidates.Count; index++)
        {
            rows[index] = new InlineCandidateRow(
                index,
                candidates[index],
                selected.HasValue && candidates[index] == selected.Value);
        }
        return new InlineCandidatePlan(rows);
    }
}
```

- [ ] **Step 4: Add null, empty, unselected, and exact-selection tests; verify GREEN**

Expected: focused tests pass and preserve duplicate ability values by full candidate equality.

- [ ] **Step 5: Commit the pure presentation task**

```powershell
git add src/EquipmentReforgeSelector/InlineCandidateRow.cs `
  src/EquipmentReforgeSelector/InlineCandidatePlan.cs `
  tests/EquipmentReforgeSelector.Tests/InlineCandidatePlanTests.cs
git commit -m "feat: add inline reforge candidate rows"
```

---

### Task 2: Bind the selector to Ratopia's beige effect rows

**Files:**
- Create: `src/EquipmentReforgeSelector/InlineReforgeSelectorView.cs`
- Create: `src/EquipmentReforgeSelector/SimpleToolTipPatch.cs`
- Delete: `src/EquipmentReforgeSelector/ReforgeSelectorPanel.cs`
- Modify: `src/EquipmentReforgeSelector/RuntimeController.cs`
- Modify: `src/EquipmentReforgeSelector/Plugin.cs`
- Modify: `tests/EquipmentReforgeSelector.Tests/PluginContractTests.cs`
- Modify: `tests/EquipmentReforgeSelector.Tests/PanelStateCoordinatorTests.cs`
- Modify: `tests/EquipmentReforgeSelector.Tests/RatopiaAssemblyContractTests.cs`

**Interfaces:**
- Consumes: `InlineCandidatePlan`, `CandidateNavigationPlan`, `PanelStateCoordinator`, `Batch_ResEffect.Txt_Value`.
- Produces: `RuntimeController.OpenInlineSelector(SimpleToolTip, Batch_ResEffect, int, int)` and a temporary `InlineReforgeSelectorView : MonoBehaviour, IPanelStateSink`.

- [ ] **Step 1: Write failing production-structure and assembly contracts**

Replace the old runtime UI contract with assertions using the existing `ReadRequiredProductionFile` helper:

```csharp
var tooltipPatch = ReadRequiredProductionFile("SimpleToolTipPatch.cs");
var inlineView = ReadRequiredProductionFile("InlineReforgeSelectorView.cs");
Assert.Contains("SimpleToolTipList.EnhanceEffect", tooltipPatch);
Assert.Contains("___m_EffectFrame", tooltipPatch);
Assert.Contains("frame.Txt_Value", inlineView);
Assert.Contains("Navigation.Mode.Explicit", inlineView);
Assert.DoesNotContain("SetSelectedGameObject", inlineView);
Assert.DoesNotContain(".Select()", inlineView);

var productionText = string.Join(
    "\n",
    Directory.GetFiles(
        Path.Combine(ContractTestPaths.RepositoryRoot, "src", "EquipmentReforgeSelector"),
        "*.cs")
        .Select(File.ReadAllText));
Assert.DoesNotContain("EquipmentReforgeSelectorPanel", productionText);
Assert.DoesNotContain("anchorMin = new Vector2(1f, 0.5f)", productionText);
```

Extend `Harmony_targets_and_runtime_fields_have_exact_signatures` with the existing `AssertMethod` and `AssertField` helpers, plus an enum constant check:

```csharp
AssertMethod(
    assembly,
    "SimpleToolTip",
    "SimpleToolTipSet",
    "System.Void",
    "SimpleToolTip/SimpleToolTipList",
    "System.Single",
    "System.Single",
    "System.Single");
AssertField(assembly, "SimpleToolTip", "m_EffectFrame", "Batch_ResEffect");
AssertField(assembly, "Batch_ResEffect", "Txt_Value", "TMPro.TextMeshProUGUI[]");

var tooltipEnum = RequireType(assembly, "SimpleToolTip")
    .NestedTypes.Single(type => type.Name == "SimpleToolTipList");
var enhanceEffect = tooltipEnum.Fields.Single(field => field.Name == "EnhanceEffect");
Assert.Equal(92, Convert.ToInt32(enhanceEffect.Constant));
```

- [ ] **Step 2: Run focused contract tests and verify RED**

Run:

```powershell
dotnet test .\tests\EquipmentReforgeSelector.Tests\EquipmentReforgeSelector.Tests.csproj -c Release `
  --filter "FullyQualifiedName~PluginContractTests|FullyQualifiedName~RatopiaAssemblyContractTests" `
  /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false
```

Expected: production-structure tests fail because the inline view and tooltip patch do not exist and the fixed right-side panel still exists. Assembly-only assertions pass.

- [ ] **Step 3: Add the exact tooltip Postfix and register it atomically**

```csharp
[HarmonyPatch(
    typeof(SimpleToolTip),
    "SimpleToolTipSet",
    new[] { typeof(SimpleToolTip.SimpleToolTipList), typeof(float), typeof(float), typeof(float) })]
internal static class SimpleToolTipPatch
{
    private static void Postfix(
        SimpleToolTip __instance,
        SimpleToolTip.SimpleToolTipList _value,
        float _a_value,
        float _b_value,
        Batch_ResEffect ___m_EffectFrame)
    {
        if (_value != SimpleToolTip.SimpleToolTipList.EnhanceEffect) return;
        RuntimeController.OpenInlineSelector(
            __instance,
            ___m_EffectFrame,
            (int)_a_value,
            (int)_b_value);
    }
}
```

Register `SimpleToolTipPatch` alongside the two existing patch types so any installation failure still unpatches the entire Mod.

- [ ] **Step 4: Implement the temporary original-row adapter**

`InlineReforgeSelectorView` must:

```csharp
public void Render(IReadOnlyList<ReforgeCandidate> candidates, ReforgeCandidate? selected)
{
    if (candidates.Count > _frame.Txt_Value.Length)
        throw new InvalidOperationException("原版效果行容量不足");

    DeactivateButtons();
    var plan = InlineCandidatePlan.Create(candidates, selected);
    for (var index = 0; index < _frame.Txt_Value.Length; index++)
    {
        var text = _frame.Txt_Value[index];
        var active = index < plan.Rows.Count;
        text.gameObject.SetActive(active);
        if (!active) continue;

        var row = plan.Rows[index];
        text.text = FormatCandidate(row.Candidate, row.IsSelected);
        var button = text.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = text;
        var candidateIndex = row.CandidateIndex;
        button.onClick.AddListener(() => RuntimeController.SelectCandidate(candidateIndex));
        _buttons.Add(button);
    }
    ApplyExplicitNavigation();
}
```

Formatting must copy the vanilla selected row style:

```csharp
return selected
    ? "<sprite name=FS_P_Right_White>" + Helpers.SetColor(Defines.Hex_DeepGreen, value)
    : value;
```

Use a Mod-owned `InlineReforgeButton : Button` component. On disable/destroy/clear, set every owned button `interactable = false` and `enabled = false`, remove only owned listeners, and retain the disabled component for safe reuse after Ratopia's same-frame effect-frame refresh. Never destroy or reparent original text objects, and close the binding for every non-`EnhanceEffect` tooltip.

- [ ] **Step 5: Refactor the controller from side-panel creation to delayed inline binding**

Required flow:

```csharp
public static void Open(BuildMidUI host, ItemInfo item, int level)
{
    // Validate and store context only. Release a previous view when item/level changes.
}

public static void OpenInlineSelector(
    SimpleToolTip tooltip,
    Batch_ResEffect frame,
    int itemType,
    int level)
{
    // Require current item, matching item type/level, active EnhanceEffect frame,
    // attach InlineReforgeSelectorView, then resolve and render candidates.
}
```

`RefreshAfterReforge` rerenders only when a current inline view exists. `WarnVanillaFallback` writes the warning into that view. Tooltip close detaches the view and resets the selection session without clearing the stored item context, so reopening starts with a fresh default selection.

- [ ] **Step 6: Run focused tests, fix only evidence-backed failures, and verify GREEN**

Expected: inline row tests, coordinator lifecycle tests, plugin contracts, and Cecil contracts all pass; no source token for the old side panel remains.

- [ ] **Step 7: Commit the runtime replacement**

```powershell
git add src/EquipmentReforgeSelector tests/EquipmentReforgeSelector.Tests
git commit -m "fix: embed reforge choices in vanilla effect panel"
```

---

### Task 3: Release version, package, and documentation

**Files:**
- Modify: `src/EquipmentReforgeSelector/Plugin.cs`
- Modify: `README.md`
- Modify: `docs/TESTING.md`
- Modify: `scripts/Package.ps1`
- Modify: `tests/EquipmentReforgeSelector.Tests/ReleaseArtifactContractTests.cs`
- Modify: `tests/EquipmentReforgeSelector.Tests/PluginContractTests.cs`

**Interfaces:**
- Consumes: completed inline runtime.
- Produces: version `0.1.1` and `dist/装备重铸自选属性-v0.1.1-BepInEx5.zip` with the same three-file whitelist.

- [ ] **Step 1: Change release tests to require `0.1.1` and verify RED**

Assertions must require:

```text
EquipmentReforgeSelectorPlugin.PluginVersion == "0.1.1"
装备重铸自选属性-v0.1.1-BepInEx5.zip
```

Expected: focused release tests fail while production files still contain `0.1.0`.

- [ ] **Step 2: Update the plugin, package script, README, and manual matrix**

Document that choices are clicked directly in the original beige effect list, the selected row uses the vanilla green arrow, and other tooltips are untouched.

- [ ] **Step 3: Run release contracts and verify GREEN**

Expected: the package script creates the exact `0.1.1` ZIP containing only README, testing documentation, and the plugin DLL under `BepInEx/plugins/EquipmentReforgeSelector/`.

- [ ] **Step 4: Commit the bugfix release metadata**

```powershell
git add README.md docs/TESTING.md scripts/Package.ps1 `
  src/EquipmentReforgeSelector/Plugin.cs tests/EquipmentReforgeSelector.Tests
git commit -m "docs: release inline selector version 0.1.1"
```

---

### Task 4: Final verification, safe installation, and runtime evidence

**Files:**
- Output: `dist/装备重铸自选属性-v0.1.1-BepInEx5.zip`
- Install: `E:\steam\steamapps\common\Ratopia\BepInEx\plugins\EquipmentReforgeSelector\EquipmentReforgeSelector.dll`
- Backup: `backups/<timestamp>/previous-plugin/EquipmentReforgeSelector.dll`

**Interfaces:**
- Consumes: Release build and package.
- Produces: installed, hash-matched `0.1.1` DLL and documented verification evidence.

- [ ] **Step 1: Run the complete Release suite without installation**

```powershell
dotnet test .\EquipmentReforgeSelector.sln -c Release `
  /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false
dotnet build .\src\EquipmentReforgeSelector\EquipmentReforgeSelector.csproj -c Release `
  /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false
```

Expected: all tests pass; build has zero warnings and zero errors.

- [ ] **Step 2: Build and validate the exact package**

```powershell
.\scripts\Package.ps1
& 'C:\Users\ASUS\.codex\skills\developing-ratopia-mods\scripts\Test-RatopiaPackage.ps1' `
  -Path '.\dist\装备重铸自选属性-v0.1.1-BepInEx5.zip' `
  -ExpectedPluginName EquipmentReforgeSelector
```

Expected: no forbidden files, unexpected files, or errors.

- [ ] **Step 3: Back up and install only while Ratopia is closed**

Confirm no `Ratopia` process, copy the currently installed DLL into a timestamped `previous-plugin` directory, copy only the new DLL, then require matching SHA-256 for the Release, ZIP-entry, and installed DLL.

- [ ] **Step 4: Verify runtime gates**

Launch the game and require fresh log evidence for BepInEx discovery, all three Harmony patches, the first `ItemDetail_Open` invocation, and the `EnhanceEffect` inline binding. Confirm no Mod Error/Exception/disable markers.

- [ ] **Step 5: Verify the reported visual regression**

In Royal level 1 and HellAnvil level 2, confirm the beige effect list itself is clickable, the selected candidate shows the vanilla green arrow, keyboard navigation/submit works after focus, no black side panel appears, and unrelated tooltips remain vanilla. Perform a reforge and confirm the result matches the exact selected `(AbilityId, Value)`.

- [ ] **Step 6: Final repository audit**

Run `git diff --check`, require a clean worktree, retain the standalone branch `feature/equipment-reforge-selector`, and leave Ratopia closed after verification.
