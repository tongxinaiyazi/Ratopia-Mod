# SuperBow All Damageable Targets Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make WoodBow splash and bleed apply to every non-friendly target that the original queen arrow can actually damage, including `AnimalBody`, `MapObj`, and enemy buildings.

**Architecture:** Keep `Bow_Arrow.OnTriggerEnter2D` as the single attack hook. Capture a typed runtime target and its HP in Prefix, let vanilla execute, and only process Mod effects in Postfix when HP decreased. Wrap all supported game target types behind one runtime adapter and use the existing generic bleed timeline with adapter identity.

**Tech Stack:** C# 7.3, .NET Framework 4.7.2, BepInEx 5.4.23.5 Mono, Harmony 2.9.0.0, xUnit 2.9.2, Mono.Cecil 0.11.6.

## Global Constraints

- Work only in the primary agent; do not use subagents.
- Game directory is `E:\steam\steamapps\common\Ratopia`.
- Keep save markers `RangeAtk=1` and `BloodDrain=3`; do not change save structure.
- Only `EnemyCategory.Boss` uses 1% max-HP bleed per tick; every other supported target uses 3%.
- Splash radius stays 1.5 and damage stays 50% of arrow direct damage without recursion.
- Tooltip text stays exactly `流血`.
- Version becomes `0.1.2`.
- Game, Unity, BepInEx, and Harmony references remain `Private=false` and are never packaged.
- This directory is not a Git repository, so commit steps are replaced by local verification checkpoints.

---

### Task 1: Pure hit confirmation contract

**Files:**
- Create: `src/SuperBow/Core/HitConfirmation.cs`
- Create: `src/SuperBow/Core/CombatTargetKind.cs`
- Create: `tests/SuperBow.Tests/CombatTargetRulesTests.cs`

**Interfaces:**
- Produces: `CombatTargetKind` values `GameUnit`, `AnimalBody`, `MapObject`, `Building`.
- Produces: `HitConfirmation.DidTakeDamage(float before, float after)`.

- [ ] **Step 1: Write the failing tests**

```csharp
[Theory]
[InlineData(100f, 90f, true)]
[InlineData(100f, 100f, false)]
[InlineData(100f, 101f, false)]
[InlineData(0f, 0f, false)]
public void Vanilla_hit_is_confirmed_only_by_hp_decrease(
    float before, float after, bool expected)
{
    Assert.Equal(expected, HitConfirmation.DidTakeDamage(before, after));
}

[Fact]
public void Target_kinds_cover_every_vanilla_arrow_damage_branch()
{
    Assert.Equal(
        new[] { "GameUnit", "AnimalBody", "MapObject", "Building" },
        Enum.GetNames(typeof(CombatTargetKind)));
}
```

- [ ] **Step 2: Run the focused test and verify RED**

Run:

```powershell
dotnet test .\tests\SuperBow.Tests\SuperBow.Tests.csproj -c Release "/p:RatopiaDir=E:\steam\steamapps\common\Ratopia" /p:InstallAfterBuild=false --filter "FullyQualifiedName~CombatTargetRulesTests" --nologo
```

Expected: compilation fails because `HitConfirmation` and `CombatTargetKind` do not exist.

- [ ] **Step 3: Add the minimal pure implementation**

```csharp
namespace SuperBow.Core
{
    public enum CombatTargetKind
    {
        GameUnit,
        AnimalBody,
        MapObject,
        Building
    }

    public static class HitConfirmation
    {
        public static bool DidTakeDamage(float before, float after)
        {
            return before > 0f && after < before;
        }
    }
}
```

- [ ] **Step 4: Re-run the focused test and verify GREEN**

Expected: all `CombatTargetRulesTests` pass.

---

### Task 2: Runtime target adapter and confirmed direct hits

**Files:**
- Create: `src/SuperBow/Runtime/RuntimeCombatTarget.cs`
- Modify: `src/SuperBow/Runtime/BowHitState.cs`
- Modify: `src/SuperBow/Patches/BowArrowHitPatch.cs`
- Modify: `tests/SuperBow.Tests/PluginSourceContractTests.cs`
- Modify: `tests/SuperBow.Tests/GameAssemblyContractTests.cs`

**Interfaces:**
- Produces: `RuntimeCombatTarget.TryFromCollision(Collider2D, T_Queen, out RuntimeCombatTarget)`.
- Produces: `RuntimeCombatTarget.CurrentHealth`, `MaxHealth`, `Kind`, `CenterX`, `CenterY`, `IsAlive`, `IsBoss`, `ApplyDamage(float)`. The wrapper retains the owning queen for attribution, while equality uses only kind plus target reference.
- Changes: `BowHitState` stores `RuntimeCombatTarget Target` and `float HealthBeforeVanilla`.

- [ ] **Step 1: Add failing source and assembly contracts**

Require production source to mention all four target types and `HitConfirmation.DidTakeDamage`. Add exact Cecil contracts for:

```csharp
AssertMethod(assembly, "AnimalBody", "BeAttacked", "System.Void",
    "System.Single", "Unit_Attacekd_Tag");
AssertMethod(assembly, "MapObj", "BeAttacked", "System.Void", "System.Single");
AssertMethod(assembly, "Building", "BeAttacked", "System.Void",
    "System.Single", "UnityEngine.Vector2", "Unit_Attacekd_Tag");
AssertField(assembly, "AnimalBody", "m_CurHP", "System.Single");
AssertField(assembly, "AnimalBody", "m_MaxHP", "System.Single");
AssertField(assembly, "MapObj", "m_CurHp", "System.Single");
AssertField(assembly, "MapObj", "m_MaxHp", "System.Single");
AssertField(assembly, "Building", "m_CurHP", "System.Single");
AssertField(assembly, "Building", "m_MaxHP", "System.Single");
```

- [ ] **Step 2: Run those contracts and verify RED**

Expected: source contract fails because `RuntimeCombatTarget.cs` and confirmed-hit usage are missing.

- [ ] **Step 3: Implement the runtime adapter**

Use an immutable wrapper keyed by `CombatTargetKind` plus reference identity. Collision discovery follows vanilla order:

```csharp
var unit = Helpers.GetGameUnitByCollision(collider);
if (unit != null && Helpers.IsTeamEnemy(unit)) { ... }
var animal = collider.GetComponent<AnimalBody>();
if (animal != null) { ... }
var mapObject = collider.GetComponent<MapObj>();
if (mapObject != null) { ... }
var building = collider.GetComponent<Building>();
if (building != null && building.m_Info.Name == BuildingName.EnemyNexus) { ... }
```

Dispatch damage through the matching original entry:

```csharp
unit.BeAttacked(-damage, Unit_Attacekd_Tag.Queen, 0);
animal.BeAttacked(-damage, Unit_Attacekd_Tag.Queen);
mapObject.BeAttacked(-damage);
building.BeAttacked(-damage, queen.Tf.position, Unit_Attacekd_Tag.Queen);
```

- [ ] **Step 4: Change Prefix/Postfix to confirm vanilla damage**

Prefix captures adapter and HP only. Postfix requires:

```csharp
HitConfirmation.DidTakeDamage(
    state.HealthBeforeVanilla,
    state.Target.CurrentHealth)
```

before calling the combat runtime. Move the supported-hit log after this confirmation and include `Target.Kind`.

- [ ] **Step 5: Run focused and full tests**

Expected: new contracts and all existing tests pass without warnings.

---

### Task 3: Unified splash enumeration and bleed timeline

**Files:**
- Modify: `src/SuperBow/Runtime/RuntimeCombatTarget.cs`
- Modify: `src/SuperBow/Runtime/CombatRuntime.cs`
- Modify: `tests/SuperBow.Tests/BleedTrackerTests.cs`
- Modify: `tests/SuperBow.Tests/PluginSourceContractTests.cs`

**Interfaces:**
- Produces: `RuntimeCombatTarget.EnumerateSplashCandidates(T_Queen)` over enemy units, animals, map objects, and enemy buildings.
- Changes: `BleedTracker<GameUnit>` to `BleedTracker<RuntimeCombatTarget>`.

- [ ] **Step 1: Add failing contracts for all four manager lists and generic target bleed**

Require source to use:

```text
_T_UnitMgr.List_AllEnemy
_AnimalMgr.List_Animal
_MapObjMgr.List_MapObj
_BuildingMgr.List_Building
BleedTracker<RuntimeCombatTarget>
```

Add Cecil field contracts for the four manager fields and four lists.

- [ ] **Step 2: Verify RED**

Expected: source contract fails because combat still uses `BleedTracker<GameUnit>` and only `List_AllEnemy`.

- [ ] **Step 3: Implement unified candidate enumeration**

Snapshot each runtime list with `ToArray()`. Wrap only alive, active targets the vanilla queen arrow may damage. For buildings, include only `BuildingName.EnemyNexus` in a damageable state. De-duplicate by wrapper reference identity and exclude the primary target before applying `SplashRules.ShouldDamage`.

- [ ] **Step 4: Generalize combat processing**

Use the adapter for direct bleed, splash damage, splash-applied bleed, tick validity, max HP, Boss policy, and damage dispatch:

```csharp
var damage = tick.Target.MaxHealth * tick.Fraction;
tick.Target.ApplyDamage(damage);
```

Only adapters wrapping `GameEnemy` with `EnemyCategory.Boss` return `IsBoss=true`; all other adapters receive 3% ticks.

- [ ] **Step 5: Add multi-kind refresh tests and run all tests**

Use distinct reference tokens with `BleedTracker<object>` to prove two kinds refresh independently, do not stack, and each emits three normal 3% ticks; keep the existing Boss 1% tests.

Expected: full Release suite passes.

---

### Task 4: Version, documentation, build, package, and install

**Files:**
- Modify: `src/SuperBow/Plugin.cs`
- Modify: `src/SuperBow/SuperBow.csproj`
- Modify: `README.md`
- Modify: `scripts/Package.ps1`
- Modify: `tests/SuperBow.Tests/PluginSourceContractTests.cs`
- Modify: `tests/SuperBow.Tests/ReleaseArtifactContractTests.cs`

**Interfaces:**
- Produces release `SuperBow-v0.1.2-BepInEx5.zip`.

- [ ] **Step 1: Change release contract expectations to 0.1.2 and verify RED**

Expected: tests fail while sources still contain 0.1.1.

- [ ] **Step 2: Update all version surfaces and README**

Document that every target actually damageable by a vanilla queen arrow is supported, with Boss-only 1% and all other targets 3%.

- [ ] **Step 3: Run the full package script**

```powershell
& .\scripts\Package.ps1 -RatopiaDir 'E:\steam\steamapps\common\Ratopia'
```

Expected: clean, 0 failed tests, 0 build errors, and a two-file archive.

- [ ] **Step 4: Audit package contents**

```powershell
& 'C:\Users\ASUS\.codex\skills\developing-ratopia-mods\scripts\Test-RatopiaPackage.ps1' -Path '.\dist\SuperBow-v0.1.2-BepInEx5.zip'
```

Expected: no forbidden, unexpected, or error entries.

- [ ] **Step 5: Install safely**

Require zero Ratopia processes. Back up the installed DLL with a timestamp, copy `src\SuperBow\bin\Release\net472\SuperBow.dll`, and require source/target SHA-256 equality.

- [ ] **Step 6: Verify runtime gates**

Require plugin 0.1.2 discovery, all Harmony patches, database initialization, and no SuperBow errors. Manual behavior acceptance must show confirmed target-kind logs plus splash and bleed tick logs for `GameUnit`, `Flaone`, `FlaoneHole`, and a Boss.
