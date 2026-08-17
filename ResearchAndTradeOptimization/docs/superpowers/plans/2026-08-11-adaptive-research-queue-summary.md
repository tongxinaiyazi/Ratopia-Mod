# 研究队列自适应单行摘要 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Ratopia Mod 工作必须由主代理顺序执行，禁止使用子代理。

**Goal:** 把无限研究队列改为自适应单行摘要：尽量向右显示真实研究，溢出时最后一格显示不可点击的 `...`，不再产生第二行重叠。

**Architecture:** 新增纯逻辑 `ResearchQueueLayoutRules` 计算水平间距、可见容量、真实节点数和内容宽度；新增 `ResearchQueueLayoutRuntime` 把 Unity `RectTransform`/视口转换为纯布局输入，并在 `ResearchingGroupSet()` Postfix 中应用布局。原版刷新循环会按完整队列长度索引 `Arr_Technode`，因此 Prefix 仍准备完整节点数组；Postfix 只激活单行可见节点，独立省略号节点不进入原版数组。

**Tech Stack:** C#、.NET Framework 4.7.2、BepInEx 5.4.23.5、Harmony 2.9.0、Unity Mono、xUnit 2.9.2、Mono.Cecil。

## Global Constraints

- 游戏目录：`E:\steam\steamapps\common\Ratopia`。
- 适配 `Assembly-CSharp.dll` SHA-256：`C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`。
- 插件 GUID、名称和版本保持 `cn.ratopia.unlimitedresearchandtradequeues`、`贸易站和研究去除最大队列限制`、`0.1.0`。
- 不修改研究数据、推进顺序、存档格式、贸易补丁或 `MaxTradeAgreementCount` getter。
- `...` 不可点击；隐藏项继续推进，但隐藏期间不能从当前队列界面直接取消。
- 所有游戏、Unity、BepInEx、Harmony 引用继续使用 `Private="false"`。
- 测试和构建必须使用 `/p:InstallAfterBuild=false`。
- Ratopia 运行时不得覆盖 DLL；覆盖前备份现有 DLL 和完整 `Ratopia_Data\SaveFile`。
- 本目录不是 Git 仓库；计划中的检查点使用测试输出和哈希代替 Git commit，不创建虚假提交。

---

## File Structure

- Create `src/UnlimitedResearchAndTradeQueues/Core/ResearchQueueLayoutRules.cs`: 纯布局输入/输出和值计算，不引用 Unity。
- Create `src/UnlimitedResearchAndTradeQueues/Runtime/ResearchQueueLayoutRuntime.cs`: 视口解析、单行定位、内容宽度和省略号生命周期。
- Modify `src/UnlimitedResearchAndTradeQueues/Runtime/ResearchQueueRuntime.cs`: 在视口可用时继续为原版刷新循环准备完整节点数组，并暴露当前队列数量给布局适配器。
- Modify `src/UnlimitedResearchAndTradeQueues/Patches/QueuePatches.cs`: 给 `ResearchQueueViewPatch` 增加 Postfix。
- Create `tests/UnlimitedResearchAndTradeQueues.Tests/ResearchQueueLayoutRulesTests.cs`: 纯容量、位置、溢出和内容宽度测试。
- Modify `tests/UnlimitedResearchAndTradeQueues.Tests/PluginContractTests.cs`: Postfix、视口解析、非交互省略号和数组增长上限静态合同。
- Modify `README.md`: 说明单行自适应和 `...` 行为。

---

### Task 1: 纯布局规则

**Files:**
- Create: `src/UnlimitedResearchAndTradeQueues/Core/ResearchQueueLayoutRules.cs`
- Create: `tests/UnlimitedResearchAndTradeQueues.Tests/ResearchQueueLayoutRulesTests.cs`

**Interfaces:**
- Consumes: 现有 `UnlimitedResearchAndTradeQueues.Core.NodePosition`。
- Produces:
  - `ResearchQueueDisplayPlan(int visibleResearchCount, int displayedSlotCount, bool showOverflow)`
  - `ResearchQueueLayoutRules.GetHorizontalStep(float firstX, float secondX, float firstWidth): float`
  - `ResearchQueueLayoutRules.GetSlotCapacity(float firstCardRight, float viewportRight, float horizontalStep): int`
  - `ResearchQueueLayoutRules.CreateDisplayPlan(int queueCount, int slotCapacity): ResearchQueueDisplayPlan`
  - `ResearchQueueLayoutRules.GetRowPosition(NodePosition first, float horizontalStep, int index): NodePosition`
  - `ResearchQueueLayoutRules.GetContentWidth(int displayedSlotCount, float cardWidth, float horizontalStep): float`

- [ ] **Step 1: Write failing pure rule tests**

Create `ResearchQueueLayoutRulesTests.cs` with reflection helpers matching the existing internal-type testing style:

```csharp
using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace UnlimitedResearchAndTradeQueues.Tests
{
    public sealed class ResearchQueueLayoutRulesTests
    {
        [Theory]
        [InlineData(20f, 180f, 150f, 160f)]
        [InlineData(20f, 20f, 150f, 150f)]
        [InlineData(20f, 20.001f, 80f, 100f)]
        public void HorizontalStepUsesOriginalFirstRowOrSafeFallback(
            float firstX, float secondX, float width, float expected)
        {
            Assert.Equal(expected, Invoke<float>("GetHorizontalStep", firstX, secondX, width));
        }

        [Theory]
        [InlineData(170f, 970f, 160f, 6)]
        [InlineData(170f, 649f, 160f, 3)]
        [InlineData(170f, 169f, 160f, 0)]
        public void SlotCapacityCountsOnlyFullyVisibleCards(
            float firstRight, float viewportRight, float step, int expected)
        {
            Assert.Equal(expected, Invoke<int>("GetSlotCapacity", firstRight, viewportRight, step));
        }

        [Theory]
        [InlineData(6, 6, 6, 6, false)]
        [InlineData(7, 6, 5, 6, true)]
        [InlineData(20, 6, 5, 6, true)]
        public void DisplayPlanReservesTheLastSlotForOverflow(
            int queueCount, int capacity, int visibleResearch, int displayedSlots, bool overflow)
        {
            var plan = Invoke<object>("CreateDisplayPlan", queueCount, capacity);
            Assert.Equal(visibleResearch, Read<int>(plan, "VisibleResearchCount"));
            Assert.Equal(displayedSlots, Read<int>(plan, "DisplayedSlotCount"));
            Assert.Equal(overflow, Read<bool>(plan, "ShowOverflow"));
        }

        [Fact]
        public void EveryPositionStaysOnTheFirstRow()
        {
            var result = InvokePoint(20f, 80f, 160f, 5);
            Assert.Equal(820f, result.x);
            Assert.Equal(80f, result.y);
        }

        [Theory]
        [InlineData(20f, 0f, 1000f, 980f)]
        [InlineData(-10f, 0f, 1000f, 1000f)]
        public void CanvasFallbackPreservesTheSafeRightMargin(
            float firstCardLeft,
            float canvasLeft,
            float canvasRight,
            float expected)
        {
            Assert.Equal(
                expected,
                Invoke<float>(
                    "GetCanvasFallbackRight",
                    firstCardLeft,
                    canvasLeft,
                    canvasRight));
        }

        [Theory]
        [InlineData(0, 150f, 160f, 20f)]
        [InlineData(1, 150f, 160f, 170f)]
        [InlineData(6, 150f, 160f, 970f)]
        public void ContentWidthTracksOnlyDisplayedSlots(
            int slots, float width, float step, float expected)
        {
            Assert.Equal(expected, Invoke<float>("GetContentWidth", slots, width, step));
        }

        private static T Invoke<T>(string name, params object[] args)
        {
            var type = Load().GetType(
                "UnlimitedResearchAndTradeQueues.Core.ResearchQueueLayoutRules", false);
            Assert.NotNull(type);
            var method = type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (T)method.Invoke(null, args);
        }

        private static T Read<T>(object value, string name)
        {
            return (T)value.GetType().GetProperty(name).GetValue(value);
        }

        private static (float x, float y) InvokePoint(
            float firstX, float firstY, float step, int index)
        {
            var assembly = Load();
            var pointType = assembly.GetType("UnlimitedResearchAndTradeQueues.Core.NodePosition", true);
            var rules = assembly.GetType(
                "UnlimitedResearchAndTradeQueues.Core.ResearchQueueLayoutRules", true);
            var point = Activator.CreateInstance(
                pointType,
                new object[] { firstX, firstY });
            var method = rules.GetMethod(
                "GetRowPosition", BindingFlags.Static | BindingFlags.NonPublic);
            var result = method.Invoke(null, new[] { point, (object)step, index });
            return (
                (float)pointType.GetProperty("X").GetValue(result),
                (float)pointType.GetProperty("Y").GetValue(result));
        }

        private static Assembly Load()
        {
            return Assembly.LoadFrom(Path.Combine(
                AppContext.BaseDirectory, "UnlimitedResearchAndTradeQueues.dll"));
        }
    }
}
```

- [ ] **Step 2: Run the tests and verify RED**

Run:

```powershell
$env:RATOPIA_DIR='E:\steam\steamapps\common\Ratopia'
dotnet test .\UnlimitedResearchAndTradeQueues.sln -c Debug `
  /p:InstallAfterBuild=false `
  --filter 'FullyQualifiedName~ResearchQueueLayoutRulesTests' --nologo
```

Expected: tests fail because `ResearchQueueLayoutRules` does not exist.

- [ ] **Step 3: Implement the minimal pure rules**

Create `ResearchQueueLayoutRules.cs`:

```csharp
using System;

namespace UnlimitedResearchAndTradeQueues.Core
{
    internal readonly struct ResearchQueueDisplayPlan
    {
        internal ResearchQueueDisplayPlan(
            int visibleResearchCount,
            int displayedSlotCount,
            bool showOverflow)
        {
            VisibleResearchCount = visibleResearchCount;
            DisplayedSlotCount = displayedSlotCount;
            ShowOverflow = showOverflow;
        }

        public int VisibleResearchCount { get; }
        public int DisplayedSlotCount { get; }
        public bool ShowOverflow { get; }
    }

    internal static class ResearchQueueLayoutRules
    {
        internal const int MinimumSummarySlotCount = 4;
        private const float MinimumHorizontalStep = 1f;
        private const float FallbackHorizontalStep = 100f;
        private const float OriginalContentPadding = 20f;

        internal static float GetHorizontalStep(float firstX, float secondX, float firstWidth)
        {
            var observed = Math.Abs(secondX - firstX);
            return observed >= MinimumHorizontalStep
                ? observed
                : Math.Max(firstWidth, FallbackHorizontalStep);
        }

        internal static int GetSlotCapacity(
            float firstCardRight,
            float viewportRight,
            float horizontalStep)
        {
            if (horizontalStep < MinimumHorizontalStep || viewportRight < firstCardRight)
            {
                return 0;
            }

            return 1 + (int)Math.Floor((viewportRight - firstCardRight) / horizontalStep);
        }

        internal static ResearchQueueDisplayPlan CreateDisplayPlan(
            int queueCount,
            int slotCapacity)
        {
            var safeQueueCount = Math.Max(0, queueCount);
            var safeCapacity = Math.Max(0, slotCapacity);
            if (safeQueueCount <= safeCapacity)
            {
                return new ResearchQueueDisplayPlan(
                    safeQueueCount,
                    safeQueueCount,
                    false);
            }

            return new ResearchQueueDisplayPlan(
                Math.Max(0, safeCapacity - 1),
                safeCapacity,
                true);
        }

        internal static NodePosition GetRowPosition(
            NodePosition first,
            float horizontalStep,
            int index)
        {
            return new NodePosition(first.X + (horizontalStep * index), first.Y);
        }

        internal static float GetCanvasFallbackRight(
            float firstCardLeft,
            float canvasLeft,
            float canvasRight)
        {
            var safeMargin = Math.Max(0f, firstCardLeft - canvasLeft);
            return canvasRight - safeMargin;
        }

        internal static float GetContentWidth(
            int displayedSlotCount,
            float cardWidth,
            float horizontalStep)
        {
            if (displayedSlotCount <= 0)
            {
                return OriginalContentPadding;
            }

            return cardWidth +
                   ((displayedSlotCount - 1) * horizontalStep) +
                   OriginalContentPadding;
        }
    }
}
```

- [ ] **Step 4: Run pure tests and the existing core tests**

Run:

```powershell
dotnet test .\UnlimitedResearchAndTradeQueues.sln -c Debug `
  /p:InstallAfterBuild=false `
  --filter 'FullyQualifiedName~ResearchQueueLayoutRulesTests|FullyQualifiedName~CoreRulesTests' `
  --nologo
```

Expected: all selected tests pass with zero warnings and zero failures.

- [ ] **Step 5: Record checkpoint**

Record the selected test count and output. Do not create a Git commit because this project is not a repository.

---

### Task 2: 视口容量门禁与安全节点增长

**Files:**
- Create: `src/UnlimitedResearchAndTradeQueues/Runtime/ResearchQueueLayoutRuntime.cs`
- Modify: `src/UnlimitedResearchAndTradeQueues/Runtime/ResearchQueueRuntime.cs`
- Modify: `tests/UnlimitedResearchAndTradeQueues.Tests/PluginContractTests.cs`
- Modify: `tests/UnlimitedResearchAndTradeQueues.Tests/GameContractTests.cs`

**Interfaces:**
- Consumes: Task 1 的全部 `ResearchQueueLayoutRules` 方法。
- Produces:
  - `ResearchQueueLayoutMetrics`：节点父级、首节点位置、卡片宽度、水平步长、槽位容量。
  - `ResearchQueueLayoutRuntime.TryGetMetrics(ResearchingGroup, TechNode[], out ResearchQueueLayoutMetrics): bool`
  - `ResearchQueueRuntime.GetCurrentQueueCount(ResearchUI): int`（从 `private` 改为 `internal`）。

- [ ] **Step 1: Characterize the original loop, then add failing viewport contracts**

Add to `GameContractTests.cs`:

```csharp
[Fact]
public void ResearchRefreshUsesThreeFullQueueLoopBoundsWithNativeNodeIndexing()
{
    using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
    {
        var method = FindMethod(module, "ResearchingGroup", "ResearchingGroupSet");
        var instructions = method.Body.Instructions;
        var fullQueueLoopGuards = new List<Instruction>();
        for (var index = 1; index < instructions.Count; index++)
        {
            var code = instructions[index].OpCode.Code;
            if (code != Code.Bge && code != Code.Bge_S)
            {
                continue;
            }

            if (instructions[index - 1].Operand is MethodReference called &&
                called.DeclaringType.FullName ==
                    "System.Collections.Generic.List`1<UpgradeNode>" &&
                called.Name == "get_Count")
            {
                fullQueueLoopGuards.Add(instructions[index]);
            }
        }

        Assert.Equal(3, fullQueueLoopGuards.Count);
        Assert.Contains(instructions, instruction =>
            instruction.Operand is FieldReference field &&
            field.DeclaringType.FullName == "ResearchingGroup" &&
            field.Name == "Arr_Technode");
        Assert.Contains(instructions, instruction =>
            instruction.OpCode.Code == Code.Ldelem_Ref);
    }
}
```

Run this characterization test first and verify it passes against the pinned
game assembly. This is the reason native node growth must still match the full
queue length.

First, in the existing
`RuntimeExpandsNativeNodesAndUsesInfinityLabelsWithoutSaveOrConfigTypes` test,
replace the stale `QueueRules.GetNextNodePosition` call assertion with:

```csharp
AssertCalls(
    ensure,
    "UnlimitedResearchAndTradeQueues.Core.ResearchQueueLayoutRules",
    "GetRowPosition");
```

Then add to `PluginContractTests.cs`:

```csharp
[Fact]
public void ResearchCapacityUsesViewportMetricsBeforeNativeNodeGrowth()
{
    using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
    {
        var ensure = FindMethod(
            module,
            "UnlimitedResearchAndTradeQueues.Runtime.ResearchQueueRuntime",
            "EnsureVisibleCapacity");
        AssertCalls(
            ensure,
            "UnlimitedResearchAndTradeQueues.Runtime.ResearchQueueLayoutRuntime",
            "TryGetMetrics");
        AssertCalls(ensure, "UnityEngine.Object", "Instantiate");
        AssertCalls(
            ensure,
            "UnlimitedResearchAndTradeQueues.Core.ResearchQueueLayoutRules",
            "GetRowPosition");
        Assert.NotEmpty(ensure.Body.ExceptionHandlers);

        var metrics = FindMethod(
            module,
            "UnlimitedResearchAndTradeQueues.Runtime.ResearchQueueLayoutRuntime",
            "TryGetMetrics");
        AssertCalls(metrics, "UnityEngine.RectTransform", "GetWorldCorners");
        AssertCalls(
            metrics,
            "UnlimitedResearchAndTradeQueues.Core.ResearchQueueLayoutRules",
            "GetSlotCapacity");
    }
}
```

- [ ] **Step 2: Run the new plugin contract and verify RED**

Run:

```powershell
dotnet test .\UnlimitedResearchAndTradeQueues.sln -c Debug `
  /p:InstallAfterBuild=false `
  --filter 'FullyQualifiedName~ResearchCapacityUsesViewportMetricsBeforeNativeNodeGrowth' `
  --nologo
```

Expected: fail because `ResearchQueueLayoutRuntime` and `TryGetMetrics` do not exist.

- [ ] **Step 3: Create runtime metrics and viewport resolution**

Create `ResearchQueueLayoutRuntime.cs` with these types and methods:

```csharp
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnlimitedResearchAndTradeQueues.Core;

namespace UnlimitedResearchAndTradeQueues.Runtime
{
    internal readonly struct ResearchQueueLayoutMetrics
    {
        internal ResearchQueueLayoutMetrics(
            RectTransform nodeParent,
            NodePosition firstPosition,
            float cardWidth,
            float horizontalStep,
            int slotCapacity)
        {
            NodeParent = nodeParent;
            FirstPosition = firstPosition;
            CardWidth = cardWidth;
            HorizontalStep = horizontalStep;
            SlotCapacity = slotCapacity;
        }

        internal RectTransform NodeParent { get; }
        internal NodePosition FirstPosition { get; }
        internal float CardWidth { get; }
        internal float HorizontalStep { get; }
        internal int SlotCapacity { get; }
    }

    internal static class ResearchQueueLayoutRuntime
    {
        internal static bool TryGetMetrics(
            ResearchingGroup group,
            TechNode[] nodes,
            out ResearchQueueLayoutMetrics metrics)
        {
            metrics = default;
            if (group == null || nodes == null || nodes.Length < 2 ||
                nodes[0] == null || nodes[1] == null)
            {
                return false;
            }

            var first = nodes[0].transform as RectTransform;
            var second = nodes[1].transform as RectTransform;
            var parent = first != null ? first.parent as RectTransform : null;
            if (first == null || second == null || parent == null)
            {
                return false;
            }

            var viewport = FindViewport(parent, out var usesCanvasFallback);
            if (viewport == null)
            {
                return false;
            }

            var firstCorners = new Vector3[4];
            var viewportCorners = new Vector3[4];
            first.GetWorldCorners(firstCorners);
            viewport.GetWorldCorners(viewportCorners);
            var firstRight = parent.InverseTransformPoint(firstCorners[2]).x;
            var viewportRight = parent.InverseTransformPoint(viewportCorners[2]).x;
            if (usesCanvasFallback)
            {
                var firstLeft = parent.InverseTransformPoint(firstCorners[0]).x;
                var viewportLeft = parent.InverseTransformPoint(viewportCorners[0]).x;
                viewportRight = ResearchQueueLayoutRules.GetCanvasFallbackRight(
                    firstLeft,
                    viewportLeft,
                    viewportRight);
            }

            var step = ResearchQueueLayoutRules.GetHorizontalStep(
                first.anchoredPosition.x,
                second.anchoredPosition.x,
                first.rect.width);
            var capacity = ResearchQueueLayoutRules.GetSlotCapacity(
                firstRight,
                viewportRight,
                step);
            if (capacity < ResearchQueueLayoutRules.MinimumSummarySlotCount)
            {
                return false;
            }

            metrics = new ResearchQueueLayoutMetrics(
                parent,
                new NodePosition(first.anchoredPosition.x, first.anchoredPosition.y),
                first.rect.width,
                step,
                capacity);
            return true;
        }

        private static RectTransform FindViewport(
            RectTransform nodeParent,
            out bool usesCanvasFallback)
        {
            usesCanvasFallback = false;
            for (var current = (Transform)nodeParent;
                 current != null;
                 current = current.parent)
            {
                var rect = current as RectTransform;
                if (rect != null &&
                    (current.GetComponent<RectMask2D>() != null ||
                     current.GetComponent<Mask>() != null))
                {
                    return rect;
                }
            }

            var canvas = nodeParent.GetComponentInParent<Canvas>();
            var canvasRect = canvas != null ? canvas.transform as RectTransform : null;
            usesCanvasFallback = canvasRect != null;
            return canvasRect;
        }
    }
}
```

- [ ] **Step 4: Gate `EnsureVisibleCapacity` with viewport readiness**

In `ResearchQueueRuntime.cs`, change `GetCurrentQueueCount` from `private` to `internal`, then replace the body after the node-array validation with:

```csharp
var metrics = default(ResearchQueueLayoutMetrics);
if (desiredCount > 3 &&
    !ResearchQueueLayoutRuntime.TryGetMetrics(group, nodes, out metrics))
{
    return false;
}

if (desiredCount <= nodes.Length)
{
    return true;
}

if (desiredCount <= 3)
{
    return false;
}

var originalLength = nodes.Length;
var expanded = new TechNode[desiredCount];
Array.Copy(nodes, expanded, nodes.Length);
var source = nodes[0];
var sourceRect = source != null ? source.transform as RectTransform : null;
if (sourceRect == null)
{
    return false;
}

for (var index = nodes.Length; index < desiredCount; index++)
{
    var clone = UnityEngine.Object.Instantiate(source, metrics.NodeParent);
    var cloneRect = clone.transform as RectTransform;
    if (cloneRect == null)
    {
        return false;
    }

    clone.name = $"TechNode_Queue_{index}";
    var position = ResearchQueueLayoutRules.GetRowPosition(
        metrics.FirstPosition,
        metrics.HorizontalStep,
        index);
    cloneRect.anchoredPosition = new Vector2(position.X, position.Y);
    cloneRect.SetSiblingIndex(sourceRect.GetSiblingIndex() + index);
    clone.gameObject.SetActive(false);
    expanded[index] = clone;
}

NodeArray(group) = expanded;
if (!_loggedFirstExpansion)
{
    _loggedFirstExpansion = true;
    Plugin.LogRuntimeInfo(
        $"研究队列界面首次扩容：{originalLength} -> {expanded.Length} 个节点；" +
        $"当前单行视觉容量 {metrics.SlotCapacity}。");
}

return true;
```

Keep the existing outer `try/catch` and original-limit fallback unchanged.

- [ ] **Step 5: Run the new contract and all research contracts**

Run:

```powershell
dotnet test .\UnlimitedResearchAndTradeQueues.sln -c Debug `
  /p:InstallAfterBuild=false `
  --filter 'FullyQualifiedName~PluginContractTests|FullyQualifiedName~GameContractTests' `
  --nologo
```

Expected: all selected tests pass. The existing transform regression must remain green.

- [ ] **Step 6: Record checkpoint**

Record the selected test count and verify `InstallAfterBuild=false` left the installed DLL hash unchanged.

---

### Task 3: Postfix 单行布局和非交互省略号

**Files:**
- Modify: `src/UnlimitedResearchAndTradeQueues/Runtime/ResearchQueueLayoutRuntime.cs`
- Modify: `src/UnlimitedResearchAndTradeQueues/Patches/QueuePatches.cs`
- Modify: `tests/UnlimitedResearchAndTradeQueues.Tests/PluginContractTests.cs`

**Interfaces:**
- Consumes: Task 2 的 `TryGetMetrics`、`ResearchQueueRuntime.GetCurrentQueueCount`。
- Produces:
  - `ResearchQueueLayoutRuntime.ApplySingleRowSummary(ResearchingGroup): void`
  - `ResearchQueueLayoutRuntime.ConfigureOverflowIndicator(TechNode): void`
  - `ResearchQueueViewPatch.Postfix(ResearchingGroup): void`

- [ ] **Step 1: Write failing adapter and overflow contracts**

Add to `PluginContractTests.cs`:

```csharp
[Fact]
public void ResearchViewPostfixAppliesSingleRowSummary()
{
    using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
    {
        AssertCalls(
            FindMethod(
                module,
                "UnlimitedResearchAndTradeQueues.Patches.ResearchQueueViewPatch",
                "Postfix"),
            "UnlimitedResearchAndTradeQueues.Runtime.ResearchQueueLayoutRuntime",
            "ApplySingleRowSummary");
    }
}

[Fact]
public void OverflowIndicatorIsSeparateAndCannotReceiveClicks()
{
    using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
    {
        var apply = FindMethod(
            module,
            "UnlimitedResearchAndTradeQueues.Runtime.ResearchQueueLayoutRuntime",
            "ApplySingleRowSummary");
        AssertCalls(
            apply,
            "UnlimitedResearchAndTradeQueues.Core.ResearchQueueLayoutRules",
            "CreateDisplayPlan");
        Assert.DoesNotContain(apply.Body.Instructions, instruction =>
            instruction.OpCode.Code == Mono.Cecil.Cil.Code.Stind_Ref);

        var configure = FindMethod(
            module,
            "UnlimitedResearchAndTradeQueues.Runtime.ResearchQueueLayoutRuntime",
            "ConfigureOverflowIndicator");
        AssertCalls(configure, "UnityEngine.CanvasGroup", "set_interactable");
        AssertCalls(configure, "UnityEngine.CanvasGroup", "set_blocksRaycasts");
        Assert.Contains(configure.Body.Instructions, instruction =>
            instruction.Operand is string text && text == "...");
    }
}
```

- [ ] **Step 2: Run contracts and verify RED**

Run:

```powershell
dotnet test .\UnlimitedResearchAndTradeQueues.sln -c Debug `
  /p:InstallAfterBuild=false `
  --filter 'FullyQualifiedName~ResearchViewPostfixAppliesSingleRowSummary|FullyQualifiedName~OverflowIndicatorIsSeparateAndCannotReceiveClicks' `
  --nologo
```

Expected: fail because the Postfix and runtime methods do not exist.

- [ ] **Step 3: Add the Harmony Postfix**

In `QueuePatches.cs`, extend `ResearchQueueViewPatch`:

```csharp
internal static void Postfix(ResearchingGroup __instance)
{
    ResearchQueueLayoutRuntime.ApplySingleRowSummary(__instance);
}
```

- [ ] **Step 4: Add single-row application and overflow indicator**

Extend `ResearchQueueLayoutRuntime` with the private field reference and these methods:

```csharp
private const string OverflowIndicatorName =
    "UnlimitedResearchQueueOverflowIndicator";

private static readonly AccessTools.FieldRef<ResearchingGroup, TechNode[]> NodeArray =
    AccessTools.FieldRefAccess<ResearchingGroup, TechNode[]>("Arr_Technode");

private static readonly AccessTools.FieldRef<ResearchingGroup, RectTransform> Area =
    AccessTools.FieldRefAccess<ResearchingGroup, RectTransform>("Tf_Area");

internal static void ApplySingleRowSummary(ResearchingGroup group)
{
    try
    {
        if (group == null)
        {
            return;
        }

        var nodes = NodeArray(group);
        if (!TryGetMetrics(group, nodes, out var metrics))
        {
            return;
        }

        var research = GameMgr.Instance?._ResearchUI;
        if (research == null)
        {
            return;
        }

        var queueCount = ResearchQueueRuntime.GetCurrentQueueCount(research);
        var plan = ResearchQueueLayoutRules.CreateDisplayPlan(
            queueCount,
            metrics.SlotCapacity);
        for (var index = 0; index < nodes.Length; index++)
        {
            var node = nodes[index];
            var rect = node != null ? node.transform as RectTransform : null;
            if (rect == null)
            {
                continue;
            }

            var isVisible = index < plan.VisibleResearchCount;
            if (isVisible)
            {
                var position = ResearchQueueLayoutRules.GetRowPosition(
                    metrics.FirstPosition,
                    metrics.HorizontalStep,
                    index);
                rect.anchoredPosition = new Vector2(position.X, position.Y);
            }

            node.gameObject.SetActive(isVisible);
        }

        var indicator = CreateOrGetOverflowIndicator(nodes[0], metrics.NodeParent);
        if (indicator != null)
        {
            var indicatorRect = indicator.transform as RectTransform;
            if (indicatorRect != null && plan.ShowOverflow)
            {
                var position = ResearchQueueLayoutRules.GetRowPosition(
                    metrics.FirstPosition,
                    metrics.HorizontalStep,
                    plan.DisplayedSlotCount - 1);
                indicatorRect.anchoredPosition = new Vector2(position.X, position.Y);
                indicatorRect.SetAsLastSibling();
            }

            indicator.gameObject.SetActive(plan.ShowOverflow);
        }

        var area = Area(group);
        if (area != null)
        {
            area.sizeDelta = new Vector2(
                ResearchQueueLayoutRules.GetContentWidth(
                    plan.DisplayedSlotCount,
                    metrics.CardWidth,
                    metrics.HorizontalStep),
                130f);
        }
    }
    catch (System.Exception exception)
    {
        Plugin.LogRuntimeError("应用研究队列单行摘要失败，保留原版显示。", exception);
    }
}

private static TechNode CreateOrGetOverflowIndicator(
    TechNode source,
    RectTransform parent)
{
    var existing = parent.Find(OverflowIndicatorName);
    var indicator = existing != null ? existing.GetComponent<TechNode>() : null;
    if (indicator != null)
    {
        return indicator;
    }

    indicator = UnityEngine.Object.Instantiate(source, parent);
    indicator.name = OverflowIndicatorName;
    ConfigureOverflowIndicator(indicator);
    indicator.gameObject.SetActive(false);
    return indicator;
}

internal static void ConfigureOverflowIndicator(TechNode indicator)
{
    indicator.enabled = false;
    var canvasGroup = indicator.GetComponent<CanvasGroup>() ??
                      indicator.gameObject.AddComponent<CanvasGroup>();
    canvasGroup.interactable = false;
    canvasGroup.blocksRaycasts = false;

    if (indicator.Txt_Name != null)
    {
        indicator.Txt_Name.text = "...";
        indicator.Txt_Name.gameObject.SetActive(true);
    }

    Hide(indicator.Img_Icon);
    Hide(indicator.Img_Lock);
    Hide(indicator.Img_CatIcon);
    Hide(indicator.Obj_Highlight);
    Hide(indicator.Obj_CatFrame);
    Hide(indicator.m_ReligionFrame);
    Hide(indicator.m_Gauge);
    Hide(indicator.m_TimePad);
}

private static void Hide(Component component)
{
    if (component != null)
    {
        component.gameObject.SetActive(false);
    }
}

private static void Hide(GameObject gameObject)
{
    if (gameObject != null)
    {
        gameObject.SetActive(false);
    }
}
```

- [ ] **Step 5: Run the two contracts and the full Debug suite**

Run:

```powershell
dotnet test .\UnlimitedResearchAndTradeQueues.sln -c Debug `
  /p:InstallAfterBuild=false `
  --filter 'FullyQualifiedName~ResearchViewPostfixAppliesSingleRowSummary|FullyQualifiedName~OverflowIndicatorIsSeparateAndCannotReceiveClicks' `
  --nologo
dotnet test .\UnlimitedResearchAndTradeQueues.sln -c Debug `
  /p:InstallAfterBuild=false --nologo
```

Expected: targeted contracts pass; full suite has zero failures and zero warnings.

- [ ] **Step 6: Record checkpoint**

Record the full Debug test count and confirm the installed DLL hash is still the pre-build value.

---

### Task 4: 文档、Release、安装和实机验收

**Files:**
- Modify: `README.md`
- Regenerate: `dist/贸易站和研究去除最大队列限制-v0.1.0-BepInEx5.zip`
- Install: `E:\steam\steamapps\common\Ratopia\BepInEx\plugins\UnlimitedResearchAndTradeQueues\UnlimitedResearchAndTradeQueues.dll`

**Interfaces:**
- Consumes: Tasks 1-3 的完整行为。
- Produces: 经过 Release 测试、包扫描、备份、安装哈希和游戏日志验证的修正版。

- [ ] **Step 1: Update README behavior description**

Replace the research UI feature bullets with:

```markdown
- 研究队列按当前面板宽度自适应为单行显示，不再换行覆盖下方内容。
- 放不下全部研究时，最后一格显示不可点击的 `...`；隐藏项目仍按原顺序推进。
- 研究界面只激活当前单行可见节点；队列缩短时自动复用，分辨率变化后重新打开界面会重新布局。
```

- [ ] **Step 2: Run Release tests and build without installation**

Run:

```powershell
$env:RATOPIA_DIR='E:\steam\steamapps\common\Ratopia'
dotnet test .\UnlimitedResearchAndTradeQueues.sln -c Release `
  /p:InstallAfterBuild=false --nologo
dotnet build .\UnlimitedResearchAndTradeQueues.sln -c Release `
  /p:InstallAfterBuild=false --no-restore --nologo
```

Expected: all tests pass; build reports 0 warnings and 0 errors.

- [ ] **Step 3: Regenerate and scan the ZIP**

Run:

```powershell
& .\scripts\Package.ps1 -GameDir 'E:\steam\steamapps\common\Ratopia'
& 'C:\Users\ASUS\.codex\skills\developing-ratopia-mods\scripts\Test-RatopiaPackage.ps1' `
  -Path '.\dist\贸易站和研究去除最大队列限制-v0.1.0-BepInEx5.zip' `
  -ExpectedPluginName 'UnlimitedResearchAndTradeQueues'
```

Expected: only `README.md` and `BepInEx/plugins/UnlimitedResearchAndTradeQueues/UnlimitedResearchAndTradeQueues.dll`; forbidden and unexpected lists are empty.

- [ ] **Step 4: Stop at the game-running gate**

Run:

```powershell
Get-Process -Name Ratopia -ErrorAction SilentlyContinue |
  Select-Object Id,StartTime,Path
```

Expected before installation: no process. If a process exists, ask the user to exit; never terminate it.

- [ ] **Step 5: Back up saves and the installed DLL**

Run:

```powershell
$gameDir = 'E:\steam\steamapps\common\Ratopia'
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$backupRoot = Join-Path (Resolve-Path '.\backups') "pre-adaptive-layout-$stamp"
$saveSource = Join-Path $gameDir 'Ratopia_Data\SaveFile'
$saveTarget = Join-Path $backupRoot 'SaveFile'
$dllSource = Join-Path $gameDir `
  'BepInEx\plugins\UnlimitedResearchAndTradeQueues\UnlimitedResearchAndTradeQueues.dll'
$dllTarget = Join-Path $backupRoot 'UnlimitedResearchAndTradeQueues.dll'

New-Item -ItemType Directory -Path $backupRoot | Out-Null
Copy-Item -LiteralPath $saveSource -Destination $saveTarget -Recurse
Copy-Item -LiteralPath $dllSource -Destination $dllTarget

foreach ($sourceFile in Get-ChildItem -LiteralPath $saveSource -Recurse -File)
{
    $relative = $sourceFile.FullName.Substring($saveSource.Length).TrimStart('\')
    $targetFile = Join-Path $saveTarget $relative
    $sourceHash = (Get-FileHash -LiteralPath $sourceFile.FullName -Algorithm SHA256).Hash
    $targetHash = (Get-FileHash -LiteralPath $targetFile -Algorithm SHA256).Hash
    if ($sourceHash -ne $targetHash)
    {
        throw "Save backup hash mismatch: $relative"
    }
}

$installedHash = (Get-FileHash -LiteralPath $dllSource -Algorithm SHA256).Hash
$backupDllHash = (Get-FileHash -LiteralPath $dllTarget -Algorithm SHA256).Hash
if ($installedHash -ne $backupDllHash)
{
    throw 'Installed DLL backup hash mismatch.'
}
```

Expected: every copied save file and the installed DLL have matching SHA-256. Abort before installation if any comparison differs.

- [ ] **Step 6: Install and verify exact DLL identity**

Run:

```powershell
& .\scripts\Install.ps1 `
  -GameDir 'E:\steam\steamapps\common\Ratopia' `
  -PluginDll '.\src\UnlimitedResearchAndTradeQueues\bin\Release\net472\UnlimitedResearchAndTradeQueues.dll'
```

Expected: source and target SHA-256 match; target directory contains only `UnlimitedResearchAndTradeQueues.dll`.

- [ ] **Step 7: Launch and verify runtime gates**

Launch `Ratopia.exe` visibly, record PID and log start time, then inspect the current session in `BepInEx/LogOutput.log`.

Expected:

- `Loading [贸易站和研究去除最大队列限制 0.1.0]`.
- Five patch-install lines including `ResearchQueueViewPatch`.
- `贸易站和研究去除最大队列限制 v0.1.0 已启用。`.
- No current-session `NullReferenceException`, `扩充研究队列界面失败`, `应用研究队列单行摘要失败`, or plugin disablement.

- [ ] **Step 8: User-visible acceptance**

In the test save:

1. Queue 3 items and confirm the original first row is unchanged.
2. Queue items 4 through 8 and confirm every visible card remains on the first row.
3. Confirm the last visible slot becomes a non-clickable `...` only after the panel is full.
4. Let one front item complete and confirm the next hidden item appears.
5. Repeat once at a narrower supported resolution and once at a wider supported resolution; confirm the visible slot count adapts while cards keep their original size.
6. Repeat in Basic, Science and Magic categories.
7. Confirm trade still exceeds 3 and shows `当前/∞`.
8. Save, exit and reload twice; compare order, count and progress.

Expected: no overlap, no second row, no lost or duplicated research, no trade regression.

- [ ] **Step 9: Final verification checkpoint**

Run a fresh full Release test, installed/build hash comparison and current-session log error scan immediately before reporting completion. Report any unexecuted manual acceptance item explicitly rather than marking it passed.
