# Special Ratizens Complete Skin Repair Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Ratopia work must stay in the primary agent; do not dispatch subagents.

**Goal:** Release and install `SpecialRatizens` 0.1.2 so every special ratizen uses a template matching its actual runtime gender and a failed rebuild can never leave it with an empty appearance.

**Architecture:** Keep the migrated legacy patch surface stable, add a small pure `SkinRepairPolicy` for required-category and recovery decisions, and make `CustomMOD` perform transactional Spine updates around that policy. Existing special citizens are repaired through `AddSpecialCitizen` on load; ordinary citizens remain outside the custom skin pipeline.

**Tech Stack:** C# 7+/net472, BepInEx 5.4.23.5, Harmony 2.9.0, Ratopia Mono 1.0.0600, Spine runtime, xUnit 2.9.2, Mono.Cecil, PowerShell packaging.

## Global Constraints

- Preserve the actual `T_Citizen.m_Gender`; never overwrite it from CSV during repair.
- Only special ratizens may enter the repair pipeline; ordinary citizens remain on Ratopia's original appearance path.
- Required non-empty skin categories are exactly `Skin`, `Face`, `Hair`, and `Dress`.
- A failed special-skin rebuild restores a complete snapshot; an incomplete snapshot falls back to Ratopia's default citizen skin for the actual gender.
- Target BepInEx 5 Mono and net472; do not introduce new runtime NuGet dependencies.
- Publish version `0.1.2` / file version `0.1.2.0` / archive `特殊鼠鼠-v0.1.2-BepInEx5.zip`.
- Do not directly edit, deserialize-and-write, or repackage game saves.
- Do not launch Ratopia automatically.
- Before installation, require `Ratopia.exe` to be stopped and create verified backups of the installed plugin and `E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile`.
- This directory is not a Git repository; record test/build/hash checkpoints instead of issuing impossible commits.

---

### Task 1: Add pure recovery policy with red-green tests

**Files:**
- Create: `src/SpecialRatizens/Core/SkinRepairPolicy.cs`
- Create: `tests/SpecialRatizens.Tests/SkinRepairPolicyTests.cs`

**Interfaces:**
- Produces: `SkinRepairPolicy.HasRequiredAppearance(IDictionary<string,string>)`, `SkinRepairPolicy.MissingRequiredCategories(IDictionary<string,string>)`, and `SkinRepairPolicy.SelectRecovery(IDictionary<string,string>)`.
- Produces: `SkinRecoveryKind.Snapshot` and `SkinRecoveryKind.Default`.

- [ ] **Step 1: Write failing policy tests**

Create tests covering a complete dictionary, every required category missing or empty, optional categories empty, and recovery selection:

```csharp
[Fact]
public void RequiresNonEmptyBodyFaceHairAndDress()
{
    var complete = Complete();
    Assert.True(SkinRepairPolicy.HasRequiredAppearance(complete));

    foreach (var category in new[] { "Skin", "Face", "Hair", "Dress" })
    {
        var missing = Complete();
        missing[category] = "";
        Assert.False(SkinRepairPolicy.HasRequiredAppearance(missing));
        Assert.Contains(category, SkinRepairPolicy.MissingRequiredCategories(missing));
    }
}

[Fact]
public void SelectsSnapshotOnlyWhenSnapshotIsComplete()
{
    Assert.Equal(SkinRecoveryKind.Snapshot, SkinRepairPolicy.SelectRecovery(Complete()));
    Assert.Equal(SkinRecoveryKind.Default, SkinRepairPolicy.SelectRecovery(new Dictionary<string, string>()));
}
```

- [ ] **Step 2: Run the focused tests and verify RED**

Run:

```powershell
dotnet test .\SpecialRatizens.sln -c Release /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false --filter FullyQualifiedName~SkinRepairPolicyTests --verbosity minimal
```

Expected: compilation fails because `SkinRepairPolicy` and `SkinRecoveryKind` do not exist.

- [ ] **Step 3: Implement the minimal policy**

Create:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpecialRatizens.Core
{
    internal enum SkinRecoveryKind { Snapshot, Default }

    internal static class SkinRepairPolicy
    {
        internal static readonly string[] RequiredCategories = { "Skin", "Face", "Hair", "Dress" };

        internal static bool HasRequiredAppearance(IDictionary<string, string> skins)
        {
            return skins != null && RequiredCategories.All(category =>
                skins.TryGetValue(category, out var value) && !string.IsNullOrWhiteSpace(value));
        }

        internal static string[] MissingRequiredCategories(IDictionary<string, string> skins)
        {
            return RequiredCategories.Where(category =>
                skins == null || !skins.TryGetValue(category, out var value) || string.IsNullOrWhiteSpace(value)).ToArray();
        }

        internal static SkinRecoveryKind SelectRecovery(IDictionary<string, string> snapshot)
        {
            return HasRequiredAppearance(snapshot) ? SkinRecoveryKind.Snapshot : SkinRecoveryKind.Default;
        }
    }
}
```

- [ ] **Step 4: Run focused tests and verify GREEN**

Run the Step 2 command. Expected: all `SkinRepairPolicyTests` pass.

- [ ] **Step 5: Record checkpoint**

Record the focused test count and output in the execution notes; no commit is possible because the directory has no `.git` repository.

### Task 2: Make gender selection and skin replacement safe

**Files:**
- Modify: `tests/SpecialRatizens.Tests/AppearanceContractTests.cs`
- Modify: `tests/SpecialRatizens.Tests/SpecialDataCatalogTests.cs`
- Modify: `src/SpecialRatizens/Legacy/CustomSpecialUnit.cs`
- Modify: `src/SpecialRatizens/Legacy/CustomMOD.cs`

**Interfaces:**
- Consumes: `SkinRepairPolicy` and `SkinRecoveryKind` from Task 1.
- Produces: runtime invariant that special template gender equals `Sp_SkinInfo.m_Gender`, and skin-info gender equals the real citizen gender at registration.
- Produces: `UpdateUnitCustomSkin(Sp_SkinInfo,string,bool)` returning `bool` to report whether the special appearance applied without recovery.

- [ ] **Step 1: Add failing assembly-contract tests**

Add Mono.Cecil tests that assert:

```csharp
[Fact]
public void SpecialTemplateUsesTheSkinObjectsRuntimeGender()
{
    using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
    {
        var method = GetLegacyMethod(module, "RegisterCustomSkin");
        Assert.Contains(method.Body.Instructions, instruction =>
            instruction.OpCode.Code == Code.Ldfld &&
            instruction.Operand is FieldReference field &&
            field.DeclaringType.FullName == "Sp_SkinInfo" && field.Name == "m_Gender");
        Assert.DoesNotContain(method.Body.Instructions, instruction =>
            instruction.Operand is MethodReference call && call.Name == "get_UnitGender");
    }
}

[Fact]
public void SpecialCitizenSynchronizesOnlyTheSkinGenderBeforeRegistration()
{
    using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
    {
        var method = GetLegacyMethod(module, "AddSpecialCitizen");
        Assert.True(CopiesCitizenGenderToSkinInfoBeforeRegistering(method));
        Assert.False(StoresCitizenGender(method));
    }
}
```

Update the existing appearance call-order assertion so it allows validation/snapshot helper calls but still requires the destructive/render sequence `ClearSkins`, `AssembleData`, `SkinSet`, `UpdateCombinedSkin` in that order.

- [ ] **Step 2: Add failing CSV normalization test**

Add:

```csharp
[Theory]
[InlineData(" Male ", "Male")]
[InlineData(" Female ", "Female")]
public void TrimsValidGenderValues(string source, string expected)
{
    using (var fixture = CatalogFixture.CreateValid(source))
    {
        var catalog = SpecialDataCatalog.Load(fixture.UnitsPath, fixture.TraitsPath, fixture.IconDirectory);
        Assert.Equal(expected, catalog.Ratizens[0].Gender);
    }
}
```

- [ ] **Step 3: Run focused tests and verify RED**

Run:

```powershell
dotnet test .\SpecialRatizens.sln -c Release /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false --filter 'FullyQualifiedName~AppearanceContractTests|FullyQualifiedName~SpecialDataCatalogTests' --verbosity minimal
```

Expected: the runtime-gender and citizen/skin synchronization contracts fail on v0.1.1.

- [ ] **Step 4: Normalize legacy CSV enum input**

Change `CustomSpecialUnit.UnitGender` to:

```csharp
public string UnitGender
{
    get { return gender.ToString(); }
    set { gender = BaseCommand.StringToEnum<Gender>((value ?? "").Trim()); }
}
```

The modern catalog already stores the trimmed `gender` local; ensure the `SpecialRatizenDefinition` constructor receives that trimmed variable.

- [ ] **Step 5: Synchronize the skin object without changing citizen gameplay gender**

Immediately before `RegisterCustomSkin(citizen.m_SkinInfo, unit)` in `AddSpecialCitizen`, add:

```csharp
citizen.m_SkinInfo.m_Gender = citizen.m_Gender;
```

Do not assign to `citizen.m_Gender` anywhere in this repair.

- [ ] **Step 6: Register the special template for the skin object's runtime gender**

Change `RegisterCustomSkin` to derive:

```csharp
string gender = skinInfo.m_Gender.ToString();
UpdateUnitSpineDress(skinInfo, key, gender, null, true, SpecialCitizenSkins[key]);
```

Remove the use of `unit.UnitGender` from this method.

- [ ] **Step 7: Implement transactional apply and recovery**

Refactor `UpdateUnitCustomSkin` to take snapshots, validate the gender template before clearing, validate the result, and recover:

```csharp
static bool UpdateUnitCustomSkin(Sp_SkinInfo skinInfo, string key, bool isCitizen = true)
{
    var skins = new Dictionary<string, string>(skinInfo.SkinDic);
    var overrides = new Dictionary<string, string>(skinInfo.OverrideSkinDic);
    var gender = skinInfo.m_Gender.ToString();

    if (!TryGetSpineDresserElement(key, out var element) ||
        !element.TryGetPairs(gender, out var pairs) || pairs == null)
    {
        RecoverUnitSkin(skinInfo, skins, overrides, key, $"缺少 {gender} 模板");
        return false;
    }

    if (isCitizen)
        skinInfo.ClearSkins();
    SpineDresserMgr.Instance.AssembleData(key, skinInfo, false);

    if (isCitizen && !SkinRepairPolicy.HasRequiredAppearance(skinInfo.SkinDic))
    {
        RecoverUnitSkin(skinInfo, skins, overrides, key,
            $"缺少 {string.Join(",", SkinRepairPolicy.MissingRequiredCategories(skinInfo.SkinDic))}");
        return false;
    }

    RenderCombinedSkin(skinInfo);
    return true;
}
```

Add focused helpers:

```csharp
static void RecoverUnitSkin(Sp_SkinInfo skinInfo, Dictionary<string, string> skins,
    Dictionary<string, string> overrides, string key, string reason)
{
    var recovery = SkinRepairPolicy.SelectRecovery(skins);
    skinInfo.ClearSkins();
    skinInfo.ClearOverrideSkin();

    if (recovery == SkinRecoveryKind.Snapshot)
    {
        skinInfo.SetStyles(skins, null);
        foreach (var pair in overrides)
            skinInfo.SetStyleOverride(pair.Key, pair.Value);
    }
    else
    {
        SpineDresserMgr.Instance.AssembleDefaultSkin(skinInfo);
        SpineDresserMgr.Instance.AssembleData("Jobless_1_1", skinInfo, true);
    }

    RenderCombinedSkin(skinInfo);
    Debug.LogError($"特殊皮肤 {key} 组合失败：{reason}；已使用 {(recovery == SkinRecoveryKind.Snapshot ? "原外观" : "原版默认外观")} 恢复");
}

static void RenderCombinedSkin(Sp_SkinInfo skinInfo)
{
    skinInfo.SkinSet(skinInfo.m_Skin, skinInfo.m_SkeletonData);
    skinInfo.UpdateCombinedSkin();
}
```

After default recovery, log an additional error if `SkinRepairPolicy.HasRequiredAppearance(skinInfo.SkinDic)` is still false.

- [ ] **Step 8: Re-register on special clothes updates**

Change `UpdateClothes` so a special `T_Citizen` synchronizes `m_SkinInfo.m_Gender`, resolves its `CustomSpecialUnit`, and calls `UpdateUnitSpineDress` with `citizen.m_Gender.ToString()`, `citizen.m_Job`, and its existing `SpecialCitizenSkins` dictionary. Return `false` for ordinary citizens or any unresolved special definition so original Ratopia clothes logic remains the fallback.

- [ ] **Step 9: Run focused tests and verify GREEN**

Run the Step 3 command. Expected: all appearance and catalog tests pass.

- [ ] **Step 10: Record checkpoint**

Record the focused test count and confirm Mono.Cecil finds no store to `GameUnit.m_Gender` in `AddSpecialCitizen`.

### Task 3: Bump release identity and document automatic repair

**Files:**
- Modify: `tests/SpecialRatizens.Tests/PluginContractTests.cs`
- Modify: `tests/SpecialRatizens.Tests/PackagingContractTests.cs`
- Modify: `src/SpecialRatizens/Plugin.cs`
- Modify: `src/SpecialRatizens/SpecialRatizens.csproj`
- Modify: `scripts/Package.ps1`
- Modify: `README.md`

**Interfaces:**
- Produces: plugin version `0.1.2`, assembly/file version `0.1.2.0`, and package name `特殊鼠鼠-v0.1.2-BepInEx5.zip`.

- [ ] **Step 1: Change release tests to expect v0.1.2**

Replace every current-release expectation in `PluginContractTests` and `PackagingContractTests` with `0.1.2`, `0.1.2.0`, and `特殊鼠鼠-v0.1.2-BepInEx5.zip`.

- [ ] **Step 2: Run version tests and verify RED**

Run:

```powershell
dotnet test .\SpecialRatizens.sln -c Release /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false --filter 'FullyQualifiedName~PluginContractTests|FullyQualifiedName~PackagingContractTests' --verbosity minimal
```

Expected: tests fail because production metadata still says 0.1.1.

- [ ] **Step 3: Update all release metadata**

Set `Plugin.PluginVersion` and the three MSBuild version properties to 0.1.2/0.1.2.0. Update the archive path in `Package.ps1`.

- [ ] **Step 4: Update README release and acceptance text**

Change the heading to `# 特殊鼠鼠 v0.1.2`, add a `v0.1.2 完整皮肤修复` section describing actual-gender templates, load-time repair, and recovery safeguards, and update the expected load log to v0.1.2.

- [ ] **Step 5: Run version tests and verify GREEN**

Run the Step 2 command. Expected: all plugin and packaging contract tests pass.

### Task 4: Full verification and package construction

**Files:**
- Create: `dist/特殊鼠鼠-v0.1.2-BepInEx5.zip`
- Update generated staging tree: `dist/package/`

**Interfaces:**
- Consumes: source and tests from Tasks 1-3.
- Produces: tested DLL/package plus SHA-256 evidence; does not install or launch Ratopia.

- [ ] **Step 1: Run the complete Release test suite**

```powershell
dotnet test .\SpecialRatizens.sln -c Release /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false --verbosity minimal
```

Expected: every test passes with zero failures.

- [ ] **Step 2: Run the package script**

```powershell
.\scripts\Package.ps1 -RatopiaDir 'E:\steam\steamapps\common\Ratopia'
```

Expected: package script repeats the full tests, builds Release, creates `特殊鼠鼠-v0.1.2-BepInEx5.zip`, and prints SHA-256.

- [ ] **Step 3: Validate archive structure and DLL identity**

Use 7-Zip test/list plus `FileVersionInfo` and SHA-256. Confirm the archive contains only `README.md` and `BepInEx/plugins/SpecialRatizens/{SpecialRatizens.dll,Data/**}` and the packaged DLL is byte-identical to the Release build.

- [ ] **Step 4: Re-run diagnostic contracts against current game assembly**

Record current `Assembly-CSharp.dll` SHA-256 and run `GameContractTests` explicitly. Expected game hash remains `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D` and all contracts pass.

### Task 5: Verified backup and installation

**Files:**
- Back up: `E:/steam/steamapps/common/Ratopia/BepInEx/plugins/SpecialRatizens/`
- Back up: `E:/steam/steamapps/common/Ratopia/Ratopia_Data/SaveFile/`
- Install: `E:/steam/steamapps/common/Ratopia/BepInEx/plugins/SpecialRatizens/`

**Interfaces:**
- Consumes: verified v0.1.2 staging directory from Task 4.
- Produces: installed byte-identical v0.1.2 plugin and timestamped rollback backup; Ratopia remains stopped.

- [ ] **Step 1: Enforce process and path gates**

Resolve all absolute paths and abort if `Get-Process Ratopia` returns a process, if the source package is missing, or if plugin/save targets differ from the exact paths in Global Constraints.

- [ ] **Step 2: Create timestamped backups**

Copy the current plugin directory and the entire actual `SaveFile` tree to `backups/pre-install-v0.1.2-<timestamp>/Plugin` and `/SaveFile`.

- [ ] **Step 3: Verify save backup before installation**

Compare source and backup relative-path sets, file counts, lengths, and SHA-256 for every file. Abort installation on any difference.

- [ ] **Step 4: Install from the verified staging directory**

Copy `dist/package/BepInEx/plugins/SpecialRatizens/**` into the exact installed plugin directory. Preserve other plugins and all configuration files outside this plugin folder.

- [ ] **Step 5: Verify installed bytes and final process state**

Compare every staged relative file to the installed file by SHA-256; confirm installed DLL file version is `0.1.2.0`; confirm `Ratopia.exe` is still not running.

- [ ] **Step 6: Hand off runtime acceptance**

Tell the user to load W02 without immediately saving, check 大正/皮卡丘/林, trigger a clothes change, return to title, and load again. If anything is wrong, exit without saving and provide current `Player.log` and `BepInEx/LogOutput.log`.
