# Special Ratizens BepInEx 5 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Finish and verify the isolated BepInEx 5 migration of RatopiaMod's complete Special Ratizens feature without installing or launching Ratopia.

**Architecture:** Keep pure CSV, validation, selection, and session state in `Core`. Use a single BepInEx 5 entry point and an explicit Harmony whitelist that delegates only retained formulas through fault-isolating adapters; the legacy class cannot self-register or patch itself.

**Tech Stack:** C# / net472, BepInEx 5.4.23.5, Harmony 2.9.0, xUnit 2.9.2, Mono.Cecil, PowerShell packaging.

## Global Constraints

- The Ratopia directory is `E:\steam\steamapps\common\Ratopia` and is read-only for this task.
- Target `Assembly-CSharp.dll` SHA-256 is `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`.
- Preserve all 12 ratizens, 24 traits, 24 referenced icons, original probabilities, and original effect formulas.
- Do not include game, Unity, BepInEx, Harmony, PDB, logs, saves, or test assemblies in the ZIP.
- Do not claim runtime behavior without a human game test.

---

### Task 1: Lock down plugin-local data discovery

**Files:**
- Modify: `tests/SpecialRatizens.Tests/PluginContractTests.cs`
- Modify: `src/SpecialRatizens/Plugin.cs`

**Interfaces:**
- Consumes: `PluginDataPaths.ResolveDataRoot(string assemblyLocation)`.
- Produces: an absolute `<plugin-dll-directory>/Data` path independent of the containing folder name.

- [ ] **Step 1: Write the failing test**

```csharp
[Fact]
public void DataRootIsResolvedBesideThePluginAssembly()
{
    var location = Path.Combine("X:\\mods\\RenamedFolder", "SpecialRatizens.dll");
    Assert.Equal(
        Path.Combine("X:\\mods\\RenamedFolder", "Data"),
        PluginDataPaths.ResolveDataRoot(location));
}
```

- [ ] **Step 2: Run the test and verify RED**

Run: `dotnet test SpecialRatizens.sln -c Release /p:RatopiaDir="E:\steam\steamapps\common\Ratopia" --filter DataRootIsResolvedBesideThePluginAssembly`

Expected: FAIL because `PluginDataPaths` does not exist.

- [ ] **Step 3: Implement the smallest path helper and use it from `Awake`**

```csharp
internal static string ResolveDataRoot(string assemblyLocation)
{
    if (string.IsNullOrWhiteSpace(assemblyLocation))
        throw new ArgumentException("插件程序集路径不能为空。", nameof(assemblyLocation));
    return Path.Combine(Path.GetDirectoryName(Path.GetFullPath(assemblyLocation)), "Data");
}
```

- [ ] **Step 4: Run the focused and full test suite**

Expected: focused test PASS, then all tests PASS.

### Task 2: Contract-test Harmony parameter names

**Files:**
- Modify: `tests/SpecialRatizens.Tests/GameContractTests.cs`

**Interfaces:**
- Consumes: the exact target method parameter names from the inspected game assembly.
- Produces: `AssertMethodParameters` checks for every adapter argument that Harmony binds by name.

- [ ] **Step 1: Add a contract assertion for names used by adapters**

```csharp
AssertMethodParameters(module, "T_Citizen", "MakeCtizen_ByCC", new[] { "pos", "_info" });
AssertMethodParameters(module, "T_Citizen", "BeAttacked", new[] { "dmg", "_tag", "_id" });
AssertMethodParameters(module, "BuffIcon", "IconSet", new[] { "Tf_parent", "_info" });
```

Add equivalent assertions for every retained target whose adapter accepts original arguments.

- [ ] **Step 2: Run the contract test**

Run: `dotnet test SpecialRatizens.sln -c Release /p:RatopiaDir="E:\steam\steamapps\common\Ratopia" --filter HarmonyAdapterParameterNamesMatchTheInspectedBuild`

Expected: PASS only if all injected names match the inspected assembly. If a name differs, change the adapter parameter to the game name and rerun.

### Task 3: Reject ambiguous external data

**Files:**
- Modify: `tests/SpecialRatizens.Tests/SpecialDataCatalogTests.cs`
- Modify: `src/SpecialRatizens/Core/SpecialDataCatalog.cs`

**Interfaces:**
- Consumes: all unlocked `char1` and `char2` references.
- Produces: a validated catalog in which each custom trait belongs to exactly one ratizen.

- [ ] **Step 1: Write a failing duplicate-ownership test**

Append a second unit that reuses `Rat_A`, load the catalog, and assert `InvalidDataException` contains `多个特殊鼠鼠`.

- [ ] **Step 2: Verify RED**

Expected: current catalog load succeeds, proving the new test detects the missing validation.

- [ ] **Step 3: Add ownership validation**

Track referenced trait names in an ordinal dictionary while parsing ratizens; reject a trait already owned by another ratizen before returning the catalog.

- [ ] **Step 4: Verify GREEN and run all data tests**

Expected: duplicate ownership is rejected and the shipped 12/24 catalog still passes.

### Task 4: Verify release and rebuild package

**Files:**
- Modify if required: `README.md`
- Modify if required: `scripts/Package.ps1`
- Generate: `dist/特殊鼠鼠-v0.1.0-BepInEx5.zip`

**Interfaces:**
- Produces: a root-installable ZIP containing only `BepInEx/plugins/SpecialRatizens/{SpecialRatizens.dll,Data/**}` and `README.md`.

- [ ] **Step 1: Run all tests and a clean Release build**

Run: `dotnet test SpecialRatizens.sln -c Release /p:RatopiaDir="E:\steam\steamapps\common\Ratopia" /p:InstallAfterBuild=false`

Run: `dotnet build src/SpecialRatizens/SpecialRatizens.csproj -c Release /p:RatopiaDir="E:\steam\steamapps\common\Ratopia" /p:InstallAfterBuild=false --no-restore`

Expected: zero failures and zero build errors.

- [ ] **Step 2: Run the package script**

Run: `powershell -ExecutionPolicy Bypass -File scripts/Package.ps1 -RatopiaDir "E:\steam\steamapps\common\Ratopia"`

Expected: ZIP created under `dist`; the script does not write to the game directory.

- [ ] **Step 3: Run the Ratopia package validator**

Run: `powershell -ExecutionPolicy Bypass -File "C:\Users\ASUS\.codex\skills\developing-ratopia-mods\scripts\Test-RatopiaPackage.ps1" -Path "dist\特殊鼠鼠-v0.1.0-BepInEx5.zip" -ExpectedPluginName "SpecialRatizens"`

Expected: package valid, one plugin DLL, no forbidden DLLs or development artifacts.

- [ ] **Step 4: Compare the game plugin inventory with the pre-build snapshot**

Expected: the six pre-existing plugin DLL paths and SHA-256 hashes are unchanged; no `SpecialRatizens.dll` exists in the game tree.

- [ ] **Step 5: Perform a final source and package review**

Confirm the plugin is the only `BepInPlugin` entry, the whitelist contains no unrelated feature patches, the ZIP matches the freshly built DLL hash, and README states save/uninstall risk plus the lack of runtime verification.
