# 电力刷新兼容修复 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 让同格端口协调保持幂等，避免无变化验证和线路修复重跑全城供电判定，从而消除量子电网等外部电力规则下的虚假缺电警报。

**Architecture:** `PortOverlayRegistry.Reconcile` 在一次遍历中记录线路恢复、线路合并和代表变化，并以这些变化决定是否局部刷新。全局耗电/发电建筑刷新从端口协调器移除；会话与周期验证入口保持不变。

**Tech Stack:** C#、.NET Framework 4.7.2、BepInEx 5 Mono、Harmony 2.9.0、xUnit 2.9.2、Mono.Cecil

## Global Constraints

- 目标游戏目录为 `E:\steam\steamapps\common\Ratopia`。
- 目标 `Assembly-CSharp.dll` SHA-256 为 `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`。
- 不新增配置、外部 API 或自定义存档字段。
- 不探测或引用“特殊鼠鼠”程序集。
- Ratopia 进程运行时不得覆盖已加载的插件 DLL。

---

### Task 1: 锁定无变化刷新回归

**Files:**
- Modify: `tests/WireThroughWalls.Tests/PluginContractTests.cs`
- Modify: `src/WireThroughWalls/Runtime/PortOverlayRegistry.cs`

**Interfaces:**
- Consumes: `PortOverlayRegistry.Reconcile(Vector2Int)` 及游戏 `BuildingMgr` 电力接口。
- Produces: 幂等的 `Reconcile(Vector2Int)`；无新增公共接口。

- [ ] **Step 1: 写失败合同测试**

在 `PluginContractTests` 中读取 `Reconcile` 的 Mono.Cecil 调用列表，断言不包含
`BuildingMgr::RefreshElecUseBuilding` 和 `BuildingMgr::RefreshElecMakeBuilding`；同时断言
`BuildingMgr::RefreshWire` 前存在基于变化布尔值的条件分支。

- [ ] **Step 2: 运行定向测试并验证 RED**

Run:

```powershell
dotnet test .\tests\WireThroughWalls.Tests\WireThroughWalls.Tests.csproj -c Release `
  --filter "FullyQualifiedName~PluginContractTests" `
  --artifacts-path "$env:TEMP\WireThroughWalls-power-red" `
  "/p:RatopiaDir=E:\steam\steamapps\common\Ratopia" `
  /p:InstallAfterBuild=false
```

Expected: 新测试因 `Reconcile` 仍调用两个全局刷新方法而失败。

- [ ] **Step 3: 实现最小幂等修复**

在 `Reconcile` 中维护 `topologyChanged` 和 `representativeChanged`：

```csharp
var topologyChanged = false;
var representativeChanged = false;
```

仅在缺失线路被 `NewConnectCheck` 恢复或不同线路被合并时设置
`topologyChanged = true`。比较现有 `Dic_PortTileMap` 端口的 ID、类型和坐标，仅在不一致
时写入代表并设置 `representativeChanged = true`。只有两者任一为真时调用
`RefreshWire(position)`；只有 `topologyChanged` 为真时调用 `primary.ActRefreshByDynamo()`；
删除两个全局电力刷新调用。

- [ ] **Step 4: 运行定向测试并验证 GREEN**

Run: 与 Step 2 相同。

Expected: 所有 `PluginContractTests` 通过，且无失败、跳过或编译警告。

### Task 2: 全套合同与发布门禁

**Files:**
- Verify: `WireThroughWalls.sln`
- Verify: `src/WireThroughWalls/Runtime/PortOverlayRegistry.cs`
- Verify: `tests/WireThroughWalls.Tests/PluginContractTests.cs`

**Interfaces:**
- Consumes: Task 1 的幂等协调行为。
- Produces: 可供实机验证的 Release DLL；游戏运行时不安装。

- [ ] **Step 1: 运行干净 Release 全套测试**

```powershell
dotnet test .\WireThroughWalls.sln -c Release `
  --artifacts-path "$env:TEMP\WireThroughWalls-power-fix" `
  "/p:RatopiaDir=E:\steam\steamapps\common\Ratopia" `
  /p:InstallAfterBuild=false
```

Expected: 全部测试通过，0 failed，0 skipped。

- [ ] **Step 2: 检查构建输出和进程状态**

```powershell
Get-Process Ratopia -ErrorAction SilentlyContinue
Get-FileHash "$env:TEMP\WireThroughWalls-power-fix\bin\WireThroughWalls\release\WireThroughWalls.dll" -Algorithm SHA256
```

Expected: 记录构建 DLL 哈希；若 Ratopia 仍运行，只报告待安装，不覆盖插件。

- [ ] **Step 3: 游戏退出后备份并安装**

使用现有 `scripts/Install.ps1` 或项目约定的安装流程，先备份
`BepInEx/plugins/WireThroughWalls/WireThroughWalls.dll`，再安装新 DLL，并比较构建与安装
DLL 的 SHA-256。

- [ ] **Step 4: 实机验证**

载入包含奥米伽-7量子电网和同格电线的备份存档，连续观察至少两个周期验证间隔；确认
建筑不再重复出现虚假缺电警报，并验证电线/前景建筑的 F 键选择、分别拆除及保存重载。

### Task 3: 生成 v0.1.3 修复发行版

**Files:**
- Modify: `src/WireThroughWalls/Plugin.cs`
- Modify: `src/WireThroughWalls/WireThroughWalls.csproj`
- Modify: `README.md`
- Modify: `docs/TESTING.md`
- Create: `docs/NEXUS-v0.1.3.md`
- Modify: `scripts/Package.ps1`

**Interfaces:**
- Consumes: Task 1 的修复和 Task 2 的门禁结果。
- Produces: 版本统一为 `0.1.3` 的插件 DLL 与 Nexus 发布压缩包。

- [ ] **Step 1: 将插件版本合同改为 0.1.3 并验证 RED**

把 `PluginContractTests.PluginMetadataIsStable` 的期望版本改为 `0.1.3`，运行该测试并确认
它因插件仍报告 `0.1.2` 而失败。

- [ ] **Step 2: 同步版本与发行说明**

将 `Plugin.PluginVersion`、项目 `Version`/`AssemblyVersion`/`FileVersion`、README 当前版本、
测试说明和压缩包文件名统一为 `0.1.3`，新增 Nexus 更新说明，记录虚假缺电根因与修复。

- [ ] **Step 3: 重新运行全套 Release 门禁**

运行 Task 2 Step 1 的干净测试命令，Expected: 85/85 或更多测试全部通过。

- [ ] **Step 4: 生成并审计发布包**

```powershell
.\scripts\Package.ps1 -RatopiaDir 'E:\steam\steamapps\common\Ratopia'
```

Expected: `dist/电线可穿墙-v0.1.3-BepInEx5.zip` 仅包含 README 与
`BepInEx/plugins/WireThroughWalls/WireThroughWalls.dll`，包内 DLL 的 SHA-256 与构建 DLL
一致。
