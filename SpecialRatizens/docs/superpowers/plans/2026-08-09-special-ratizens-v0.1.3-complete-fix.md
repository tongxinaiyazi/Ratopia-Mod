# Special Ratizens v0.1.3 Complete Fix Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Ratopia work must stay in the primary agent; do not dispatch subagents.

**Goal:** Audit and repair the complete standalone Special Ratizens feature, eliminate Queen candidate-skin leakage and the empty prosperity baseline, restore the two missing Omega-7 traits, harden shared effect/session paths, then package and install BepInEx 5 release `0.1.3` without touching saves.

**Architecture:** Keep the existing Harmony whitelist and legacy formulas, but add explicit preview-vs-live skin rendering, a small pure prosperity-baseline policy, and deterministic session reset boundaries. Validate the 12-ratizen/24-trait runtime matrix with Mono.Cecil and data contracts, then produce an audit report and a hash-verified release.

**Tech Stack:** C# / net472, BepInEx 5.4.23.5, Harmony 2.9.0, Ratopia Mono 1.0.0600, Spine runtime, xUnit 2.9.2, Mono.Cecil, PowerShell packaging.

## Global Constraints

- Cover all 12 shipped special ratizens and all 24 shipped traits, not only the two reported symptoms.
- Candidate preview code may build `Sp_SkinInfo.m_Skin` but must never call `UpdateCombinedSkin()` before `T_Citizen.SkinInit` binds the citizen skeleton.
- Ordinary candidates and citizens must never enter the custom-skin pipeline.
- Prosperity policy values must always be calculated from a deep-copied raw database baseline; repeated calls must not accumulate.
- Do not restore the original full-mod `DB_Mgr.Awake` patch or any unrelated full-mod feature.
- Target BepInEx 5 Mono and net472; add no runtime NuGet dependency.
- Publish version `0.1.3` / file version `0.1.3.0` / archive `特殊鼠鼠-v0.1.3-BepInEx5.zip`.
- Do not edit, deserialize-and-write, or repackage game saves.
- Do not launch Ratopia automatically.
- Do not overwrite the installed DLL while `Ratopia.exe` is running.
- Before installation, back up the installed v0.1.2 plugin and `E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile`, then verify hashes.
- This directory is not a Git repository; record RED/GREEN, build, package, backup, and hash checkpoints instead of commits.

---

### Task 1: Add complete migration audit contracts and prove RED

**Files:**
- Create: `tests/SpecialRatizens.Tests/MigrationAuditContractTests.cs`
- Modify: `tests/SpecialRatizens.Tests/AppearanceContractTests.cs`
- Modify: `tests/SpecialRatizens.Tests/GameContractTests.cs`

**Interfaces:**
- Produces: Cecil helper `GetLegacyMethod(ModuleDefinition,string)` and runtime trait-key extraction from `RatopiaMod.CustomMOD..cctor`.
- Produces: executable contracts for all 24 runtime trait registrations, candidate preview isolation, session reset coverage, and Ratopia DB initialization order.

- [ ] **Step 1: Add the failing runtime trait registry test**

Extract strings used as keys in the static `CustomCharInfo` dictionary and compare them to the shipped CSV:

```csharp
[Fact]
public void RuntimeStateRegistryContainsEveryShippedTraitExactlyOnce()
{
    var shipped = LoadCatalog().Traits.Select(item => item.Name)
        .OrderBy(item => item, StringComparer.Ordinal).ToArray();

    using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
    {
        var cctor = module.Types.Single(type => type.FullName == "RatopiaMod.CustomMOD")
            .Methods.Single(method => method.Name == ".cctor");
        var registered = ExtractCustomCharInfoKeys(cctor)
            .OrderBy(item => item, StringComparer.Ordinal).ToArray();

        Assert.Equal(24, registered.Length);
        Assert.Equal(shipped, registered);
    }
}
```

Expected v0.1.2 failure: registry has 22 entries and is missing `AMJ7_LZDW` and `AMJ7_LZJX`.

- [ ] **Step 2: Add failing Queen-preview contracts**

Add contracts that require `RegisterCustomSkin(Sp_SkinInfo,CustomSpecialUnit,bool)`, a `false` literal in the special candidate branch, and a `true` literal in `AddSpecialCitizen`:

```csharp
[Fact]
public void CandidateBuildsPreviewWithoutApplyingToBoundSkeleton()
{
    using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
    {
        var candidate = GetLegacyMethod(module, "CCMake_Info");
        Assert.True(CallsWithBoolean(candidate, "RegisterCustomSkin", false));

        var apply = GetLegacyMethod(module, "UpdateUnitCustomSkin");
        Assert.Contains(apply.Parameters, parameter =>
            parameter.Name == "applyToSkeleton" && parameter.ParameterType.FullName == "System.Boolean");
    }
}

[Fact]
public void LiveCitizenStillAppliesTheCombinedSkin()
{
    using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
    {
        Assert.True(CallsWithBoolean(GetLegacyMethod(module, "AddSpecialCitizen"),
            "RegisterCustomSkin", true));
    }
}
```

Add a call-graph assertion that the `applyToSkeleton == false` branch reaches `SkinSet` but not `UpdateCombinedSkin`.

- [ ] **Step 3: Add failing session-isolation contracts**

Require `SpecialRatizensSessionLoaded` to reset old runtime state before rebuilding citizens, and require reset to clear the leaked session fields:

```csharp
[Fact]
public void LoadingAnotherWorldResetsRuntimeStateBeforeRebuildingCitizens()
{
    using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
    {
        var loaded = GetLegacyMethod(module, "SpecialRatizensSessionLoaded");
        Assert.True(CallsInOrder(loaded, "ResetSpecialRatizensSession", "LoadCitizenDatas", "UpdateAllUsedSpecialEffects"));

        var reset = GetLegacyMethod(module, "ResetSpecialRatizensSession");
        Assert.True(LoadsFieldThenCalls(reset, "preValueDic", "Clear"));
        Assert.True(LoadsFieldThenCalls(reset, "CountryCommercialityDatas", "Clear"));
        Assert.True(StoresStaticField(reset, "SuperElecLine"));
        Assert.True(StoresStaticField(reset, "AMJ7_PDI"));
    }
}
```

This catches cross-save `isUsed`/probability, commercial progress, cached PDI, and electrical-grid references.

- [ ] **Step 4: Add the Ratopia initialization-order contract**

Use Mono.Cecil against `Assembly-CSharp.dll`:

```csharp
[Fact]
public void ProsperityDatabaseIsBuiltBeforeCharacterDatabasePostfixRuns()
{
    using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
    {
        var awake = FindType(module, "DB_Mgr").Methods.Single(method => method.Name == "Awake");
        var calls = awake.Body.Instructions
            .Select(instruction => instruction.Operand as MethodReference)
            .Where(call => call != null && call.DeclaringType.FullName == "DB_Mgr")
            .Select(call => call.Name).ToArray();

        Assert.True(Array.IndexOf(calls, "Prosperity_DB_Setting") <
                    Array.IndexOf(calls, "Character_DB_Setting"));
    }
}
```

- [ ] **Step 5: Run focused tests and verify RED**

Run:

```powershell
dotnet test .\SpecialRatizens.sln -c Release /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false --filter 'FullyQualifiedName~MigrationAuditContractTests|FullyQualifiedName~AppearanceContractTests|FullyQualifiedName~GameContractTests' --verbosity minimal
```

Expected: failures for 22/24 runtime traits, missing preview/live boolean split, missing session reset ordering/fields, while the original game DB order contract passes.

- [ ] **Step 6: Record RED checkpoint**

Record the exact failed test names and confirm no production source or game file changed in Task 1.

### Task 2: Separate candidate preview data from live Spine rendering

**Files:**
- Modify: `src/SpecialRatizens/Legacy/CustomMOD.cs`
- Modify: `tests/SpecialRatizens.Tests/AppearanceContractTests.cs`

**Interfaces:**
- Produces: `RegisterCustomSkin(Sp_SkinInfo skinInfo, CustomSpecialUnit unit, bool applyToSkeleton)`.
- Produces: `UpdateUnitSpineDress(..., bool applyToSkeleton)` and `UpdateUnitCustomSkin(..., bool applyToSkeleton)`.
- Produces: `RenderCombinedSkin(Sp_SkinInfo skinInfo, bool applyToSkeleton)` where `SkinSet` always runs and `UpdateCombinedSkin` runs only when the flag is true.

- [ ] **Step 1: Preserve failing preview/live tests from Task 1**

Run the focused appearance filter alone and retain the RED output before editing production:

```powershell
dotnet test .\SpecialRatizens.sln -c Release /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false --filter FullyQualifiedName~AppearanceContractTests --verbosity minimal
```

- [ ] **Step 2: Add the explicit apply flag at the two lifecycle entry points**

Change only the two registrations:

```csharp
// CCMake_Info candidate branch: SkinInfo still points at the Queen skeleton.
RegisterCustomSkin(__instance.SkinInfo, specialUnit, false);

// AddSpecialCitizen: T_Citizen.SkinInit has rebound SkinInfo to the citizen.
RegisterCustomSkin(citizen.m_SkinInfo, unit, true);
```

- [ ] **Step 3: Thread the flag through the custom-skin pipeline**

Use these signatures:

```csharp
static void RegisterCustomSkin(Sp_SkinInfo skinInfo, CustomSpecialUnit unit, bool applyToSkeleton)
static bool UpdateUnitSpineDress(Sp_SkinInfo skinInfo, string key, string gender,
    Building job, bool isCitizen, Dictionary<string, string> customSkin, bool applyToSkeleton = true)
static bool UpdateUnitCustomSkin(Sp_SkinInfo skinInfo, string key,
    bool isCitizen = true, bool applyToSkeleton = true)
static void RecoverUnitSkin(Sp_SkinInfo skinInfo, Dictionary<string, string> skinSnapshot,
    Dictionary<string, string> overrideSnapshot, string key, string reason, bool applyToSkeleton)
static void RenderCombinedSkin(Sp_SkinInfo skinInfo, bool applyToSkeleton)
```

Every recovery call must forward the same flag.

- [ ] **Step 4: Make rendering data-only when requested**

Implement:

```csharp
static void RenderCombinedSkin(Sp_SkinInfo skinInfo, bool applyToSkeleton)
{
    skinInfo.SkinSet(skinInfo.m_Skin, skinInfo.m_SkeletonData);
    if (applyToSkeleton)
        skinInfo.UpdateCombinedSkin();
}
```

Do not read, replace, or restore the Queen's skeleton. Candidate preview UI already consumes `SkinInfo.m_Skin` directly.

- [ ] **Step 5: Keep ordinary citizens outside the pipeline**

Retain the ordinary candidate branch as only:

```csharp
__instance.MakeSkinInfo();
MakeCharacterList(__instance);
return false;
```

Extend the ordinary-citizen contract to forbid calls to `RegisterCustomSkin`, `UpdateUnitSpineDress`, `UpdateUnitCustomSkin`, and `UpdateCombinedSkin` in that branch.

- [ ] **Step 6: Run appearance tests and verify GREEN**

Run the Step 1 command. Expected: all appearance contracts pass, including existing full-body recovery and actual-gender checks.

### Task 3: Add safe, idempotent prosperity baseline handling

**Files:**
- Create: `src/SpecialRatizens/Core/ProsperityBaselinePolicy.cs`
- Create: `tests/SpecialRatizens.Tests/ProsperityBaselinePolicyTests.cs`
- Modify: `src/SpecialRatizens/Legacy/CustomMOD.cs`
- Modify: `tests/SpecialRatizens.Tests/MigrationAuditContractTests.cs`

**Interfaces:**
- Produces: `ProsperityBaselinePolicy.Matches(IReadOnlyList<int> liveLevels, IReadOnlyList<int> baselineLevels)`.
- Produces: `ProsperityBaselinePolicy.ApplyBonus(IReadOnlyList<int> baselinePolicyCounts, int bonus)` returning a new `int[]`.
- Produces: `EnsureProsperityBaseline()` and a `ProsperityDBOwner` session/database identity guard in `CustomMOD`.

- [ ] **Step 1: Write pure policy tests**

```csharp
[Fact]
public void RequiresSameNonEmptyLevelSequence()
{
    Assert.True(ProsperityBaselinePolicy.Matches(new[] { 1, 2, 3 }, new[] { 1, 2, 3 }));
    Assert.False(ProsperityBaselinePolicy.Matches(new int[0], new int[0]));
    Assert.False(ProsperityBaselinePolicy.Matches(new[] { 1, 2, 3 }, new[] { 1, 2 }));
    Assert.False(ProsperityBaselinePolicy.Matches(new[] { 1, 3 }, new[] { 1, 2 }));
}

[Fact]
public void BonusAlwaysUsesTheBaselineAndNeverAccumulates()
{
    var baseline = new[] { 2, 3, 4 };
    Assert.Equal(new[] { 7, 8, 9 }, ProsperityBaselinePolicy.ApplyBonus(baseline, 5));
    Assert.Equal(new[] { 7, 8, 9 }, ProsperityBaselinePolicy.ApplyBonus(baseline, 5));
    Assert.Equal(baseline, ProsperityBaselinePolicy.ApplyBonus(baseline, 0));
}
```

- [ ] **Step 2: Run policy tests and verify RED**

```powershell
dotnet test .\SpecialRatizens.sln -c Release /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false --filter FullyQualifiedName~ProsperityBaselinePolicyTests --verbosity minimal
```

Expected: compilation fails because `ProsperityBaselinePolicy` does not exist.

- [ ] **Step 3: Implement the pure policy**

```csharp
internal static class ProsperityBaselinePolicy
{
    internal static bool Matches(IReadOnlyList<int> liveLevels, IReadOnlyList<int> baselineLevels)
    {
        if (liveLevels == null || baselineLevels == null || liveLevels.Count == 0 ||
            liveLevels.Count != baselineLevels.Count)
            return false;

        for (var i = 0; i < liveLevels.Count; i++)
            if (liveLevels[i] != baselineLevels[i]) return false;
        return true;
    }

    internal static int[] ApplyBonus(IReadOnlyList<int> baselinePolicyCounts, int bonus)
    {
        if (baselinePolicyCounts == null) throw new ArgumentNullException(nameof(baselinePolicyCounts));
        var result = new int[baselinePolicyCounts.Count];
        for (var i = 0; i < result.Length; i++) result[i] = baselinePolicyCounts[i] + bonus;
        return result;
    }
}
```

- [ ] **Step 4: Initialize the baseline at the existing narrow DB hook**

At the end of `DB_Mgr_Character_DB_Setting`, call:

```csharp
LoadProsperityDB(__instance);
```

Replace the old loader with a guarded version that deep-copies nonzero levels from `manager.m_Prosperity_DB1.sheets[0].list`, sorts them, records `ProsperityDBOwner = manager`, and resets a one-shot failure-log flag. Update the dormant legacy `DB_Mgr_Awake` body to pass its `__instance` to the new loader as well, so the source remains type-consistent even though that broad patch is not installed. The loader must return `false` rather than throw when the raw table is unavailable.

- [ ] **Step 5: Add defensive lazy validation before Qin Law**

Implement `EnsureProsperityBaseline()` to compare the current `DB_Mgr` identity and level sequences. If invalid, rebuild only from the raw DB table. Log one diagnostic error and return `false` if rebuilding is impossible.

Update `SY_QL_Effect`:

```csharp
static void SY_QL_Effect()
{
    if (!EnsureProsperityBaseline())
        return;

    int bonus = CustomCharInfoIsActive("SY_QL") ? SYQL_Value : 0;
    int[] values = ProsperityBaselinePolicy.ApplyBonus(
        ProsperityDB.Select(info => info.PolicyNum).ToArray(), bonus);

    for (int i = 0; i < DBMgr.List_ProsperityDB.Count; i++)
        DBMgr.List_ProsperityDB[i].PolicyNum = values[i];
}
```

The inactive path intentionally restores the original baseline when switching to a save without Shang Yang.

- [ ] **Step 6: Run prosperity and migration tests and verify GREEN**

Run:

```powershell
dotnet test .\SpecialRatizens.sln -c Release /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false --filter 'FullyQualifiedName~ProsperityBaselinePolicyTests|FullyQualifiedName~MigrationAuditContractTests|FullyQualifiedName~GameContractTests' --verbosity minimal
```

Expected: baseline policy and DB order tests pass; runtime registry/session tests may remain RED until Task 4.

### Task 4: Repair the 24-trait registry, cross-save state, and effect null boundaries

**Files:**
- Modify: `src/SpecialRatizens/Legacy/CustomMOD.cs`
- Modify: `tests/SpecialRatizens.Tests/MigrationAuditContractTests.cs`
- Create: `tests/SpecialRatizens.Tests/EffectSafetyContractTests.cs`

**Interfaces:**
- Produces: exact 24-key `CustomCharInfo` runtime registry.
- Produces: deterministic world-load reset before citizen reconstruction.
- Produces: null-safe Omega-7 and Pikachu electrical effects and trade-price logging.

- [ ] **Step 1: Keep the 22/24 registry test RED, then register Omega-7**

Add exactly:

```csharp
{ "AMJ7_LZDW", new CustomCharInfo(C_Buff.None) },
{ "AMJ7_LZJX", new CustomCharInfo(C_Buff.None) },
```

Run the single runtime-registry test. Expected: 24/24 GREEN and exact equality with the CSV.

- [ ] **Step 2: Reset before rebuilding a loaded world**

Change:

```csharp
public static void SpecialRatizensSessionLoaded()
{
    ResetSpecialRatizensSession();
    LoadCitizenDatas();
    if (ActiveCustomSpecialUnit)
        UpdateAllUsedSpecialEffects();
}
```

Extend `ResetSpecialRatizensSession` with:

```csharp
preValueDic.Clear();
CountryCommercialityDatas.Clear();
SuperElecLine = null;
AMJ7_PDI = 0;
priceIsUpdateBySS = -1;
```

The existing loop resetting every `CustomSpecialUnit.isUsed` and `pdr_C` must remain before `LoadCitizenDatas` reconstructs current-world usage.

- [ ] **Step 3: Write failing effect-safety contracts**

Require these call orders/guards with Mono.Cecil:

```csharp
[Fact]
public void PikachuChecksTheElectricalGridBeforeReadingItsFields()
{
    using (var module = OpenPlugin())
        Assert.True(NullGuardPrecedesFieldRead(GetLegacyMethod(module, "PKQ_SWFT_Effect"),
            "ElecLine_Info", "m_Watt"));
}

[Fact]
public void OmegaFillHandlesMissingGridAndMissingBot()
{
    using (var module = OpenPlugin())
    {
        var method = GetLegacyMethod(module, "FillUpGbotPower");
        Assert.True(ChecksNullBeforeDereference(method, "SuperElecLine"));
        Assert.True(ChecksArgumentNullBeforeDereference(method, 0));
    }
}
```

Add source/IL contracts requiring the manpower-generator postfix to check `building` and `worker`, and export price to check `____info`, before dereferencing them.

- [ ] **Step 4: Run effect-safety tests and verify RED**

```powershell
dotnet test .\SpecialRatizens.sln -c Release /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false --filter FullyQualifiedName~EffectSafetyContractTests --verbosity minimal
```

Expected: failures for the current early `elecLine_Info.m_Watt` read, missing `SuperElecLine` guard, and missing trade/worker guards.

- [ ] **Step 5: Add minimal null guards without changing formulas**

Implement:

```csharp
static bool FillUpGbotPower(GBot bot)
{
    if (bot == null || SuperElecLine == null || SuperElecLine.m_Watt <= 0f)
        return false;
    // existing formula follows unchanged
}
```

In `PKQ_SWFT_Effect`, move overflow calculation after:

```csharp
ElecLine_Info elecLineInfo = BuildingMgr.SearchElecInfo(building.m_ID);
if (elecLineInfo == null)
{
    Debug.LogWarning($"皮卡丘发电失败：建筑 {building.m_CustomName} 未连接电网");
    return;
}
```

In `MasonryInfo_WorkUpdate_Postfix`, return when `building == null || building.m_Master == null`. In export-price handling, return original behavior when `____info == null`, matching the existing import guard.

- [ ] **Step 6: Run migration and effect-safety tests and verify GREEN**

Run both test classes. Expected: 24/24 registry, session reset, and all null-boundary contracts pass.

### Task 5: Complete the 12-ratizen/24-trait audit and full regression

**Files:**
- Create: `docs/audits/2026-08-09-special-ratizens-v0.1.3-audit.md`
- Modify: `tests/SpecialRatizens.Tests/ReleaseDataContractTests.cs`
- Modify: `tests/SpecialRatizens.Tests/PluginContractTests.cs`

**Interfaces:**
- Produces: traceable matrix from each shipped trait to its runtime state registration, effect method, Harmony entry point, trigger, baseline/cleanup rule, and runtime acceptance case.

- [ ] **Step 1: Strengthen shipped-data matrix tests**

Assert all 12 names are unique, all 24 traits are referenced exactly once, each ratizen has `Skin`, `Face`, `Hair`, and `Dress`, every icon exists, and all divisor fields used by formulas are nonzero for the shipped data.

- [ ] **Step 2: Verify the exact special-feature patch whitelist**

Keep the existing 39 patch descriptor names exact. Cross-check the public legacy entry points for generation, 24 traits, combat, appearance, and session load against `LegacyPatchAdapters` and `PatchRegistry`; fail if an expected adapter or descriptor is absent or if a Queen patch is added.

- [ ] **Step 3: Write the audit matrix**

Document all 24 rows:

```markdown
| Ratizen | Trait | Runtime method/hook | Trigger | Baseline/cleanup | Offline result | Runtime check |
|---|---|---|---|---|---|---|
| 商鞅 | SY_QL | SY_QL_Effect / state.pdi | recruit, load, PDI | raw prosperity DB | PASS | policy count + no error |
| 奥米伽-7 | AMJ7_LZDW | AMJ7_LZDW_Effect / power.* | recruit, load, grid changes | SuperElecLine reset | PASS | unified grid |
```

Fill every row with concrete methods and tests; no placeholder rows or copied shorthand are allowed. Add separate sections for Queen/ordinary isolation, session/save safety, log scan, and unresolved runtime-only checks.

- [ ] **Step 4: Run the complete test suite**

```powershell
dotnet test .\SpecialRatizens.sln -c Release /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false --verbosity minimal
```

Expected: all tests pass with zero skipped/failed tests.

- [ ] **Step 5: Scan build output and source**

Confirm no compiler warnings attributable to new code, no dormant Harmony attributes, no `BepInPlugin` entry beyond `SpecialRatizens.Plugin`, no write path into saves, and no new dependency.

### Task 6: Version, documentation, build, and package v0.1.3

**Files:**
- Modify: `tests/SpecialRatizens.Tests/PluginContractTests.cs`
- Modify: `tests/SpecialRatizens.Tests/PackagingContractTests.cs`
- Modify: `src/SpecialRatizens/Plugin.cs`
- Modify: `src/SpecialRatizens/SpecialRatizens.csproj`
- Modify: `scripts/Package.ps1`
- Modify: `README.md`
- Create: `dist/特殊鼠鼠-v0.1.3-BepInEx5.zip`

**Interfaces:**
- Produces: plugin identity `0.1.3`, assembly/file version `0.1.3.0`, and archive `特殊鼠鼠-v0.1.3-BepInEx5.zip`.

- [ ] **Step 1: Change version contracts to v0.1.3 and verify RED**

Update only tests first to expect:

```csharp
Assert.Equal("0.1.3", Plugin.PluginVersion);
Assert.Contains("<Version>0.1.3</Version>", project);
Assert.Contains("<AssemblyVersion>0.1.3.0</AssemblyVersion>", project);
Assert.Contains("特殊鼠鼠-v0.1.3-BepInEx5.zip", package);
```

Run version/packaging tests. Expected: RED on v0.1.2 source.

- [ ] **Step 2: Bump production and documentation version**

Change `Plugin.PluginVersion`, three csproj version values, package archive name, README title, load message example, change log, fixes, backup/install warnings, and runtime acceptance notes to v0.1.3.

- [ ] **Step 3: Run full tests again**

Run the Task 5 full-suite command. Expected: all tests GREEN.

- [ ] **Step 4: Package using the project script**

```powershell
.\scripts\Package.ps1 -RatopiaDir 'E:\steam\steamapps\common\Ratopia'
```

Expected: tests and Release build pass and the v0.1.3 ZIP is created.

- [ ] **Step 5: Verify package contents and identity**

Confirm ZIP entries are only `BepInEx/plugins/SpecialRatizens/SpecialRatizens.dll`, `Data/**`, and `README.md`. Inspect DLL file version `0.1.3.0` and record SHA-256 for built DLL, staged DLL, and ZIP; built and staged DLL hashes must match.

### Task 7: Back up and install only while Ratopia is stopped

**Files:**
- Read/backup: `E:\steam\steamapps\common\Ratopia\BepInEx\plugins\SpecialRatizens`
- Read/backup: `E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile`
- Install: `E:\steam\steamapps\common\Ratopia\BepInEx\plugins\SpecialRatizens`
- Create: `backups/pre-install-v0.1.3-<timestamp>/**`

**Interfaces:**
- Consumes: verified v0.1.3 staged package from Task 6.
- Produces: recoverable v0.1.2 plugin/save backup and an installed directory byte-identical to the staged v0.1.3 package.

- [ ] **Step 1: Require the game process to be absent**

```powershell
$ratopia = Get-Process -Name Ratopia -ErrorAction SilentlyContinue
if ($ratopia) { throw 'Ratopia 正在运行，禁止覆盖模组 DLL。' }
```

- [ ] **Step 2: Resolve and validate exact targets**

Resolve the absolute plugin, save, project-backup, and staged-package paths. Reject any plugin target not under `E:\steam\steamapps\common\Ratopia\BepInEx\plugins` and any backup target not under this project’s `backups` directory.

- [ ] **Step 3: Back up plugin and saves**

Create `backups/pre-install-v0.1.3-<timestamp>/Plugin` and `/SaveFile`; copy with native PowerShell `Copy-Item -LiteralPath -Recurse`. Generate a SHA-256 manifest for both backups and compare file counts/hashes to their sources before installation.

- [ ] **Step 4: Install staged package**

Copy the staged `BepInEx\plugins\SpecialRatizens` contents into the exact installed plugin directory. Do not delete or touch any sibling mod directory and do not launch Ratopia.

- [ ] **Step 5: Verify installed bytes**

Recursively compare relative paths, file lengths, and SHA-256 between staged and installed plugin directories. Confirm the installed DLL reports version `0.1.3.0` and its hash equals the packaged DLL.

- [ ] **Step 6: Deliver runtime acceptance checklist**

Ask the user to test: multiple special candidates without Queen changes, a normal candidate, recruit/reload/replace jobs, Shang Yang policy count and error absence, Omega-7 grid/robot effects, Pikachu with and without a connected grid, switching between two saves, and a fresh `Player.log`/`LogOutput.log` scan. Make clear that no save was modified by installation and the v0.1.2 backup remains available.

## Execution Checkpoints

1. Task 1 RED evidence captured before production edits.
2. Tasks 2-4 focused suites GREEN after each minimal fix.
3. Task 5 complete audit matrix and full suite GREEN.
4. Task 6 release build, ZIP layout, version, and hashes verified.
5. Task 7 game stopped, backups verified, installed bytes match staged package.
