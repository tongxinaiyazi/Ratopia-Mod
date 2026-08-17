# 普通城市完整商品池与三行布局实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 只为使用非全局本地商品组的普通城市开放完整候选池，并把国家详情商品区域从两行扩展到最多三行；全局公共池城市完整保留原版行为。

**Architecture:** 在纯逻辑层增加“是否允许完整展开”和行数/高度规划规则；Harmony 适配层只读取原版 `IsGlobal`、调用规则并应用 `RectTransform`。原版已有 `VerticalLayoutGroup + ContentSizeFitter`，因此通过增加资源根节点和内容节点高度，让出口区自动排到进口区下方；信息面板保持顶部不变并向下增高，使底部按钮组随面板底锚点整体下移。

**Tech Stack:** C#、.NET Framework 4.7.2、BepInEx 5.4.23.5、Harmony 2.9.0、Unity 2021.3.21f1 UI、xUnit、Mono.Cecil。

## Global Constraints

- 插件名称、GUID 和版本保持 `研究与贸易优化`、`cn.ratopia.unlimitedresearchandtradequeues`、`0.2.0`。
- 目标 `Assembly-CSharp.dll` SHA-256 保持 `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`。
- 不增加配置、自定义存档字段或运行时依赖。
- 不硬编码城市名或 `Exception_A/B/C` 名称，只使用原版 `DiplomaticTradeResourceGroupData.IsGlobal`。
- 运行时异常完整回退原版，不向游戏主循环传播。
- 构建和安装分离；仅在 Ratopia 退出后备份并覆盖安装 DLL。

---

### Task 1: 商品池全局组分流

**Files:**
- Modify: `src/ResearchAndTradeOptimization/Core/FullTradeResourceRules.cs`
- Modify: `src/ResearchAndTradeOptimization/Runtime/FullTradeResourceRuntime.cs`
- Modify: `tests/ResearchAndTradeOptimization.Tests/FullTradeResourceRulesTests.cs`
- Modify: `tests/ResearchAndTradeOptimization.Tests/PluginContractTests.cs`
- Modify: `tests/ResearchAndTradeOptimization.Tests/GameContractTests.cs`

**Interfaces:**
- Consumes: 每个配置桶的 `TradeResourceBucket.IsGlobal`。
- Produces: `FullTradeResourceRules.CanExpandAll(TradeResourceBucket[], TradeResourceBucket[])`；任一全局桶返回 `false`。

- [ ] 在 `TradeResourceBucket` 的反射测试构造参数中增加 `bool isGlobal`，写失败测试：全本地桶允许展开，任一方向含全局桶禁止整个城市展开，组名本身不影响结果。
- [ ] 运行 `dotnet test ... --filter FullyQualifiedName~FullTradeResourceRulesTests`，确认因缺少 `IsGlobal`/`CanExpandAll` 失败。
- [ ] 为桶增加 `IsGlobal`，实现：

```csharp
internal static bool CanExpandAll(
    IEnumerable<TradeResourceBucket> countryToHometown,
    IEnumerable<TradeResourceBucket> hometownToCountry)
{
    return countryToHometown.Concat(hometownToCountry)
        .All(bucket => !bucket.IsGlobal);
}
```

- [ ] `BuildBuckets()` 从原版 `group.IsGlobal` 填充桶；`TryApplyBothDirections()` 在构建两个方向后先调用 `CanExpandAll()`，返回 `false` 时不写任何字段，让 Prefix 执行原版。
- [ ] `RefreshAfterLoad()` 先复用相同资格判断：普通城市调用 `SetTradeResource()` 重建完整本地池；全局池城市不调用，保留 `SetSavableData()` 刚恢复的原版数组。
- [ ] 增加合同测试：`DiplomaticTradeResourceGroupData.IsGlobal` 存在且为 `bool`；运行定向测试确认通过。

### Task 2: 三行布局纯逻辑

**Files:**
- Replace: `src/ResearchAndTradeOptimization/Core/TradeResourcePreviewRules.cs`
- Replace: `tests/ResearchAndTradeOptimization.Tests/TradeResourcePreviewRulesTests.cs`

**Interfaces:**
- Consumes: 单方向实际数量，列数 6，原版可见行数 2，最大行数 3，原版内容高度 140，行跨度 70。
- Produces: `TradeResourceLayoutPlan CreatePlan(int actualCount)`，包含 `VisibleCount`、`VisibleRows`、`AdditionalRows`、`AdditionalHeight`。

- [ ] 写失败理论测试：`0→0 行/0 高度`、`1/6→1 行`、`7/12→2 行`、`13/17/18→3 行/+70`；`19` 只显示前 18 项，作为意外数据的安全上限。
- [ ] 运行定向测试，确认旧规则在 13 项仍截为 12 而失败。
- [ ] 将纯逻辑改为：

```csharp
visible = Math.Min(actualCount, 18);
rows = visible == 0 ? 0 : (visible + 5) / 6;
additionalRows = Math.Max(0, rows - 2);
additionalHeight = additionalRows * 70f;
```

- [ ] 运行定向测试确认通过。

### Task 3: 国家详情运行时三行布局

**Files:**
- Replace: `src/ResearchAndTradeOptimization/Runtime/TradeResourcePreviewRuntime.cs`
- Modify: `src/ResearchAndTradeOptimization/Patches/QueuePatches.cs`
- Modify: `src/ResearchAndTradeOptimization/Plugin.cs`
- Modify: `tests/ResearchAndTradeOptimization.Tests/PluginContractTests.cs`
- Modify: `tests/ResearchAndTradeOptimization.Tests/GameContractTests.cs`

**Interfaces:**
- Consumes: 两次 `DiplomaticWorldDetailResourceLayoutUI.SetData()` 的完整数组和相应布局实例；`DiplomaticWorldDetailUI.Refresh()` 的父详情实例。
- Produces: 最多 18 项参数副本、资源根/内容节点高度，以及详情信息面板的总额外高度。

- [ ] 写失败合同测试：旧 `AppendHiddenCount` 不再存在；`SetData` Prefix 调用 `PrepareThreeRowLayout`，`DiplomaticWorldDetailUI.Refresh` Postfix 在更新 `当前/∞` 后调用 `ApplyDetailPanelHeight`。
- [ ] 写失败游戏合同：`DiplomaticWorldDetailUI` 的 `_importsLayoutUI`、`_exportsLayoutUI`、`_informationPanel` 与资源布局的 `_contents` 字段类型保持不变。
- [ ] 运行定向合同测试确认失败。
- [ ] 运行时使用 `ConditionalWeakTable` 按 Unity 实例保存基线：资源根 `sizeDelta`、内容 `sizeDelta`；详情信息面板 `anchoredPosition` 和 `sizeDelta`。每次调用都从基线绝对赋值。
- [ ] `PrepareThreeRowLayout(ref arr, layout)` 最多复制前 18 项；把布局根高度和 `_contents` 高度分别设为基线 `+ AdditionalHeight`，不修改标题。
- [ ] `ApplyDetailPanelHeight(detail)` 读取两个布局当前额外高度之和；信息面板高度设为基线 `+ total`，中心 y 设为基线 `- total / 2`，保持顶部不动，使底锚定按钮组整体向下移动；最后调用 `LayoutRebuilder.ForceRebuildLayoutImmediate`。
- [ ] 所有入口捕获异常；异常时恢复已有基线并只记录一次类别日志。
- [ ] 更新 Harmony 参数与补丁安装列表，运行定向合同测试确认通过。

### Task 4: 文档、回归、打包与安装

**Files:**
- Modify: `README.md`
- Modify: `docs/TESTING.md`
- Regenerate: `dist/研究与贸易优化-v0.2.0-BepInEx5.zip`

**Interfaces:**
- Consumes: 修复后的 Release DLL。
- Produces: 最新发布包和安装 DLL；构建、ZIP、安装三份 DLL 哈希一致。

- [ ] README 明确：普通本地池城市完整开放；全局公共池城市暂时保持原版；国家详情最多三行。
- [ ] 人工验收清单加入普通城市 13–17 项、三个市场城市保持约 8–10 项/方向、换季和读档分流。
- [ ] 执行 `scripts/Package.ps1`；要求所有 Release 测试通过、0 警告、0 错误，ZIP 仅含 DLL 与 README。
- [ ] 再次确认 Ratopia 进程不存在；备份当前安装 DLL 和最新存档。
- [ ] 执行 `scripts/Install.ps1`；校验构建、ZIP 内和安装 DLL SHA-256 一致。
- [ ] 记录未做游戏内交互验收，交由用户检查三个特殊市场城市与普通三行布局。
