# SuperBow Combat Fix Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. The user explicitly requested no subagents.

**Goal:** Scope the two special reforge candidates to WoodBow, restore verifiable arrow combat processing, and shorten the bleed tooltip to `流血`.

**Architecture:** Keep the shared game database unchanged outside an active WoodBow reforge context. A pure identity rule decides whether an item is WoodBow; the runtime catalog owns reversible contextual candidate patches; thin Harmony prefixes provide the current item and combat entry evidence.

**Tech Stack:** C# 7.3, .NET Framework 4.7.2, BepInEx 5.4.23.5, Harmony 2.9.0.0, xUnit, Mono.Cecil.

## Global Constraints

- Game directory: `E:\steam\steamapps\common\Ratopia`.
- Preserve save markers `RangeAtk=1` and `BloodDrain=3`.
- Do not change range or bleed damage/timing constants.
- All game/runtime references remain `Private=false`.
- Build and test with `/p:InstallAfterBuild=false` before installation.
- Workspace is not a Git repository; use verified checkpoints instead of commits.

---

### Task 1: Regression tests and identity rule

**Files:**
- Create: `src/SuperBow/Core/QueenBowIdentity.cs`
- Modify: `tests/SuperBow.Tests/SplashAndTooltipRulesTests.cs`
- Modify: `tests/SuperBow.Tests/PluginSourceContractTests.cs`
- Modify: `tests/SuperBow.Tests/ReleaseArtifactContractTests.cs`

**Interfaces:**
- Produces: `QueenBowIdentity.IsMatch(int index, int type, string name)`.
- Locks: tooltip `流血`, version `0.1.1`, contextual Harmony targets, entry diagnostics.

- [ ] Write tests that accept only `(1, 1, "WoodBow")`, reject `(0, 1, "Gradius")`, require `TooltipRules.BleedText == "流血"`, and require contextual patch source.
- [ ] Run `dotnet test .\SuperBow\SuperBow.sln -c Release /p:RatopiaDir=E:\steam\steamapps\common\Ratopia /p:InstallAfterBuild=false` and confirm the new assertions fail for missing behavior.
- [ ] Implement `QueenBowIdentity` and update tooltip/version constants only.
- [ ] Run the same command and confirm identity/tooltip/version tests pass while contextual source assertions remain red if not yet implemented.

### Task 2: Contextual candidate ownership

**Files:**
- Modify: `src/SuperBow/Runtime/RuntimeCatalog.cs`
- Modify: `src/SuperBow/Patches/DatabasePatches.cs`
- Modify: `src/SuperBow/Plugin.cs`
- Modify: `tests/SuperBow.Tests/PluginSourceContractTests.cs`

**Interfaces:**
- Consumes: `QueenBowIdentity.IsMatch`.
- Produces: `RuntimeCatalog.SetReforgeContextSafely(ItemInfo item)` and `RuntimeCatalog.ClearReforgeContext()`.

- [ ] Add failing source/contract assertions for `BuildMidUI.ItemDetail_Open`, `T_Queen.ItemEnhance`, context set and context clear.
- [ ] Run the focused contract tests and confirm failure is caused by missing contextual patches.
- [ ] Refactor `RuntimeCatalog` so the base session owns only the ATK patch and a separate disposable session owns RangeAtk/BloodDrain candidates while WoodBow is the active context.
- [ ] Add thin Harmony prefixes for the exact inspected method signatures and clear contextual candidates on scene changes.
- [ ] Run the full Release test suite and confirm all tests pass.

### Task 3: Combat diagnostics and release

**Files:**
- Modify: `src/SuperBow/Patches/BowArrowHitPatch.cs`
- Modify: `src/SuperBow/Runtime/CombatRuntime.cs`
- Modify: `src/SuperBow/SuperBow.csproj`
- Modify: `src/SuperBow/Plugin.cs`
- Modify: `README.md`
- Modify: `scripts/Package.ps1`

**Interfaces:**
- Produces: distinct one-time logs for raw arrow collision and supported WoodBow enemy hit.
- Produces: `SuperBow-v0.1.1-BepInEx5.zip`.

- [ ] Move raw patch invocation logging before equipment filters and add a separate supported-hit log after all guards.
- [ ] Update all versioned release surfaces to `0.1.1` and document WoodBow-only candidates.
- [ ] Run `scripts/Package.ps1`; require 0 test failures, 0 build warnings/errors, and exact two-file archive contents.
- [ ] Run `Test-RatopiaPackage.ps1` against the ZIP and require no forbidden or unexpected files.
- [ ] Confirm Ratopia is closed, back up the installed DLL, copy the fixed DLL, and compare source/installed SHA-256.
- [ ] Start Ratopia for a smoke test, require plugin `0.1.1`, all patches, database initialization and no SuperBow error; restore the pre-test save snapshot and leave the final DLL installed.
