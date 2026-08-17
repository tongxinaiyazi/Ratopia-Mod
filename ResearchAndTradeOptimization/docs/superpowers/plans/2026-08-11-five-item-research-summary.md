# Five-Item Research Queue Summary Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking. Do not use subagents: the Ratopia mod workflow requires one agent to own reverse engineering, testing, packaging, and installation evidence end to end.

**Goal:** Make the research summary show the earliest five queued researches followed by a non-interactive `...` in the sixth slot whenever more items remain.

**Architecture:** Keep the complete native `Arr_Technode` array so `ResearchingGroupSet()` can bind every queued item without changing game data. Replace only the visual display plan with a fixed five-real-item summary; the existing Postfix then hides later native nodes, places the overflow indicator in slot six, and sizes the black summary area from the six displayed slots instead of the full queue.

**Tech Stack:** C# 7.3, .NET Framework 4.7.2, BepInEx 5.4.23.5, Harmony 2.9.0, Unity UI, xUnit, Mono.Cecil, PowerShell.

## Global Constraints

- Plugin name remains `贸易站和研究去除最大队列限制`, GUID remains `cn.ratopia.unlimitedresearchandtradequeues`, and version remains `0.1.0`.
- Ratopia `Assembly-CSharp.dll` SHA-256 must remain `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`.
- Research queue data, order, progress, and save format must not change.
- The complete native node array must still grow to the complete queue length before the original refresh method runs.
- Trade queue code and behavior must not change.
- The summary shows queue indexes 0-4; queue index 5 and later are hidden.
- A queue of six or more uses slot six for a non-interactive `...` indicator.
- Build and tests must use `/p:InstallAfterBuild=false`; installation happens only after Ratopia has exited.
- The release archive must contain only the plugin DLL under `BepInEx/plugins/UnlimitedResearchAndTradeQueues/` plus `README.md`.
- The workspace is not a Git repository, so commit steps are intentionally omitted.

---

### Task 1: Replace the adaptive display count with the fixed five-item rule

**Files:**
- Modify: `tests/UnlimitedResearchAndTradeQueues.Tests/ResearchQueueLayoutRulesTests.cs`
- Modify: `src/UnlimitedResearchAndTradeQueues/Core/ResearchQueueLayoutRules.cs`

**Interfaces:**
- Produces: `ResearchQueueLayoutRules.MaximumVisibleResearchCount: int` with value `5`.
- Produces: `ResearchQueueLayoutRules.CreateDisplayPlan(int queueCount): ResearchQueueDisplayPlan`.
- Preserves: `ResearchQueueDisplayPlan.VisibleResearchCount`, `DisplayedSlotCount`, and `ShowOverflow`.

- [ ] **Step 1: Write the failing fixed-summary theory**

Replace `DisplayPlanReservesTheLastSlotForOverflow` with:

```csharp
[Theory]
[InlineData(0, 0, 0, false)]
[InlineData(1, 1, 1, false)]
[InlineData(5, 5, 5, false)]
[InlineData(6, 5, 6, true)]
[InlineData(8, 5, 6, true)]
[InlineData(100, 5, 6, true)]
public void DisplayPlanShowsTheEarliestFiveThenOverflow(
    int queueCount,
    int visibleResearch,
    int displayedSlots,
    bool overflow)
{
    var plan = Invoke<object>("CreateDisplayPlan", queueCount);
    Assert.Equal(visibleResearch, Read<int>(plan, "VisibleResearchCount"));
    Assert.Equal(displayedSlots, Read<int>(plan, "DisplayedSlotCount"));
    Assert.Equal(overflow, Read<bool>(plan, "ShowOverflow"));
}
```

- [ ] **Step 2: Run the targeted test and verify RED**

Run:

```powershell
dotnet test tests\UnlimitedResearchAndTradeQueues.Tests\UnlimitedResearchAndTradeQueues.Tests.csproj -c Release "/p:RatopiaDir=E:\steam\steamapps\common\Ratopia" /p:InstallAfterBuild=false --filter "FullyQualifiedName~DisplayPlanShowsTheEarliestFiveThenOverflow" --nologo
```

Expected: FAIL because the existing `CreateDisplayPlan` takes two parameters and uses viewport capacity.

- [ ] **Step 3: Implement the fixed five-item display plan**

In `ResearchQueueLayoutRules`, add the constant and replace `CreateDisplayPlan`:

```csharp
internal const int MaximumVisibleResearchCount = 5;

internal static ResearchQueueDisplayPlan CreateDisplayPlan(int queueCount)
{
    var safeQueueCount = Math.Max(0, queueCount);
    if (safeQueueCount <= MaximumVisibleResearchCount)
    {
        return new ResearchQueueDisplayPlan(
            safeQueueCount,
            safeQueueCount,
            false);
    }

    return new ResearchQueueDisplayPlan(
        MaximumVisibleResearchCount,
        MaximumVisibleResearchCount + 1,
        true);
}
```

Do not change `GetRowPosition` or `GetContentWidth`: six displayed slots already calculate a fixed width of `cardWidth + 5 * horizontalStep + 20`.

- [ ] **Step 4: Run core rule tests and verify GREEN**

Run:

```powershell
dotnet test tests\UnlimitedResearchAndTradeQueues.Tests\UnlimitedResearchAndTradeQueues.Tests.csproj -c Release "/p:RatopiaDir=E:\steam\steamapps\common\Ratopia" /p:InstallAfterBuild=false --filter "FullyQualifiedName~ResearchQueueLayoutRulesTests" --nologo
```

Expected: all `ResearchQueueLayoutRulesTests` pass.

---

### Task 2: Make the runtime use the fixed plan and retain the first five native nodes

**Files:**
- Modify: `tests/UnlimitedResearchAndTradeQueues.Tests/PluginContractTests.cs`
- Modify: `src/UnlimitedResearchAndTradeQueues/Runtime/ResearchQueueLayoutRuntime.cs`
- Modify: `src/UnlimitedResearchAndTradeQueues/Runtime/ResearchQueueRuntime.cs`

**Interfaces:**
- Consumes: `ResearchQueueLayoutRules.MaximumVisibleResearchCount`.
- Consumes: `ResearchQueueLayoutRules.CreateDisplayPlan(int queueCount)`.
- Preserves: `ResearchQueueRuntime.EnsureVisibleCapacity(ResearchingGroup group, int desiredCount): bool` and full-array growth.
- Preserves: `ResearchQueueLayoutRuntime.ApplySingleRowSummary(ResearchingGroup group): void`.

- [ ] **Step 1: Write static regression tests for the fixed plan and threshold**

Replace `ResearchSummaryDefersItsSnapshotUntilTheQueueExceedsThree` with:

```csharp
[Fact]
public void ResearchSummaryUsesTheFixedFiveItemDisplayPlan()
{
    using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
    {
        var apply = FindMethod(
            module,
            "UnlimitedResearchAndTradeQueues.Runtime.ResearchQueueLayoutRuntime",
            "ApplySingleRowSummary");
        var planCall = apply.Body.Instructions.Single(instruction =>
            instruction.Operand is MethodReference called &&
            called.DeclaringType.FullName ==
                "UnlimitedResearchAndTradeQueues.Core.ResearchQueueLayoutRules" &&
            called.Name == "CreateDisplayPlan");
        var planMethod = (MethodReference)planCall.Operand;
        Assert.Single(planMethod.Parameters);
    }
}

[Fact]
public void ResearchSummaryDefersItsSnapshotUntilTheQueueExceedsFive()
{
    using (var module = ModuleDefinition.ReadModule(GetPluginAssemblyPath()))
    {
        var apply = FindMethod(
            module,
            "UnlimitedResearchAndTradeQueues.Runtime.ResearchQueueLayoutRuntime",
            "ApplySingleRowSummary");
        var instructions = apply.Body.Instructions;
        Assert.Contains(Enumerable.Range(0, instructions.Count - 1), index =>
            instructions[index].OpCode.Code == Mono.Cecil.Cil.Code.Ldc_I4_5 &&
            (instructions[index + 1].OpCode.Code == Mono.Cecil.Cil.Code.Ble ||
             instructions[index + 1].OpCode.Code == Mono.Cecil.Cil.Code.Ble_S));
    }
}
```

Keep the existing contracts proving that the Postfix is installed, the overflow indicator is separate/non-interactive, the grid is fixed to one row, and the complete node array grows.

- [ ] **Step 2: Run the two contract tests and verify RED**

Run:

```powershell
dotnet test tests\UnlimitedResearchAndTradeQueues.Tests\UnlimitedResearchAndTradeQueues.Tests.csproj -c Release "/p:RatopiaDir=E:\steam\steamapps\common\Ratopia" /p:InstallAfterBuild=false --filter "FullyQualifiedName~ResearchSummaryUsesTheFixedFiveItemDisplayPlan|FullyQualifiedName~ResearchSummaryDefersItsSnapshotUntilTheQueueExceedsFive" --nologo
```

Expected: FAIL because the runtime still passes `metrics.SlotCapacity` and logs after queue count 3.

- [ ] **Step 3: Use the fixed plan in the Postfix**

In `ApplySingleRowSummary`, replace the adaptive call:

```csharp
var plan = ResearchQueueLayoutRules.CreateDisplayPlan(queueCount);
```

Keep this visibility loop unchanged, because node indexes match original queue order and therefore retain the earliest five:

```csharp
var isVisible = index < plan.VisibleResearchCount;
```

Keep overflow positioning based on:

```csharp
plan.DisplayedSlotCount - 1
```

With the fixed plan this is index 5, the sixth slot. Keep `area.sizeDelta` based on `plan.DisplayedSlotCount`, so an overflow queue always uses exactly six slots and no longer shifts according to full queue length.

- [ ] **Step 4: Move the one-shot diagnostic threshold to six items**

Change the condition to:

```csharp
if (queueCount > ResearchQueueLayoutRules.MaximumVisibleResearchCount &&
    !_loggedFirstLayout)
```

Update the expansion message in `ResearchQueueRuntime` so it does not describe the unrelated measured viewport capacity:

```csharp
Plugin.LogRuntimeInfo(
    $"研究队列界面首次扩容：" +
    $"{originalLength} -> {expanded.Length} 个节点；" +
    $"摘要固定显示前 {ResearchQueueLayoutRules.MaximumVisibleResearchCount} 项，" +
    $"其余使用省略号。");
```

- [ ] **Step 5: Run runtime contract and research rule tests**

Run:

```powershell
dotnet test tests\UnlimitedResearchAndTradeQueues.Tests\UnlimitedResearchAndTradeQueues.Tests.csproj -c Release "/p:RatopiaDir=E:\steam\steamapps\common\Ratopia" /p:InstallAfterBuild=false --filter "FullyQualifiedName~PluginContractTests|FullyQualifiedName~ResearchQueueLayoutRulesTests" --nologo
```

Expected: all selected tests pass. The existing trade contracts must remain unchanged.

---

### Task 3: Update documentation, verify Release, package, back up, and install

**Files:**
- Modify: `README.md`
- Modify: `docs/TESTING.md`
- Regenerate: `dist/贸易站和研究去除最大队列限制-v0.1.0-BepInEx5.zip`
- Back up: `backups/pre-five-item-summary-<timestamp>/UnlimitedResearchAndTradeQueues.dll`
- Install: `E:/steam/steamapps/common/Ratopia/BepInEx/plugins/UnlimitedResearchAndTradeQueues/UnlimitedResearchAndTradeQueues.dll`

**Interfaces:**
- Consumes: the verified Release DLL from Tasks 1-2.
- Produces: a clean release archive and an installed DLL with identical SHA-256.

- [ ] **Step 1: Update user-facing behavior documentation**

Replace the adaptive-width README bullets with:

```markdown
- 研究队列摘要始终优先显示最先排入的五项，并保持原版顺序。
- 队列超过五项时，第六格显示不可点击的 `...`；后续隐藏项目仍按原顺序推进。
- 研究界面只激活摘要所需节点；完整队列节点仍会复用，不改变存档数据。
```

Replace the adaptive-resolution checks in `docs/TESTING.md` with:

```markdown
- [ ] 至少一个研究类别达到 8 项，摘要从左到右显示编号 1–5，第六格显示 `...`，编号 6–8 不显示。
- [ ] `...` 位于第五项正右侧并完全处于黑色摘要框内，不覆盖研究树或右侧详情栏。
- [ ] 完成或取消第一项后，原编号 2–6 成为新的可见前五项，顺序没有丢失或重复。
```

- [ ] **Step 2: Run the complete Release test suite with installation disabled**

Run:

```powershell
dotnet test UnlimitedResearchAndTradeQueues.sln -c Release "/p:RatopiaDir=E:\steam\steamapps\common\Ratopia" /p:InstallAfterBuild=false --nologo
```

Expected: all tests pass; no installation occurs.

- [ ] **Step 3: Build Release with installation disabled**

Run:

```powershell
dotnet build UnlimitedResearchAndTradeQueues.sln -c Release "/p:RatopiaDir=E:\steam\steamapps\common\Ratopia" /p:InstallAfterBuild=false --no-restore --nologo
```

Expected: 0 warnings, 0 errors.

- [ ] **Step 4: Regenerate and scan the release package**

Run:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Package.ps1 -GameDir 'E:\steam\steamapps\common\Ratopia'
```

Expected: the archive is regenerated, its SHA-256 is printed, package tests report no forbidden or unexpected files, and no DLL other than `UnlimitedResearchAndTradeQueues.dll` is present.

- [ ] **Step 5: Confirm Ratopia has exited and back up the installed DLL**

Run:

```powershell
$running = @(Get-Process -Name Ratopia -ErrorAction SilentlyContinue)
if ($running.Count -gt 0) { throw "Ratopia is running: $($running.Id -join ', ')" }
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$backup = "backups\pre-five-item-summary-$stamp"
New-Item -ItemType Directory -Path $backup -Force | Out-Null
Copy-Item -LiteralPath 'E:\steam\steamapps\common\Ratopia\BepInEx\plugins\UnlimitedResearchAndTradeQueues\UnlimitedResearchAndTradeQueues.dll' -Destination $backup
```

Expected: zero Ratopia processes and one timestamped DLL backup. Existing save backups remain untouched.

- [ ] **Step 6: Install and verify source/target hashes**

Run:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Install.ps1 -GameDir 'E:\steam\steamapps\common\Ratopia'
```

Expected: installation succeeds and the source and installed DLL SHA-256 values match.

- [ ] **Step 7: Launch and perform focused runtime acceptance**

Launch `E:\steam\steamapps\common\Ratopia\Ratopia.exe`, load the test-save copy, and queue at least eight researches. Verify the view is `1, 2, 3, 4, 5, ...`; verify 6-8 are hidden; then complete or cancel the first item and verify the next earliest item enters the fifth visible slot. Read `BepInEx/LogOutput.log` and confirm the plugin loaded all five patches with no `ERROR` or `Exception` lines attributable to this plugin.

Do not claim the UI fix complete until the user confirms the screenshot. Do not save the test file until the visual order and overflow placement are confirmed.
