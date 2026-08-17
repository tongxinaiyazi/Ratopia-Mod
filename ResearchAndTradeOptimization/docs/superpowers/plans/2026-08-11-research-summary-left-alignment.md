# Research Summary Left Alignment Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Do not use subagents: the Ratopia mod workflow requires one agent to own runtime evidence, tests, packaging, and installation.

**Goal:** Move the expanded black research-summary frame so its left edge aligns with the canvas and the earliest queued item is fully visible.

**Architecture:** Add a pure horizontal alignment rule that calculates `canvasLeft - areaLeft`, allowing both right and left corrections. After `ApplySingleRowSummary()` updates the six-slot frame width, a focused runtime helper converts the frame and canvas world corners into the frame parent's local coordinates and applies the correction to `anchoredPosition.x`.

**Tech Stack:** C# 7.3, .NET Framework 4.7.2, BepInEx 5.4.23.5, Harmony 2.9.0, Unity UI, xUnit, Mono.Cecil, PowerShell 7.

## Global Constraints

- Preserve the confirmed summary rule: earliest five researches in slots 1-5 and non-interactive `...` in slot 6 when the queue exceeds five.
- Preserve the complete native node array, research order, progress, and save format.
- Do not hard-code the observed correction `131.8`.
- Recalculate alignment after every frame-width update so queue and resolution changes cannot accumulate drift.
- Do not change any trade behavior.
- Run all tests/builds with `/p:InstallAfterBuild=false` and install only after Ratopia exits.
- The workspace is not a Git repository, so commit steps are intentionally omitted.

---

### Task 1: Add the pure left-edge alignment rule

**Files:**
- Modify: `tests/UnlimitedResearchAndTradeQueues.Tests/ResearchQueueLayoutRulesTests.cs`
- Modify: `src/UnlimitedResearchAndTradeQueues/Core/ResearchQueueLayoutRules.cs`

**Interfaces:**
- Produces: `ResearchQueueLayoutRules.GetHorizontalAlignmentShift(float areaLeft, float canvasLeft): float`.

- [ ] **Step 1: Write the failing alignment test**

```csharp
[Theory]
[InlineData(-1091.8f, -960f, 131.8f)]
[InlineData(-960f, -960f, 0f)]
[InlineData(-900f, -960f, -60f)]
public void HorizontalAlignmentShiftPinsTheAreaToTheCanvasLeft(
    float areaLeft,
    float canvasLeft,
    float expected)
{
    Assert.Equal(
        expected,
        Invoke<float>("GetHorizontalAlignmentShift", areaLeft, canvasLeft),
        3);
}
```

- [ ] **Step 2: Run the targeted test and verify RED**

```powershell
dotnet test tests\UnlimitedResearchAndTradeQueues.Tests\UnlimitedResearchAndTradeQueues.Tests.csproj -c Release "/p:RatopiaDir=E:\steam\steamapps\common\Ratopia" /p:InstallAfterBuild=false --filter "FullyQualifiedName~HorizontalAlignmentShiftPinsTheAreaToTheCanvasLeft" --nologo
```

Expected: FAIL because `GetHorizontalAlignmentShift` does not exist.

- [ ] **Step 3: Implement the minimal pure rule**

```csharp
internal static float GetHorizontalAlignmentShift(
    float areaLeft,
    float canvasLeft)
{
    return canvasLeft - areaLeft;
}
```

- [ ] **Step 4: Run all layout-rule tests and verify GREEN**

```powershell
dotnet test tests\UnlimitedResearchAndTradeQueues.Tests\UnlimitedResearchAndTradeQueues.Tests.csproj -c Release "/p:RatopiaDir=E:\steam\steamapps\common\Ratopia" /p:InstallAfterBuild=false --filter "FullyQualifiedName~ResearchQueueLayoutRulesTests" --nologo
```

Expected: all layout-rule tests pass, including positive, zero, and negative corrections.

---

### Task 2: Align the runtime frame after changing its width

**Files:**
- Modify: `tests/UnlimitedResearchAndTradeQueues.Tests/PluginContractTests.cs`
- Modify: `src/UnlimitedResearchAndTradeQueues/Runtime/ResearchQueueLayoutRuntime.cs`

**Interfaces:**
- Consumes: `ResearchQueueLayoutRules.GetHorizontalAlignmentShift(float areaLeft, float canvasLeft)`.
- Produces: `ResearchQueueLayoutRuntime.AlignAreaToCanvasLeft(RectTransform area): bool`.

- [ ] **Step 1: Write the failing runtime contract**

```csharp
[Fact]
public void ResearchSummaryAlignsTheAreaAfterUpdatingItsWidth()
{
    using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
    {
        var apply = FindMethod(
            module,
            "UnlimitedResearchAndTradeQueues.Runtime.ResearchQueueLayoutRuntime",
            "ApplySingleRowSummary");
        var instructions = apply.Body.Instructions;
        var widthUpdate = -1;
        var alignment = -1;
        for (var index = 0; index < instructions.Count; index++)
        {
            if (!(instructions[index].Operand is MethodReference called))
            {
                continue;
            }

            if (called.DeclaringType.FullName == "UnityEngine.RectTransform" &&
                called.Name == "set_sizeDelta")
            {
                widthUpdate = index;
            }
            else if (called.DeclaringType.FullName ==
                         "UnlimitedResearchAndTradeQueues.Runtime.ResearchQueueLayoutRuntime" &&
                     called.Name == "AlignAreaToCanvasLeft")
            {
                alignment = index;
            }
        }
        Assert.True(widthUpdate >= 0);
        Assert.True(alignment > widthUpdate);

        var helper = FindMethod(
            module,
            "UnlimitedResearchAndTradeQueues.Runtime.ResearchQueueLayoutRuntime",
            "AlignAreaToCanvasLeft");
        Assert.Equal(2, helper.Body.Instructions.Count(instruction =>
            instruction.Operand is MethodReference called &&
            called.DeclaringType.FullName == "UnityEngine.RectTransform" &&
            called.Name == "GetWorldCorners"));
        AssertCalls(
            helper,
            "UnlimitedResearchAndTradeQueues.Core.ResearchQueueLayoutRules",
            "GetHorizontalAlignmentShift");
        AssertCalls(helper, "UnityEngine.RectTransform", "set_anchoredPosition");
    }
}
```

- [ ] **Step 2: Run the contract and verify RED**

```powershell
dotnet test tests\UnlimitedResearchAndTradeQueues.Tests\UnlimitedResearchAndTradeQueues.Tests.csproj -c Release "/p:RatopiaDir=E:\steam\steamapps\common\Ratopia" /p:InstallAfterBuild=false --filter "FullyQualifiedName~ResearchSummaryAlignsTheAreaAfterUpdatingItsWidth" --nologo
```

Expected: FAIL because `AlignAreaToCanvasLeft` is absent.

- [ ] **Step 3: Add the focused runtime alignment helper**

```csharp
private static bool AlignAreaToCanvasLeft(RectTransform area)
{
    if (area == null)
    {
        return false;
    }

    var parent = area.parent as RectTransform;
    var canvas = area.GetComponentInParent<Canvas>();
    var canvasRect = canvas != null
        ? canvas.transform as RectTransform
        : null;
    if (parent == null || canvasRect == null)
    {
        return false;
    }

    var areaCorners = new Vector3[4];
    var canvasCorners = new Vector3[4];
    area.GetWorldCorners(areaCorners);
    canvasRect.GetWorldCorners(canvasCorners);
    var areaLeft = parent.InverseTransformPoint(areaCorners[0]).x;
    var canvasLeft = parent.InverseTransformPoint(canvasCorners[0]).x;
    var shift = ResearchQueueLayoutRules.GetHorizontalAlignmentShift(
        areaLeft,
        canvasLeft);
    if (float.IsNaN(shift) || float.IsInfinity(shift))
    {
        return false;
    }

    var position = area.anchoredPosition;
    area.anchoredPosition = new Vector2(position.x + shift, position.y);
    return true;
}
```

- [ ] **Step 4: Call alignment immediately after the width update**

```csharp
area.sizeDelta = new Vector2(
    ResearchQueueLayoutRules.GetContentWidth(
        plan.DisplayedSlotCount,
        metrics.CardWidth,
        metrics.HorizontalStep),
    130f);
AlignAreaToCanvasLeft(area);
```

The existing outer `try/catch` keeps a coordinate failure out of the game loop. The helper returns `false` without moving the frame if required transforms are unavailable.

- [ ] **Step 5: Run all research layout and plugin contract tests**

```powershell
dotnet test tests\UnlimitedResearchAndTradeQueues.Tests\UnlimitedResearchAndTradeQueues.Tests.csproj -c Release "/p:RatopiaDir=E:\steam\steamapps\common\Ratopia" /p:InstallAfterBuild=false --filter "FullyQualifiedName~ResearchQueueLayoutRulesTests|FullyQualifiedName~PluginContractTests" --nologo
```

Expected: all selected tests pass; the five-item/ellipsis, non-interaction, one-row, and trade contracts remain green.

---

### Task 3: Document, verify, package, back up, install, and retest

**Files:**
- Modify: `README.md`
- Modify: `docs/TESTING.md`
- Regenerate: `dist/贸易站和研究去除最大队列限制-v0.1.0-BepInEx5.zip`
- Back up and replace the installed plugin DLL.

- [ ] **Step 1: Document the alignment behavior**

Add to `README.md`:

```markdown
- 黑色研究摘要框会按画布左边界重新对齐，确保最早的研究项目完整可见。
```

Add to `docs/TESTING.md`:

```markdown
- [ ] 黑色摘要框左边界与画布左边界对齐，编号 1 的卡片和编号标记完整可见。
```

- [ ] **Step 2: Run complete Release verification**

```powershell
dotnet test UnlimitedResearchAndTradeQueues.sln -c Release "/p:RatopiaDir=E:\steam\steamapps\common\Ratopia" /p:InstallAfterBuild=false --nologo
dotnet build UnlimitedResearchAndTradeQueues.sln -c Release "/p:RatopiaDir=E:\steam\steamapps\common\Ratopia" /p:InstallAfterBuild=false --no-restore --nologo
```

Expected: all tests pass and build reports 0 warnings, 0 errors.

- [ ] **Step 3: Regenerate and scan the archive with PowerShell 7**

```powershell
pwsh -NoProfile -File scripts\Package.ps1 -GameDir 'E:\steam\steamapps\common\Ratopia'
```

Expected: archive generation succeeds; a manual ZIP entry scan reports zero forbidden and unexpected files.

- [ ] **Step 4: Require game exit, back up the installed DLL, and install**

Confirm `Get-Process -Name Ratopia` returns no process. Copy the installed DLL into `backups/pre-left-alignment-<timestamp>/`, run:

```powershell
pwsh -NoProfile -File scripts\Install.ps1 -GameDir 'E:\steam\steamapps\common\Ratopia'
```

Expected: build and installed DLL SHA-256 values match.

- [ ] **Step 5: Launch and obtain visual acceptance**

Load the eight-item test queue. Verify the black frame starts at the canvas left boundary, card 1 and its number are fully visible, cards 1-5 retain original order, and slot 6 shows `...`. Confirm the BepInEx log contains all five patch-install messages and no plugin error or exception. Do not claim completion until the user confirms the screenshot.
