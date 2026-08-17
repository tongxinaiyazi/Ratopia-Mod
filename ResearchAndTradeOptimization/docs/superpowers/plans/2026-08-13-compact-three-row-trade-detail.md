# 国家详情三行商品紧凑布局实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 把普通城市的三行进口/出口商品压缩进原版固定高度，确保“新增贸易”按钮不再被推出屏幕。

**Architecture:** 纯逻辑层统一决定整张国家详情是否进入紧凑模式，并计算最多 18 项及三行内容高度。Harmony 在 `DiplomaticWorldDetailUI.Refresh` Prefix 中、原版填充两个方向之前统一设置两组 `GridLayoutGroup`；`SetData` Prefix 只截取意外超过 18 项的参数副本。运行时缓存每个 Unity 布局实例的原版网格和 `RectTransform` 基线，每次从基线绝对应用或恢复，不再修改父信息面板。

**Tech Stack:** C#、.NET Framework 4.7.2、BepInEx 5.4.23.5、Harmony 2.9.0、Unity 2021.3.21f1 UI、xUnit、Mono.Cecil。

## Global Constraints

- 插件名称、GUID 和版本保持 `研究与贸易优化`、`cn.ratopia.unlimitedresearchandtradequeues`、`0.2.0`。
- 目标 `Assembly-CSharp.dll` SHA-256 保持 `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`。
- 紧凑模式固定使用 `52×52` 商品格、`2×2` 间距、每行 6 项、最多 3 行和最多 18 项。
- 紧凑模式固定 `GridLayoutGroup.Constraint.FixedColumnCount` 与 `constraintCount = 6`，标准模式恢复原版约束。
- 紧凑模式由两个方向共同决定：任一方向超过 12 项时，两边统一紧凑。
- 不改变进口/出口根区域、父信息面板、城市文字、方向标题或“新增贸易”按钮尺寸与位置。
- 不修改商品池、贸易协议、价格或存档数据；不增加配置、自定义存档字段或运行时依赖。
- 运行时异常恢复已缓存原版基线，不向游戏主循环传播，同类错误只记录一次。
- 构建和安装分离；仅在 Ratopia 退出后备份并覆盖安装 DLL。
- 当前目录不是 Git 仓库，因此本计划不包含不可执行的提交步骤；所有验证依靠测试、构建、发布包和文件哈希。

---

### Task 1: 紧凑网格纯逻辑

**Files:**
- Modify: `src/ResearchAndTradeOptimization/Core/TradeResourcePreviewRules.cs`
- Modify: `tests/ResearchAndTradeOptimization.Tests/TradeResourcePreviewRulesTests.cs`

**Interfaces:**
- Consumes: `importCount`、`exportCount`、原版内容上内边距 `10`。
- Produces: `TradeResourceDetailLayoutPlan CreateDetailPlan(int importCount, int exportCount, int topPadding)`，包含 `UseCompactGrid`、`CellWidth`、`CellHeight`、`HorizontalSpacing`、`VerticalSpacing`、`ContentHeight`；保留 `CreatePlan(int actualCount)` 产生 `VisibleCount` 与 `VisibleRows`。

- [ ] **Step 1: 用新契约替换旧的额外高度测试**

在 `TradeResourcePreviewRulesTests.cs` 中保留可见数量边界，但删除 `AdditionalRows`/`AdditionalHeight` 断言，新增反射辅助调用 `CreateDetailPlan`：

```csharp
[Theory]
[InlineData(12, 12, false)]
[InlineData(13, 1, true)]
[InlineData(1, 13, true)]
[InlineData(18, 18, true)]
public void EitherThirdRowUsesOneCompactModeForBothDirections(
    int importCount,
    int exportCount,
    bool expectedCompact)
{
    var result = InvokePlan("CreateDetailPlan", importCount, exportCount, 10);
    Assert.Equal(expectedCompact, Read<bool>(result, "UseCompactGrid"));
}

[Fact]
public void CompactThreeRowsFitInsideTheNativeDirectionPanel()
{
    var result = InvokePlan("CreateDetailPlan", 14, 14, 10);
    Assert.Equal(52f, Read<float>(result, "CellWidth"));
    Assert.Equal(52f, Read<float>(result, "CellHeight"));
    Assert.Equal(2f, Read<float>(result, "HorizontalSpacing"));
    Assert.Equal(2f, Read<float>(result, "VerticalSpacing"));
    Assert.Equal(170f, Read<float>(result, "ContentHeight"));
    Assert.Equal(6, Read<int>(result, "Columns"));
}
```

另保留 `19 → VisibleCount 18`，证明安全上限没有丢失。

- [ ] **Step 2: 运行定向测试并确认红灯原因正确**

Run:

```powershell
dotnet test tests\ResearchAndTradeOptimization.Tests\ResearchAndTradeOptimization.Tests.csproj `
  -c Release --no-restore `
  --filter FullyQualifiedName~TradeResourcePreviewRulesTests `
  -p:InstallAfterBuild=false `
  -p:RatopiaDir='E:\steam\steamapps\common\Ratopia'
```

Expected: 因缺少 `CreateDetailPlan`/`TradeResourceDetailLayoutPlan`，或旧计划仍返回 `AdditionalHeight = 70` 而失败。

- [ ] **Step 3: 实现最小纯逻辑计划**

在 `TradeResourcePreviewRules.cs` 中把预览计划缩减为可见数量/行数，并增加：

```csharp
internal readonly struct TradeResourceDetailLayoutPlan
{
    internal TradeResourceDetailLayoutPlan(bool useCompactGrid, float contentHeight)
    {
        UseCompactGrid = useCompactGrid;
        CellWidth = useCompactGrid ? 52f : 0f;
        CellHeight = useCompactGrid ? 52f : 0f;
        HorizontalSpacing = useCompactGrid ? 2f : 0f;
        VerticalSpacing = useCompactGrid ? 2f : 0f;
        ContentHeight = useCompactGrid ? contentHeight : 0f;
    }

    public bool UseCompactGrid { get; }
    public float CellWidth { get; }
    public float CellHeight { get; }
    public float HorizontalSpacing { get; }
    public float VerticalSpacing { get; }
    public float ContentHeight { get; }
    public int Columns => 6;
}

internal static TradeResourceDetailLayoutPlan CreateDetailPlan(
    int importCount,
    int exportCount,
    int topPadding)
{
    if (importCount < 0) throw new ArgumentOutOfRangeException(nameof(importCount));
    if (exportCount < 0) throw new ArgumentOutOfRangeException(nameof(exportCount));
    if (topPadding < 0) throw new ArgumentOutOfRangeException(nameof(topPadding));

    var compact = importCount > 12 || exportCount > 12;
    var height = topPadding + 3 * 52f + 2 * 2f;
    return new TradeResourceDetailLayoutPlan(compact, height);
}
```

- [ ] **Step 4: 运行定向测试确认绿灯**

Run: 与 Step 2 相同。

Expected: 全部 `TradeResourcePreviewRulesTests` 通过，0 失败。

---

### Task 2: 在原版固定高度内应用和恢复紧凑网格

**Files:**
- Replace: `src/ResearchAndTradeOptimization/Runtime/TradeResourcePreviewRuntime.cs`
- Modify: `src/ResearchAndTradeOptimization/Patches/QueuePatches.cs`
- Modify: `tests/ResearchAndTradeOptimization.Tests/PluginContractTests.cs`
- Modify: `tests/ResearchAndTradeOptimization.Tests/GameContractTests.cs`

**Interfaces:**
- Consumes: `TradeResourcePreviewRules.CreateDetailPlan()`、`DiplomaticWorldDetailUI._country`、`_importsLayoutUI`、`_exportsLayoutUI`、每个资源布局 `_contents` 及其 `GridLayoutGroup`。
- Produces: `ApplyCompactDetailLayout(DiplomaticWorldDetailUI detail)`、`LimitVisibleItems(ref KeyValuePair<int, TileType>[] resources)`。

- [ ] **Step 1: 写 Harmony 和布局行为失败合同**

更新 `PluginContractTests`，要求：

```csharp
AssertCalls(
    FindMethod(module, "ResearchAndTradeOptimization.Patches.TradeWorldDetailPatch", "Prefix"),
    "ResearchAndTradeOptimization.Runtime.TradeResourcePreviewRuntime",
    "ApplyCompactDetailLayout");
AssertCalls(
    FindMethod(module, "ResearchAndTradeOptimization.Patches.TradeWorldDetailPatch", "Postfix"),
    "ResearchAndTradeOptimization.Runtime.TradeQueueRuntime",
    "UpdateWorldDetailLabel");
AssertCalls(
    FindMethod(module, "ResearchAndTradeOptimization.Patches.TradeResourcePreviewPatch", "Prefix"),
    "ResearchAndTradeOptimization.Runtime.TradeResourcePreviewRuntime",
    "LimitVisibleItems");
```

对 `ApplyCompactDetailLayout` 断言调用：

- `TradeResourcePreviewRules.CreateDetailPlan`；
- `UnityEngine.UI.GridLayoutGroup.set_cellSize`；
- `UnityEngine.UI.GridLayoutGroup.set_spacing`；
- `UnityEngine.UI.GridLayoutGroup.set_constraint`；
- `UnityEngine.UI.GridLayoutGroup.set_constraintCount`；
- `UnityEngine.RectTransform.set_sizeDelta`；
- `UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate`。

同时断言：

```csharp
Assert.DoesNotContain(runtime.Methods, method => method.Name == "ApplyDetailPanelHeight");
Assert.DoesNotContain(apply.Body.Instructions, instruction =>
    instruction.Operand is MethodReference called &&
    called.DeclaringType.FullName == "UnityEngine.RectTransform" &&
    called.Name == "set_anchoredPosition");
```

在 `GameContractTests` 保留 `_country`、`_importsLayoutUI`、`_exportsLayoutUI` 和 `_contents` 字段契约；不再把 `_informationPanel` 作为本功能的运行时依赖。新增原版商品 `SetData` 目标方法的精确签名合同（如果现有综合合同尚未覆盖）。

- [ ] **Step 2: 运行合同测试并确认旧扩高实现红灯**

Run:

```powershell
dotnet test tests\ResearchAndTradeOptimization.Tests\ResearchAndTradeOptimization.Tests.csproj `
  -c Release --no-restore `
  --filter "FullyQualifiedName~PluginContractTests|FullyQualifiedName~GameContractTests" `
  -p:InstallAfterBuild=false `
  -p:RatopiaDir='E:\steam\steamapps\common\Ratopia'
```

Expected: 旧 Postfix 仍调用 `ApplyDetailPanelHeight`，缺少 `ApplyCompactDetailLayout`/`LimitVisibleItems`，合同失败。

- [ ] **Step 3: 把运行时状态改为商品网格基线**

`ResourceLayoutState` 保存：

```csharp
internal ResourceLayoutState(
    RectTransform root,
    RectTransform contents,
    GridLayoutGroup grid)
{
    Root = root;
    Contents = contents;
    Grid = grid;
    RootSize = root.sizeDelta;
    ContentsSize = contents.sizeDelta;
    CellSize = grid.cellSize;
    Spacing = grid.spacing;
    Constraint = grid.constraint;
    ConstraintCount = grid.constraintCount;
}
```

删除 `DetailPanelState`、`InformationPanel` FieldRef、`DetailPanels`、`AdditionalHeight` 和父面板恢复逻辑。新增 `DiplomaticWorldDetailUI._country` FieldRef；已有两个方向 FieldRef 保留。

- [ ] **Step 4: 在 Refresh Prefix 统一应用两边布局**

实现：

```csharp
internal static void ApplyCompactDetailLayout(DiplomaticWorldDetailUI detail)
{
    try
    {
        if (detail == null) return;
        var imports = ImportsLayout(detail);
        var exports = ExportsLayout(detail);
        var country = Country(detail);
        var importCount = country?.CountryToHometownArray?.Length ?? 0;
        var exportCount = country?.HometownToCountryArray?.Length ?? 0;
        var first = GetOrCreateResourceLayout(imports);
        var second = GetOrCreateResourceLayout(exports);
        var topPadding = Math.Max(first.Grid.padding.top, second.Grid.padding.top);
        var plan = TradeResourcePreviewRules.CreateDetailPlan(
            importCount,
            exportCount,
            topPadding);

        Apply(first, plan);
        Apply(second, plan);
    }
    catch (Exception exception)
    {
        RestoreResourceLayout(ImportsLayout(detail));
        RestoreResourceLayout(ExportsLayout(detail));
        LogLayoutFailureOnce(exception);
    }
}
```

`Apply` 必须始终先把根大小恢复为 `RootSize`。紧凑时把 `cellSize` 设为 `(52,52)`、`spacing` 设为 `(2,2)`、`constraint` 设为 `FixedColumnCount`、`constraintCount` 设为 `6`，内容区只改 `sizeDelta.y = 170`；标准时恢复全部基线。每次完成后重建根布局。不得设置父信息面板或任何 `anchoredPosition`。

- [ ] **Step 5: 让 SetData Prefix 只处理 18 项安全上限**

实现：

```csharp
internal static void LimitVisibleItems(
    ref KeyValuePair<int, TileType>[] resources)
{
    resources = resources ?? Array.Empty<KeyValuePair<int, TileType>>();
    var plan = TradeResourcePreviewRules.CreatePlan(resources.Length);
    if (resources.Length <= plan.VisibleCount) return;

    var visible = new KeyValuePair<int, TileType>[plan.VisibleCount];
    Array.Copy(resources, visible, visible.Length);
    resources = visible;
}
```

异常时保留原参数并只记录一次。这里不再取得布局实例或设置尺寸。

- [ ] **Step 6: 调整 Harmony 适配器**

```csharp
[HarmonyPatch(typeof(DiplomaticWorldDetailUI), "Refresh")]
internal static class TradeWorldDetailPatch
{
    internal static void Prefix(DiplomaticWorldDetailUI __instance)
    {
        TradeResourcePreviewRuntime.ApplyCompactDetailLayout(__instance);
    }

    internal static void Postfix(DiplomaticWorldDetailUI __instance)
    {
        TradeQueueRuntime.UpdateWorldDetailLabel(__instance);
    }
}

[HarmonyPatch(typeof(DiplomaticWorldDetailResourceLayoutUI), "SetData")]
internal static class TradeResourcePreviewPatch
{
    internal static void Prefix(ref KeyValuePair<int, TileType>[] arr)
    {
        TradeResourcePreviewRuntime.LimitVisibleItems(ref arr);
    }
}
```

- [ ] **Step 7: 运行定向规则和合同测试确认绿灯**

Run:

```powershell
dotnet test tests\ResearchAndTradeOptimization.Tests\ResearchAndTradeOptimization.Tests.csproj `
  -c Release --no-restore `
  --filter "FullyQualifiedName~TradeResourcePreviewRulesTests|FullyQualifiedName~PluginContractTests|FullyQualifiedName~GameContractTests" `
  -p:InstallAfterBuild=false `
  -p:RatopiaDir='E:\steam\steamapps\common\Ratopia'
```

Expected: 所有定向测试通过，0 失败；新运行时不存在 `ApplyDetailPanelHeight`。

---

### Task 3: 文档、回归和发布包

**Files:**
- Modify: `README.md`
- Modify: `docs/TESTING.md`
- Modify: `tests/ResearchAndTradeOptimization.Tests/ReleaseContractTests.cs`
- Regenerate: `dist/研究与贸易优化-v0.2.0-BepInEx5.zip`

**Interfaces:**
- Consumes: 修复后的 Release DLL 和已确认规格。
- Produces: 最新中文说明、验收清单与只含插件 DLL/README 的发布包。

- [ ] **Step 1: 先写失败的发布说明合同**

在 `ReleaseContractTests` 中要求 README/验收清单包含“紧凑商品格”“52×52”“新增贸易”和“保持原位”，并不再包含“下方方向和贸易控件会按原版布局整体下移”。

- [ ] **Step 2: 运行发布合同并确认旧说明红灯**

Run:

```powershell
dotnet test tests\ResearchAndTradeOptimization.Tests\ResearchAndTradeOptimization.Tests.csproj `
  -c Release --no-restore `
  --filter FullyQualifiedName~ReleaseContractTests `
  -p:InstallAfterBuild=false `
  -p:RatopiaDir='E:\steam\steamapps\common\Ratopia'
```

Expected: README/TESTING 尚未描述紧凑模式而失败；旧 ZIP 与新构建 DLL 不一致也可作为预期红灯之一。

- [ ] **Step 3: 更新 README 和游戏内验收清单**

README 明确：任一方向超过 12 项时两个方向统一使用 `52×52` 紧凑商品格，最多三行/18 项，父面板与“新增贸易”保持原位。

TESTING 增加：

- 索尔德姆两个方向 14 项时按钮完整可见；
- 单边第三行与双边第三行；
- 紧凑城市 ↔ 标准城市连续切换无缩放累积；
- 18 个图标的悬浮说明和点击仍正常；
- 键盘可聚焦并触发“新增贸易”。

- [ ] **Step 4: 执行完整 Release 打包门禁**

Run:

```powershell
& .\scripts\Package.ps1 -GameDir 'E:\steam\steamapps\common\Ratopia'
```

Expected: 全部测试通过；构建 0 警告、0 错误；ZIP 内容一致性测试通过；ZIP 仅含：

```text
BepInEx/plugins/ResearchAndTradeOptimization/ResearchAndTradeOptimization.dll
README.md
```

---

### Task 4: 备份、安装与交互验收交接

**Files:**
- Backup: `backups/pre-compact-trade-detail-install-<timestamp>/ResearchAndTradeOptimization.dll`
- Backup: `backups/pre-compact-trade-detail-install-<timestamp>/SaveFile/`
- Install: `E:\steam\steamapps\common\Ratopia\BepInEx\plugins\ResearchAndTradeOptimization\ResearchAndTradeOptimization.dll`

**Interfaces:**
- Consumes: Task 3 通过所有门禁的 Release DLL。
- Produces: 已备份、已安装且哈希一致的测试版本；游戏内交互验收由用户启动游戏完成。

- [ ] **Step 1: 只读确认安装门禁**

确认 Ratopia 进程为 0；检查旧 `UnlimitedResearchAndTradeQueues` 目录不存在；记录当前安装 DLL 哈希和 SaveFile 文件数/字节数。若游戏仍运行，停止安装，不覆盖 DLL。

- [ ] **Step 2: 备份安装 DLL 和存档**

复制到新时间戳目录，并再次比较备份 DLL 哈希、SaveFile 文件数和总字节数。任何一项不一致都停止安装。

- [ ] **Step 3: 执行安装脚本**

Run:

```powershell
& .\scripts\Install.ps1 -GameDir 'E:\steam\steamapps\common\Ratopia'
```

Expected: 安装成功且脚本报告源/目标 DLL 哈希一致。

- [ ] **Step 4: 独立验证三份 DLL 和最终测试**

比较构建 DLL、ZIP 内 DLL、安装 DLL 的 SHA-256，要求完全相同；随后执行：

```powershell
dotnet test tests\ResearchAndTradeOptimization.Tests\ResearchAndTradeOptimization.Tests.csproj `
  -c Release --no-build --no-restore `
  -p:InstallAfterBuild=false `
  -p:RatopiaDir='E:\steam\steamapps\common\Ratopia'
```

Expected: 全部测试通过，Ratopia 进程仍为 0。

- [ ] **Step 5: 明确运行时验收边界**

交接给用户启动游戏，重点复测索尔德姆截图场景。“插件加载/补丁安装/首次紧凑布局”日志只是运行证据，只有按钮完整可见、可操作且连续切换不漂移才算行为验收完成。
