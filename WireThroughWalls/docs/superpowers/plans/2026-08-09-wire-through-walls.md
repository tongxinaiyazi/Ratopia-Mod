# 电线可穿墙 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 将鼠托邦的高压电线改成不占用前景格的背景建筑，使它可以与墙、普通建筑及对应蓝图同格存在，同时保持电力连接、显示和原版资源规则。

**Architecture:** Mod 不替换游戏资源或存档结构，而是在建造检查期间提供临时透明视图，并在蓝图、建成、拆除和读档生命周期结束后重新计算格子的前景 `TileType`。纯规则和集合作用域与 Unity/Harmony 适配分离；所有临时隐藏都必须可嵌套、幂等释放并由 Harmony Finalizer 恢复。

**Tech Stack:** C# / net472 / BepInEx 5.4.23.5 / Harmony 2.9.0 / Unity Mono / xUnit / Mono.Cecil

## Global Constraints

- 独立根目录：`WireThroughWalls`；不得修改现有 `SharedWarehouse` 或其他 Mod。
- 插件 GUID：`cn.ratopia.wirethroughwalls`；名称：`电线可穿墙`；版本：`0.1.0`。
- 仅处理 `BuildingName.HeavyWire` / `BuildAbility.HeavyWire`，不处理 `Wireroad`。
- 不写自定义存档字段，不改材质消耗、电力端口或电线渲染层。
- 构建和测试始终使用 `/p:InstallAfterBuild=false`；Ratopia 关闭后才能安装。
- 发布包只包含本 Mod DLL 与 README，不包含游戏、Unity、BepInEx、Harmony、PDB、bin/obj 或存档。
- 当前工作区不是 Git 仓库，因此计划中的提交步骤不适用；通过独立目录、测试门禁与已安装 DLL 时间戳备份实现隔离和回滚。

---

### Task 1: 纯占位规则与可恢复集合视图

**Files:**
- Create: `src/WireThroughWalls/Core/OverlayRules.cs`
- Create: `src/WireThroughWalls/Core/ScopedListMask.cs`
- Test: `tests/WireThroughWalls.Tests/OverlayRulesTests.cs`
- Test: `tests/WireThroughWalls.Tests/ScopedListMaskTests.cs`

**Interfaces:**
- Produces: `OverlayRules.ResolveForegroundTileType(bool, bool, int?) -> int`
- Produces: `OverlayRules.CanBlueprintsShare(bool, bool) -> bool`
- Produces: `ScopedListMask<T>.RemoveWhere(IList<T>, Predicate<T>) -> ScopedListMask<T>`

- [ ] Write tests for building/blueprint/terrain/empty precedence, wire-foreground sharing, duplicate rejection, list order restoration, nesting and idempotent disposal.
- [ ] Run `dotnet test tests/WireThroughWalls.Tests/WireThroughWalls.Tests.csproj -c Release /p:InstallAfterBuild=false` and confirm RED for missing behavior.
- [ ] Implement the smallest pure rules and restoration scope.
- [ ] Re-run the same command and confirm GREEN.

### Task 2: 锁定本机游戏合同

**Files:**
- Create: `tests/WireThroughWalls.Tests/GameContractTests.cs`

**Interfaces:**
- Consumes: `Assembly-CSharp.dll` at `$(RatopiaDir)\Ratopia_Data\Managed`.
- Produces: static checks for exact types, overloads, enum values and fields used by Harmony.

- [ ] Write Mono.Cecil tests for `MiningBox.BuildEnableCheck/Update`, `BP_Building` lifecycle, `Building_HeavyWire` lifecycle, `BuildingMgr.FindBuildingByBpos`, `C_Tile.DestroyTile` and required fields.
- [ ] Run the contract test and confirm it passes only against the inspected 2026-07-24 assembly (`SHA256 C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`).

### Task 3: 前景格协调器与会话修复

**Files:**
- Create: `src/WireThroughWalls/Runtime/WireOverlayCoordinator.cs`
- Create: `src/WireThroughWalls/Runtime/NodeTileSnapshot.cs`
- Create: `src/WireThroughWalls/Runtime/WireActionScope.cs`
- Create: `src/WireThroughWalls/Plugin.cs`

**Interfaces:**
- Consumes: `BuildingMgr`, `TileMgr`, completed buildings, blueprints, tiles and heavy-wire lists.
- Produces: `ReconcilePosition`, `ReconcileAll`, action/demolition scopes and per-session polling.

- [ ] Add tests for session gating and nested action scope state before runtime implementation.
- [ ] Implement foreground resolution: non-wire completed building, enabled non-wire blueprint, terrain tile, then `None`.
- [ ] Implement manager-change initialization and periodic idempotent reconciliation after `m_GameLoading` becomes false.
- [ ] Install Harmony patch classes one-by-one with diagnostics; on installation failure unpatch self and disable the feature.

### Task 4: 建造与蓝图透明视图

**Files:**
- Create: `src/WireThroughWalls/Patches/PlacementPatches.cs`
- Create: `src/WireThroughWalls/Patches/BlueprintPatches.cs`

**Interfaces:**
- Prefix captures/removes only the entries that should be transparent; Postfix and Finalizer dispose the same idempotent state.
- `C_Tile.DestroyTile` suppression applies only inside an active wire-completion scope and only to the scope's target positions.

- [ ] Test and implement `MiningBox.BuildEnableCheck` masking: wire ignores foreground occupancy/non-wire blueprints; non-wire ignores wire blueprints; wire+wire stays rejected.
- [ ] Test and implement `BP_Building.BluePrintSet`, `EnableCheck`, `BuildingUpdate_Call` and `CancelBP` reconciliation.
- [ ] Keep all original support, map, material and work rules by restoring the exact original nodes/lists after the original call.
- [ ] Prevent wire completion from destroying a wall and prevent road completion from demolishing an overlapping wire.

### Task 5: 建成电线与拆除优先级

**Files:**
- Create: `src/WireThroughWalls/Patches/WireBuildingPatches.cs`
- Create: `src/WireThroughWalls/Patches/DemolitionSelectionPatches.cs`

**Interfaces:**
- Patches exact `Building_HeavyWire.BuildingSet(BuildInfo, Vector2, int)` and `BuildingDemolition(bool)` overloads.
- During `MiningBoxMode.Demolition` only, `BuildingMgr.FindBuildingByBpos(Vector2Int)` returns overlapping wire before foreground building.

- [ ] Preserve `List_BuildPos` and `ElecPort` behavior by allowing original heavy-wire lifecycle methods to run unchanged.
- [ ] Reconcile every affected foreground node after build/load/demolition writes.
- [ ] Scope wire-first selection to demolition `MiningBox.Update`; do not change repair or general building queries.

### Task 6: Build, package and install gates

**Files:**
- Create: `README.md`
- Create: `scripts/Package.ps1`

- [ ] Run the full Release test suite with automatic installation disabled.
- [ ] Build the net472 plugin with all game/Unity/BepInEx/Harmony references `Private=false` and inspect the output dependency set.
- [ ] Package `BepInEx/plugins/WireThroughWalls/WireThroughWalls.dll` plus root `README.md`.
- [ ] Run `Test-RatopiaPackage.ps1 -ExpectedPluginName WireThroughWalls` against both stage directory and ZIP.
- [ ] Confirm Ratopia is closed, enumerate residual plugins/patchers, back up an existing target DLL if present, copy only the built DLL, and compare SHA-256.

### Task 7: Runtime acceptance

**Files:**
- Create: `docs/TESTING.md`

- [ ] Verify discovery, every patch installation log, session initialization and first reconciliation separately.
- [ ] In a disposable test save, test wall+wire and building+wire in both construction orders, both blueprint orders, cancel/finish, duplicate-wire rejection, wire-first demolition and electrical connectivity.
- [ ] Save/reload twice and verify the same overlaps; then temporarily remove the Mod DLL and confirm the save remains readable before restoring it.
- [ ] Record any acceptance step that cannot be safely automated as unverified rather than claiming completion.

---

## Plan self-review

- Spec coverage: HeavyWire-only targeting, foreground precedence, both placement directions, blueprint lifecycle, tile-destruction protection, road coexistence, demolition priority, session repair, packaging, installation and save/reload are each assigned to a task.
- Placeholder scan: no implementation placeholder or deferred requirement remains.
- Type consistency: coordinator, pure rules, list mask and patch scope names are consistent across tasks.
