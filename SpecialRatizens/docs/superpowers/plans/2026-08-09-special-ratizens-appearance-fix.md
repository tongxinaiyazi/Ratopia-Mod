# Special Ratizens Appearance Fix Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. The `developing-ratopia-mods` skill forbids subagent execution for Ratopia work, so every checkbox must be completed sequentially by the primary agent. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Release and install `SpecialRatizens` 0.1.1 so special ratizens apply their combined Spine skin correctly while ordinary citizens remain entirely on Ratopia's original appearance path.

**Architecture:** Preserve the migrated legacy feature boundary and make three focused corrections inside `CustomMOD`: finish the original Spine refresh sequence, remove ordinary-citizen custom-skin calls from active runtime paths, and clear all appearance session state during reset. Lock those behaviors with Mono.Cecil assembly-contract tests before changing production code.

**Tech Stack:** C# net472, BepInEx 5.4.23.5, Harmony 2.9.0, Spine runtime bundled with Ratopia, xUnit 2.9.2, Mono.Cecil, PowerShell packaging and installation checks.

## Global Constraints

- Target Ratopia Mono build `1.0.0600` and `Assembly-CSharp.dll` SHA-256 `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`.
- Use BepInEx `5.4.23.5`, Harmony `2.9.0`, and `net472`; do not copy game, Unity, BepInEx, Harmony, or third-party DLLs into the package.
- Keep all special-ratizen generation, probability, traits, effects, data, and icons unchanged.
- Ordinary citizen appearance must be handled only by Ratopia.
- Build and test with `/p:InstallAfterBuild=false`; install only after Ratopia is closed.
- Do not automatically launch Ratopia, load a save, or save the game.
- Back up the installed Mod and the real game save directory `E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile` before overwriting the installed plugin.
- The project is not a Git repository; use failing/passing tests, hashes, and backup paths as execution checkpoints instead of commits.

---

### Task 1: Lock and fix the Spine combined-skin refresh

**Files:**
- Create: `tests/SpecialRatizens.Tests/AppearanceContractTests.cs`
- Modify: `src/SpecialRatizens/Legacy/CustomMOD.cs:10238-10248`

**Interfaces:**
- Consumes: private `RatopiaMod.CustomMOD.UpdateUnitCustomSkin(Sp_SkinInfo, string, bool)` and the current game's `Sp_SkinInfo.UpdateCombinedSkin()`.
- Produces: the exact refresh sequence `ClearSkins → AssembleData → SkinSet → UpdateCombinedSkin`.

- [ ] **Step 1: Add the failing combined-skin contract test**

Create `AppearanceContractTests.cs` with:

```csharp
using System.Linq;
using Mono.Cecil;
using SpecialRatizens.Core;
using Xunit;

namespace SpecialRatizens.Tests
{
    public sealed class AppearanceContractTests
    {
        [Fact]
        public void SpecialRatizenAppearanceInstallsTheCombinedSkinOnTheSkeleton()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
            {
                var method = GetLegacyMethod(module, "UpdateUnitCustomSkin");
                var relevantCalls = method.Body.Instructions
                    .Select(instruction => instruction.Operand as MethodReference)
                    .Where(reference => reference != null)
                    .Select(reference => reference.Name)
                    .Where(name => name == "ClearSkins" ||
                                   name == "AssembleData" ||
                                   name == "SkinSet" ||
                                   name == "SetSlotsToSetupPose" ||
                                   name == "UpdateCombinedSkin")
                    .ToArray();

                Assert.Equal(
                    new[] { "ClearSkins", "AssembleData", "SkinSet", "UpdateCombinedSkin" },
                    relevantCalls);
            }
        }

        private static MethodDefinition GetLegacyMethod(ModuleDefinition module, string name)
        {
            return module.Types
                .Single(type => type.FullName == "RatopiaMod.CustomMOD")
                .Methods
                .Single(method => method.Name == name);
        }
    }
}
```

- [ ] **Step 2: Run the new test and verify RED**

Run:

```powershell
dotnet test .\tests\SpecialRatizens.Tests\SpecialRatizens.Tests.csproj -c Release `
  /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' `
  /p:InstallAfterBuild=false `
  --filter 'FullyQualifiedName~AppearanceContractTests.SpecialRatizenAppearanceInstallsTheCombinedSkinOnTheSkeleton' `
  --verbosity normal
```

Expected: one failure showing actual sequence ends in `SetSlotsToSetupPose` instead of `UpdateCombinedSkin`.

- [ ] **Step 3: Make the minimal production change**

Replace `UpdateUnitCustomSkin` with:

```csharp
static void UpdateUnitCustomSkin(Sp_SkinInfo skinInfo, string key, bool isCitizen = true)
{
    if (isCitizen)
        skinInfo.ClearSkins();

    SpineDresserMgr.Instance.AssembleData(key, skinInfo, false);
    skinInfo.SkinSet(skinInfo.m_Skin, skinInfo.m_SkeletonData);
    skinInfo.UpdateCombinedSkin();
}
```

- [ ] **Step 4: Run the focused test and verify GREEN**

Run the Step 2 command again.

Expected: one passed test, zero failed tests, and no warning or error output.

---

### Task 2: Remove ordinary-citizen skin behavior and clear session state

**Files:**
- Modify: `tests/SpecialRatizens.Tests/AppearanceContractTests.cs`
- Modify: `src/SpecialRatizens/Legacy/CustomMOD.cs:265-287`
- Modify: `src/SpecialRatizens/Legacy/CustomMOD.cs:1774-1812`
- Modify: `src/SpecialRatizens/Legacy/CustomMOD.cs:9955-9984`

**Interfaces:**
- Consumes: `LoadCitizenDatas`, `UpdateClothes`, `ResetSpecialRatizensSession`, and the existing special-ratizen lookup `CitizenIsSpecialUnit`.
- Produces: ordinary citizens never call `TryGetCitizenCustomSkin` or `UpdateUnitSpineDress`; reset clears all appearance dictionaries and editor references.

- [ ] **Step 1: Add failing isolation and reset tests**

Add `using Mono.Cecil.Cil;` and these tests/helpers inside `AppearanceContractTests`:

```csharp
[Fact]
public void OrdinaryCitizensNeverEnterTheCustomSkinPipeline()
{
    using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
    {
        var loadCalls = CalledMethodNames(GetLegacyMethod(module, "LoadCitizenDatas"));
        Assert.DoesNotContain("TryGetCitizenCustomSkin", loadCalls);
        Assert.DoesNotContain("UpdateUnitSpineDress", loadCalls);

        var clothes = GetLegacyMethod(module, "UpdateClothes");
        Assert.DoesNotContain("UpdateUnitSpineDress", CalledMethodNames(clothes));
        Assert.DoesNotContain(clothes.Body.Instructions, instruction =>
            instruction.Operand is FieldReference field && field.Name == "CitizenCustomSkins");
    }
}

[Fact]
public void SessionResetClearsAllAppearanceRuntimeState()
{
    using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
    {
        var reset = GetLegacyMethod(module, "ResetSpecialRatizensSession");

        Assert.True(LoadsFieldThenCalls(reset, "CitizenCustomSkins", "Clear"));
        Assert.True(LoadsFieldThenCalls(reset, "EditingCustomSkinIndex", "Clear"));
        Assert.True(StoresStaticField(reset, "OpenedCitizenInfo"));
        Assert.True(StoresStaticField(reset, "OpenedSpcialCitizen"));
        Assert.True(StoresStaticField(reset, "EditingCustomSkins"));
    }
}

private static string[] CalledMethodNames(MethodDefinition method)
{
    return method.Body.Instructions
        .Select(instruction => instruction.Operand as MethodReference)
        .Where(reference => reference != null)
        .Select(reference => reference.Name)
        .ToArray();
}

private static bool LoadsFieldThenCalls(MethodDefinition method, string fieldName, string callName)
{
    var instructions = method.Body.Instructions;
    for (var index = 0; index < instructions.Count - 1; index++)
    {
        if (instructions[index].OpCode.Code == Code.Ldsfld &&
            instructions[index].Operand is FieldReference field &&
            field.Name == fieldName &&
            instructions[index + 1].Operand is MethodReference call &&
            call.Name == callName)
        {
            return true;
        }
    }

    return false;
}

private static bool StoresStaticField(MethodDefinition method, string fieldName)
{
    return method.Body.Instructions.Any(instruction =>
        instruction.OpCode.Code == Code.Stsfld &&
        instruction.Operand is FieldReference field &&
        field.Name == fieldName);
}
```

- [ ] **Step 2: Run the two new tests and verify RED**

Run:

```powershell
dotnet test .\tests\SpecialRatizens.Tests\SpecialRatizens.Tests.csproj -c Release `
  /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' `
  /p:InstallAfterBuild=false `
  --filter 'FullyQualifiedName~AppearanceContractTests&FullyQualifiedName!~SpecialRatizenAppearanceInstalls' `
  --verbosity normal
```

Expected: both tests fail because `LoadCitizenDatas` and `UpdateClothes` still reference ordinary skins and reset does not clear those fields.

- [ ] **Step 3: Remove the ordinary-citizen branch from `LoadCitizenDatas`**

Replace the non-special branch with:

```csharp
if (!TryGetSpecialUnit(citizen, out CustomSpecialUnit unit) || !AddSpecialCitizen(unit, citizen))
{
    Debug.LogWarning($"加载市民 {citizen.m_UnitName}");
    continue;
}
```

- [ ] **Step 4: Restrict `UpdateClothes` to special ratizens**

Replace `UpdateClothes` with:

```csharp
static bool UpdateClothes(GameUnit unit)
{
    if (!CitizenIsSpecialUnit(unit.m_UnitName, unit.m_ID, out _))
        return false;

    UpdateUnitCustomSkin(unit.m_SkinInfo, unit.m_UnitName, unit is T_Citizen);
    Debug.Log($"特殊单位 {unit.m_UnitName} 更新了服装");
    return true;
}
```

- [ ] **Step 5: Expand `ResetSpecialRatizensSession`**

After clearing `SpecialCitizenSkins`, add:

```csharp
CitizenCustomSkins.Clear();
OpenedCitizenInfo = null;
OpenedSpcialCitizen = false;
EditingCustomSkins = null;
EditingCustomSkinIndex.Clear();
```

Keep the existing special-unit and custom-trait cleanup below those statements.

- [ ] **Step 6: Run all appearance tests and verify GREEN**

Run:

```powershell
dotnet test .\tests\SpecialRatizens.Tests\SpecialRatizens.Tests.csproj -c Release `
  /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' `
  /p:InstallAfterBuild=false `
  --filter 'FullyQualifiedName~AppearanceContractTests' `
  --verbosity normal
```

Expected: three passed tests and zero failures.

---

### Task 3: Bump release identity and document the fix

**Files:**
- Modify: `tests/SpecialRatizens.Tests/PluginContractTests.cs:14-20`
- Modify: `tests/SpecialRatizens.Tests/PackagingContractTests.cs`
- Modify: `src/SpecialRatizens/Plugin.cs:16-18`
- Modify: `src/SpecialRatizens/SpecialRatizens.csproj:7-9`
- Modify: `scripts/Package.ps1:24`
- Modify: `README.md`

**Interfaces:**
- Consumes: `Plugin.PluginVersion`, MSBuild version properties, package filename, and README release text.
- Produces: consistent public version `0.1.1` / assembly version `0.1.1.0` and archive `特殊鼠鼠-v0.1.1-BepInEx5.zip`.

- [ ] **Step 1: Change version expectations first**

In `PluginContractTests.PluginIdentityIsStable`, change the expected version to:

```csharp
Assert.Equal("0.1.1", Plugin.PluginVersion);
```

Add this test to `PackagingContractTests`:

```csharp
[Fact]
public void ReleaseVersionIsConsistentAcrossProjectDocumentationAndPackageName()
{
    var root = GetProjectRoot();
    var project = File.ReadAllText(Path.Combine(root, "src", "SpecialRatizens", "SpecialRatizens.csproj"));
    var readme = File.ReadAllText(Path.Combine(root, "README.md"));
    var package = File.ReadAllText(Path.Combine(root, "scripts", "Package.ps1"));

    Assert.Contains("<Version>0.1.1</Version>", project);
    Assert.Contains("<AssemblyVersion>0.1.1.0</AssemblyVersion>", project);
    Assert.Contains("<FileVersion>0.1.1.0</FileVersion>", project);
    Assert.Contains("# 特殊鼠鼠 v0.1.1", readme);
    Assert.Contains("特殊鼠鼠 v0.1.1 已加载", readme);
    Assert.Contains("特殊鼠鼠-v0.1.1-BepInEx5.zip", package);
}
```

- [ ] **Step 2: Run version tests and verify RED**

Run:

```powershell
dotnet test .\tests\SpecialRatizens.Tests\SpecialRatizens.Tests.csproj -c Release `
  /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' `
  /p:InstallAfterBuild=false `
  --filter 'PluginIdentityIsStable|ReleaseVersionIsConsistentAcrossProjectDocumentationAndPackageName' `
  --verbosity normal
```

Expected: failures referencing current version `0.1.0`.

- [ ] **Step 3: Update production and release version strings**

Apply these exact values:

```csharp
// Plugin.cs
public const string PluginVersion = "0.1.1";
```

```xml
<Version>0.1.1</Version>
<AssemblyVersion>0.1.1.0</AssemblyVersion>
<FileVersion>0.1.1.0</FileVersion>
```

```powershell
$archivePath = Join-Path $distRoot '特殊鼠鼠-v0.1.1-BepInEx5.zip'
```

Update README heading and log expectation to `v0.1.1`. Replace the obsolete sentence saying the task did not install the Mod with:

```markdown
打包脚本不会自动安装或启动游戏；人工安装必须在鼠托邦完全退出后进行。
```

Add this release note after the introductory paragraph:

```markdown
## v0.1.1 外观修复

- 特殊鼠鼠换装后会完成 Spine 组合皮肤刷新，修复身体或服装附件缺失。
- 普通市民不再进入迁移版自定义皮肤流程，读档和换装完全交还原版。
```

- [ ] **Step 4: Run version tests and verify GREEN**

Run the Step 2 command again.

Expected: two passed tests and zero failures.

---

### Task 4: Run the complete offline verification and rebuild the package

**Files:**
- Regenerate: `dist/package/BepInEx/plugins/SpecialRatizens/**`
- Create: `dist/特殊鼠鼠-v0.1.1-BepInEx5.zip`

**Interfaces:**
- Consumes: all source, tests, Data files, README, and the target Ratopia assemblies.
- Produces: a validated root-installable BepInEx 5 package without changing the game directory.

- [ ] **Step 1: Run the complete Release suite**

```powershell
dotnet test .\SpecialRatizens.sln -c Release `
  /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' `
  /p:InstallAfterBuild=false `
  --verbosity normal
```

Expected: 32 tests passed, zero failed, zero skipped, zero warnings, and zero errors.

- [ ] **Step 2: Perform a clean Release rebuild**

```powershell
dotnet build .\src\SpecialRatizens\SpecialRatizens.csproj -c Release -t:Rebuild `
  /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' `
  /p:InstallAfterBuild=false `
  --no-restore --verbosity normal
```

Expected: build succeeded with zero warnings and zero errors.

- [ ] **Step 3: Rebuild the release archive**

```powershell
& '.\scripts\Package.ps1' -RatopiaDir 'E:\steam\steamapps\common\Ratopia'
```

Expected: package path ends in `特殊鼠鼠-v0.1.1-BepInEx5.zip`; the script repeats all 32 tests and reports a SHA-256.

- [ ] **Step 4: Validate package policy and ZIP integrity**

```powershell
& 'C:\Users\ASUS\.codex\skills\developing-ratopia-mods\scripts\Test-RatopiaPackage.ps1' `
  -Path '.\dist\特殊鼠鼠-v0.1.1-BepInEx5.zip' `
  -ExpectedPluginName 'SpecialRatizens' | Format-List *

& 'C:\Program Files\7-Zip\7z.exe' t '.\dist\特殊鼠鼠-v0.1.1-BepInEx5.zip'
```

Expected: one plugin DLL, empty forbidden/unexpected/error lists, and `Everything is Ok` for 28 archive files.

- [ ] **Step 5: Compare built and staged artifacts**

Compare SHA-256 for:

```text
src/SpecialRatizens/bin/Release/net472/SpecialRatizens.dll
dist/package/BepInEx/plugins/SpecialRatizens/SpecialRatizens.dll
```

Recursively compare all 26 Data files. Expected: DLL hashes match, Data hash differences are zero, and 24 PNG icons are present.

---

### Task 5: Back up and install 0.1.1 without launching Ratopia

**Files:**
- Back up: `E:\steam\steamapps\common\Ratopia\BepInEx\plugins\SpecialRatizens`
- Back up: `E:\steam\steamapps\common\Ratopia\Ratopia_Data\SaveFile`
- Update: `E:\steam\steamapps\common\Ratopia\BepInEx\plugins\SpecialRatizens\**`

**Interfaces:**
- Consumes: the verified `dist/package/BepInEx/plugins/SpecialRatizens` directory.
- Produces: installed `SpecialRatizens.dll` version `0.1.1.0`, byte-identical to the packaged DLL, with a timestamped rollback backup.

- [ ] **Step 1: Establish the installation gate**

Verify `Get-Process Ratopia` returns no process. Enumerate `BepInEx/plugins` and `BepInEx/patchers`, record SHA-256 for every currently installed DLL, confirm only one `cn.ratopia.specialratizens` GUID, and confirm the staged plugin contains 27 files.

Expected: Ratopia closed, no duplicate GUID, no patcher conflict, and the installed 0.1.0 directory exists.

- [ ] **Step 2: Back up the installed plugin and real saves**

Create a timestamped directory under:

```text
D:\SOFTWARE\项目\鼠托邦mod\SpecialRatizens\backups\pre-appearance-fix-install-$stamp\
```

Copy the installed `SpecialRatizens` directory to `SpecialRatizens-0.1.0` and the entire real `Ratopia_Data\SaveFile` directory to `SaveFile`. Recursively compare source and backup hashes before continuing.

Use:

```powershell
$projectRoot = 'D:\SOFTWARE\项目\鼠托邦mod\SpecialRatizens'
$gameRoot = 'E:\steam\steamapps\common\Ratopia'
$installedRoot = Join-Path $gameRoot 'BepInEx\plugins\SpecialRatizens'
$saveRoot = Join-Path $gameRoot 'Ratopia_Data\SaveFile'
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$backupRoot = Join-Path $projectRoot "backups\pre-appearance-fix-install-$stamp"
$pluginBackup = Join-Path $backupRoot 'SpecialRatizens-0.1.0'
$saveBackup = Join-Path $backupRoot 'SaveFile'

New-Item -ItemType Directory -Path $backupRoot | Out-Null
Copy-Item -LiteralPath $installedRoot -Destination $pluginBackup -Recurse
Copy-Item -LiteralPath $saveRoot -Destination $saveBackup -Recurse

function Get-TreeHashes([string]$Root) {
    $map = @{}
    foreach ($file in Get-ChildItem -LiteralPath $Root -Recurse -Force -File) {
        $relative = $file.FullName.Substring($Root.Length).TrimStart('\')
        $map[$relative] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    }
    return $map
}

$installedHashes = Get-TreeHashes $installedRoot
$pluginBackupHashes = Get-TreeHashes $pluginBackup
$saveHashes = Get-TreeHashes $saveRoot
$saveBackupHashes = Get-TreeHashes $saveBackup

$pluginDiff = @($installedHashes.Keys | Where-Object {
    -not $pluginBackupHashes.ContainsKey($_) -or $pluginBackupHashes[$_] -ne $installedHashes[$_]
})
$saveDiff = @($saveHashes.Keys | Where-Object {
    -not $saveBackupHashes.ContainsKey($_) -or $saveBackupHashes[$_] -ne $saveHashes[$_]
})

if ($installedHashes.Count -ne $pluginBackupHashes.Count -or $pluginDiff.Count -ne 0) {
    throw 'Installed plugin backup verification failed.'
}
if ($saveHashes.Count -ne $saveBackupHashes.Count -or $saveDiff.Count -ne 0) {
    throw 'Save backup verification failed.'
}
```

Expected: plugin and save backup file counts match their sources and both hash-difference counts are zero.

- [ ] **Step 3: Copy only this Mod's staged files**

With Ratopia checked again immediately before the write, recursively copy each child of:

```text
dist/package/BepInEx/plugins/SpecialRatizens
```

into:

```text
E:\steam\steamapps\common\Ratopia\BepInEx\plugins\SpecialRatizens
```

Use literal resolved paths and `-Force`; do not delete or recurse over the parent `plugins` directory.

Use:

```powershell
$stageRoot = 'D:\SOFTWARE\项目\鼠托邦mod\SpecialRatizens\dist\package\BepInEx\plugins\SpecialRatizens'
$installedRoot = 'E:\steam\steamapps\common\Ratopia\BepInEx\plugins\SpecialRatizens'

if (@(Get-Process -Name Ratopia -ErrorAction SilentlyContinue).Count -gt 0) {
    throw 'Ratopia is running; installation stopped.'
}

$pluginsRoot = [IO.Path]::GetFullPath('E:\steam\steamapps\common\Ratopia\BepInEx\plugins').TrimEnd('\')
$installedFull = [IO.Path]::GetFullPath($installedRoot)
if (-not $installedFull.StartsWith($pluginsRoot + '\', [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Install target escaped the BepInEx plugins directory.'
}

foreach ($item in Get-ChildItem -LiteralPath $stageRoot -Force) {
    Copy-Item -LiteralPath $item.FullName -Destination $installedRoot -Recurse -Force
}
```

- [ ] **Step 4: Verify the installed artifact independently**

Verify all of the following in one read-only pass:

- Ratopia is still closed.
- Installed file count is 27, Data file count is 26, and PNG count is 24.
- Source/staged/installed DLL SHA-256 values are identical.
- Installed assembly version is `0.1.1.0`.
- The special-ratizen GUID appears in exactly one installed DLL.
- All DLLs belonging to other Mods retain their pre-install SHA-256.
- The current `SaveFile` tree still matches the just-created backup.

- [ ] **Step 5: Hand off the manual runtime acceptance test**

Do not start the game. Ask the user to launch Ratopia and test without saving first:

1. Load the affected save and inspect ordinary citizens.
2. Generate or inspect a special ratizen and verify body, clothes, hair, face, and accessories.
3. Trigger default/work-clothes switching.
4. Return to the title and load the same or another save again in the same process.
5. Confirm ordinary citizens remain original and special ratizens remain complete.

If any check fails, have the user close Ratopia and provide the fresh `BepInEx/LogOutput.log` and `Player.log`; restore the timestamped plugin and save backups only after confirming whether the test session wrote a save.
