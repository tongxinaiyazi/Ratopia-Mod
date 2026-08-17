# 贸易方向与无限期修复实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 修复共享商品组被同时复制到进口和出口，以及普通贸易无法选择无限期的问题。

**Architecture:** 用纯逻辑分配器一次性联合计算国家两个贸易方向，再由 `SetTradeResource` 前缀原子写回四个原版数组/列表；普通期限控件仅扩大原版下限到 0。保留原版存档字段、协议状态和界面文本。

**Tech Stack:** C#、.NET Framework 4.7.2、BepInEx 5.4.23.5、Harmony 2.9.0、xUnit、Mono.Cecil。

## Global Constraints

- GUID 保持 `cn.ratopia.unlimitedresearchandtradequeues`，版本保持 `0.2.0`。
- 适配 `Assembly-CSharp.dll` SHA-256 `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`。
- 不增加配置、自定义存档字段或运行时依赖。
- Ratopia 运行时不得覆盖安装 DLL；安装前备份当前 DLL。
- 运行时联合构建异常必须完整回退原版 `SetTradeResource`。

---

### Task 1: 共享商品组联合分配器

**Files:**
- Modify: `src/ResearchAndTradeOptimization/Core/FullTradeResourceRules.cs`
- Modify: `tests/ResearchAndTradeOptimization.Tests/FullTradeResourceRulesTests.cs`

**Interfaces:**
- Consumes: 两方向的繁荣等级、资源组标识、`PickCount` 权重和资源数组。
- Produces: `BuildBothDirections(TradeResourceBucket[] countryToHometown, TradeResourceBucket[] hometownToCountry, int[] ignoredResources)`，返回两个互不相交的稳定结果。

- [ ] 写失败测试：共享 `Exception_A` 组按两方向权重分配，所有资源恰好出现一次且无交集。
- [ ] 写失败测试：独立组保持原方向；跨组重复商品按最低繁荣等级稳定归属；忽略项被排除。
- [ ] 运行定向测试并确认因缺少联合 API 失败。
- [ ] 实现按组的确定性加权轮转分配；相同组的多个繁荣桶分别保留权重和等级。
- [ ] 运行定向测试并确认通过。

### Task 2: 原子替换城市完整贸易池

**Files:**
- Modify: `src/ResearchAndTradeOptimization/Runtime/FullTradeResourceRuntime.cs`
- Modify: `src/ResearchAndTradeOptimization/Patches/QueuePatches.cs`
- Modify: `src/ResearchAndTradeOptimization/Plugin.cs`
- Modify: `tests/ResearchAndTradeOptimization.Tests/PluginContractTests.cs`
- Modify: `tests/ResearchAndTradeOptimization.Tests/GameContractTests.cs`

**Interfaces:**
- Consumes: `DiplomaticCountryData.Raw` 的两方向原始数组和 `DiplomaticTradeResourceGroupAsset`。
- Produces: `TryApplyBothDirections(DiplomaticCountryData, DiplomaticAsset)`；成功时更新 `_countryToHometownArray`、`_hometownToCountryArray`、两个 `_all...List`，再调用原版 `CalculateTradable...`。

- [ ] 写失败合同测试，要求补丁目标为 `DiplomaticCountryData.SetTradeResource`，不再补丁私有 `PickUpTradeResources`。
- [ ] 写失败源码合同测试，要求联合运行时同时处理两个方向并验证交集为空。
- [ ] 运行定向测试确认失败。
- [ ] 将补丁改为 `SetTradeResource` Prefix；只有四个字段全部成功构造后才写回并跳过原版，否则执行原版。
- [ ] 读档 Postfix 继续调用 `SetTradeResource`，季节刷新自然复用同一路径。
- [ ] 运行定向测试确认通过。

### Task 3: 普通贸易所有入口启用无限期

**Files:**
- Modify: `src/ResearchAndTradeOptimization/Runtime/TradeAgreementEditRuntime.cs`
- Modify: `tests/ResearchAndTradeOptimization.Tests/TradeAgreementRulesTests.cs`
- Modify: `tests/ResearchAndTradeOptimization.Tests/PluginContractTests.cs`
- Modify: `tests/ResearchAndTradeOptimization.Tests/GameContractTests.cs`

**Interfaces:**
- Consumes: `DiplomaticTradeSheetDetailSlotUI.SetData(..., TypeTradeSheet, bool)` Postfix 参数。
- Produces: 普通 `TypeTradeSheet.Period` 始终设置 `_minValue = 0`；只在调整会话时额外解锁已有协议的控件。

- [ ] 写失败测试/合同：非调整会话的普通 `Period` 也允许 0，特殊期限不受影响。
- [ ] 运行定向测试确认失败。
- [ ] 把期限下限修改移到会话守卫之前；按钮解锁和最大值扩展仍只作用于调整会话。
- [ ] 运行定向测试确认通过。

### Task 4: 回归、打包与安装

**Files:**
- Modify: `README.md`
- Modify: `docs/TESTING.md`
- Regenerate: `dist/研究与贸易优化-v0.2.0-BepInEx5.zip`

**Interfaces:**
- Consumes: 修复后的 Release DLL。
- Produces: 最新发布包与已安装 DLL，二者哈希一致。

- [ ] 更新 README 和人工验收清单，明确进口/出口不重复与三类普通贸易入口的无限期。
- [ ] 运行全部 Release 测试，要求零失败。
- [ ] 运行 Release 构建，要求零警告、零错误且 `InstallAfterBuild=false`。
- [ ] 重新打包并验证 ZIP 仅含 DLL 与 README。
- [ ] 确认 Ratopia 已退出，备份当前安装 DLL后安装。
- [ ] 比较构建、ZIP 内和安装 DLL 的 SHA-256。

### Task 4: 限制国家详情商品预览高度

**Files:**
- Create: `src/ResearchAndTradeOptimization/Core/TradeResourcePreviewRules.cs`
- Create: `src/ResearchAndTradeOptimization/Runtime/TradeResourcePreviewRuntime.cs`
- Modify: `src/ResearchAndTradeOptimization/Patches/QueuePatches.cs`
- Modify: `src/ResearchAndTradeOptimization/Plugin.cs`
- Create: `tests/ResearchAndTradeOptimization.Tests/TradeResourcePreviewRulesTests.cs`
- Modify: `tests/ResearchAndTradeOptimization.Tests/PluginContractTests.cs`

**Interfaces:**
- Consumes: 国家详情某一方向的完整 `KeyValuePair<int, TileType>[]`。
- Produces: 最多 12 项的预览数组和准确的隐藏数量标题；不修改原国家数组。

- [ ] 写失败测试：0、12、13、59 项分别得到 `(0,0)`、`(12,0)`、`(12,1)`、`(12,47)`。
- [ ] 写失败合同测试：`DiplomaticWorldDetailResourceLayoutUI.SetData` 补丁调用预览运行时。
- [ ] 运行定向测试确认失败。
- [ ] 实现预览拷贝与标题 `... (+N)`，只改变方法参数的副本。
- [ ] 运行定向测试确认通过。

### Task 5: 回归、打包与安装
