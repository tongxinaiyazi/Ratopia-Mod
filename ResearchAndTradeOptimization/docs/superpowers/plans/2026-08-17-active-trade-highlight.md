# 国家详情贸易中商品高亮实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在国家详情进出口列表中，为正在执行贸易协议的商品增加可配置颜色的背景与描边高亮；有限期与无限期贸易使用不同颜色，默认 `rgb(145, 135, 106)` 与 `rgb(96, 169, 23)`。

**Architecture:** 纯逻辑规则层提供"高亮类型"判定（`None`/`Limited`/`Infinite`）与 RGB 文本解析；Harmony 复用现有 `TradeWorldDetailPatch.Postfix` 追加运行时调用，不新增补丁类；运行时通过 FieldRef 读取槽位列表与物品类型，遍历 `DiplomaticCountryData.Sheets` 用公开 API `Resource`/`IsEnded()`/`IsInfinitePeriod()` 判定贸易状态与期限类型，动态创建铺满背景子物体并在 `_icon` 上启用 `Outline` 描边；两种颜色经 BepInEx `ConfigEntry<string>` 可配置，非法值回退对应默认色。

**Tech Stack:** C#、.NET Framework 4.7.2、BepInEx 5.4.23.5、Harmony 2.9.0、Unity 2021.3.21f1 UI、xUnit、Mono.Cecil。

## Global Constraints

- 插件名称、GUID 和版本保持 `研究与贸易优化`、`cn.ratopia.unlimitedresearchandtradequeues`、`0.3.0`。
- 目标 `Assembly-CSharp.dll` SHA-256 保持 `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`（当前机器 DLL 哈希不同，见任务 4）。
- 不新增 Harmony 补丁类：高亮挂载在现有 `TradeWorldDetailPatch.Postfix`。
- 高亮只作用于当前可见且正在贸易的槽位；不修改国家数据、贸易协议、价格或存档。
- 配置节 `TradeDetailSlot`、键 `ActiveTradeBackgroundColor`（默认 `145,135,106`）与 `InfiniteTradeBackgroundColor`（默认 `96,169,23`）。
- 程序集不引用名字含 `Configuration` 的程序集；Mod 类型名不含 `Save`/`Config`。
- 运行时异常只记录一次，不向游戏主循环传播。
- 构建和安装分离；仅在 Ratopia 退出后备份并覆盖安装 DLL。

---

### Task 1: 纯逻辑规则与颜色解析

**Files:**
- Modify: `src/ResearchAndTradeOptimization/Core/TradeResourceStateRules.cs`（untracked 骨架）
- Modify: `tests/ResearchAndTradeOptimization.Tests/TradeResourceStateRulesTests.cs`（untracked 骨架）

**Interfaces:**
- Consumes: 配置节/键/默认色字符串、是否可见、是否正在贸易。
- Produces: `ShouldHighlight(bool, bool)`、`ParseColorOrDefault(string, TradeHighlightColor)`、`TradeHighlightColor` 结构、`DefaultHighlightColor` 静态默认色。

- [x] **Step 1: 编写失败的颜色解析测试**

`TradeResourceStateRulesTests` 增加 `DefaultColorMatchesTheDocumentedRgbValue`、`ValidRgbTextIsParsedIntoChannels`、`InvalidRgbTextFallsBackToDefault`，覆盖 `145,135,106`、空白、`0,0,0`、`255,255,255`、非法格式（空、缺段、多段、非数字、越界、负值、小数）。

- [x] **Step 2: 运行定向测试确认红灯**

Run:

```powershell
dotnet test tests\ResearchAndTradeOptimization.Tests\ResearchAndTradeOptimization.Tests.csproj -c Release --no-restore --filter FullyQualifiedName~TradeResourceStateRulesTests -p:InstallAfterBuild=false -p:RatopiaDir=$env:RATOPIA_DIR
```

Expected: 因缺少 `TradeHighlightColor`/`ParseColorOrDefault`/`DefaultHighlightColor` 而失败。

- [x] **Step 3: 实现规则与解析**

`TradeResourceStateRules` 增加 `TradeHighlightColor`（R/G/B byte）结构与 `ParseColorOrDefault`：按 `,` 拆分、Trim、`byte.TryParse`（`NumberStyles.None` + `InvariantCulture`），任一失败回退；保留 `ShouldHighlight` 与配置常量。

- [x] **Step 4: 运行定向测试确认绿灯**

Expected: 全部 `TradeResourceStateRulesTests` 通过。

---

### Task 2: 运行时高亮与补丁挂载

**Files:**
- Add: `src/ResearchAndTradeOptimization/Runtime/TradeResourceStateRuntime.cs`
- Modify: `src/ResearchAndTradeOptimization/Patches/QueuePatches.cs`
- Modify: `src/ResearchAndTradeOptimization/Plugin.cs`
- Modify: `tests/ResearchAndTradeOptimization.Tests/PluginContractTests.cs`
- Modify: `tests/ResearchAndTradeOptimization.Tests/GameContractTests.cs`

**Interfaces:**
- Consumes: `DiplomaticWorldDetailUI._country`/`_importsLayoutUI`/`_exportsLayoutUI`、`DiplomaticWorldDetailResourceLayoutUI._slotsUI`、`DiplomaticWorldDetailResourceSlotUI._tileType`/`_icon`、`DiplomaticCountryData.UsedTradeResource(TileType)`。
- Produces: `ApplyActiveTradeHighlight(DiplomaticWorldDetailUI)`；`Plugin.ActiveTradeHighlightColor`。

- [x] **Step 1: 写失败契约**

`PluginContractTests` 要求 `TradeWorldDetailPatch.Postfix` 调用 `TradeResourceStateRuntime.ApplyActiveTradeHighlight`；`TradeResourceStateRuntime` 无 `Prefix`/`Postfix` 方法；`ApplyActiveTradeHighlight` 调用 `ApplyToLayout` 且含异常处理器；`ApplyToLayout` 调用 `UsedTradeResource`/`ShouldHighlight`/`ShowHighlight`/`HideHighlight`；`ShowHighlight` 调用 `GetOrCreateBackground` 与 `Graphic.set_color`；`GetOrCreateBackground` 调用 `GameObject..ctor`/`Transform.SetParent`/`Transform.SetAsFirstSibling`/`RectTransform` 锚点 setter/`Graphic.set_raycastTarget`。

`GameContractTests` 增加 `_slotsUI`（List<DiplomaticWorldDetailResourceSlotUI>）、`_tileType`（TileType）、`_icon`（Image）字段契约。

- [x] **Step 2: 运行契约测试确认红灯**

Expected: 新运行时/Postfix 调用缺失而失败。

- [x] **Step 3: 实现运行时**

`TradeResourceStateRuntime`：FieldRef 读取 `_country`、两个 layout、`_slotsUI`、`_tileType`、`_icon`；`ApplyActiveTradeHighlight` 空值保护 + try/catch；`ApplyToLayout` 遍历可见槽位，`UsedTradeResource` 判定后 `ShouldHighlight` 分流；`ShowHighlight` 复用 `ConditionalWeakTable` 缓存的背景子物体（铺满锚点、`raycastTarget=false`）并启用 `_icon` 上的 `Outline`；`HideHighlight` 隐藏背景并禁用 `Outline`。

- [x] **Step 4: 挂载 Postfix 并绑定配置**

`TradeWorldDetailPatch.Postfix` 追加 `TradeResourceStateRuntime.ApplyActiveTradeHighlight(__instance)`。`Plugin.Awake` 增加 `BindConfiguration()`（补丁安装前调用），`Config.Bind` 到 `TradeResourceStateRules` 常量，解析失败回退默认并记录一次。

- [x] **Step 5: 运行契约测试确认绿灯**

Expected: 定向测试全部通过；主程序集 0 警告 0 错误。

---

### Task 3: 文档、回归与发布包

**Files:**
- Add: `docs/superpowers/specs/2026-08-17-active-trade-highlight-design.md`
- Modify: `README.md`
- Modify: `docs/TESTING.md`

**Interfaces:**
- Consumes: 已实现的 Release DLL 与已确认规格。
- Produces: 最新中文说明与游戏内验收清单。

- [x] **Step 1: 编写设计规格文档**

记录原版结构、方案、配置、回退与测试覆盖。

- [ ] **Step 2: 更新 README 与验收清单**

README 在贸易队列/商品池节补充"正在贸易的商品以可配置颜色高亮"；TESTING 增加对应验收项（正在贸易商品显示背景与描边、未贸易保持原版、切换国家实时更新、配置生效与非法回退）。

- [ ] **Step 3: 重新打包发布 ZIP**

Run:

```powershell
& .\scripts\Package.ps1 -GameDir $env:RATOPIA_DIR
```

Expected: ZIP 内 DLL 与新建 DLL 哈希一致，仅含插件 DLL 与 README。

---

### Task 4: 备份、安装与交互验收交接

**Files:**
- Backup: `backups/pre-active-trade-highlight-install-<timestamp>/ResearchAndTradeOptimization.dll`
- Backup: `backups/pre-active-trade-highlight-install-<timestamp>/SaveFile/`
- Install: `$env:RATOPIA_DIR\BepInEx\plugins\ResearchAndTradeOptimization\ResearchAndTradeOptimization.dll`

**Interfaces:**
- Consumes: 通过全部门禁的 Release DLL。
- Produces: 已备份、已安装且哈希一致的测试版本；游戏内交互验收由用户启动游戏完成。

- [ ] **Step 1: 只读确认安装门禁**

确认 Ratopia 进程为 0；记录当前安装 DLL 哈希与 SaveFile 文件数/字节数。注意：本机 `Assembly-CSharp.dll` SHA-256 为 `B3C8EC736A5D21A21A83F7CCE6EEAFDCC3F616E91`，与 README 记录 `C94847...` 不一致；字段/方法契约测试全部通过，若确认本机为适配版本，需同步更新 README/契约中的哈希。

- [ ] **Step 2: 备份安装 DLL 和存档**

复制到新时间戳目录并比较哈希。

- [ ] **Step 3: 执行安装脚本**

Run:

```powershell
& .\scripts\Install.ps1 -GameDir $env:RATOPIA_DIR
```

- [ ] **Step 4: 独立验证 DLL 和最终测试**

比较构建/打包/安装三份 DLL 哈希；运行完整测试套件。

- [ ] **Step 5: 明确运行时验收边界**

用户启动游戏：打开贸易中心 → 点击国家 → 详情进出口列表中正在贸易的商品显示 `rgb(145,135,106)` 背景与描边；修改配置颜色重启后生效。
