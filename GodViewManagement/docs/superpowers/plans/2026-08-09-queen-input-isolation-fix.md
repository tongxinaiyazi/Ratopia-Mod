# Queen Input Isolation Fix Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 上帝视角开启时屏蔽女王的全部方向输入，同时保留相机原始 WASD/边缘滚屏与女王的非移动状态更新。

**Architecture:** 用纯 C# 规则识别方向热键，并用可嵌套、幂等释放的作用域标记 `T_Queen.Update` 调用链。Harmony 只在该作用域和模式开启时令 `InputMgr.GetKey(HotKeyName, bool)` 的方向查询返回 `false`；进入模式时额外停止已开始的女王移动。

**Tech Stack:** C# / net472、BepInEx 5.4.23.5、Harmony 2.9.0、Unity Input System、xUnit 2.9.2、Mono.Cecil。

## Global Constraints

- 项目根目录：当前仓库根目录，不得修改其他 Mod 项目。
- 游戏目录：`E:\steam\steamapps\common\Ratopia`。
- `Assembly-CSharp.dll` SHA-256 必须为 `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`。
- 插件 ID、显示名和版本保持 `cn.ratopia.godviewmanagement`、`上帝视角管理`、`0.1.0`。
- 不跳过整个 `T_Queen.Update`，不修改存档字段，不修改原版 Input Action Asset 或绑定。
- 游戏、Unity、Input System、BepInEx 和 Harmony 引用继续保持 `Private=false`。
- 所有生产代码必须先有失败测试；Release 构建与测试统一使用 `/p:InstallAfterBuild=false`。
- 工作区不是 Git 仓库；每个任务记录测试结果，不执行提交命令。

---

### Task 1: 纯方向规则与异常安全作用域

**Files:**
- Create: `src/GodViewManagement/Core/QueenInputIsolationRules.cs`
- Create: `src/GodViewManagement/Core/QueenInputUpdateScope.cs`
- Create: `tests/GodViewManagement.Tests/QueenInputIsolationRulesTests.cs`
- Create: `tests/GodViewManagement.Tests/QueenInputUpdateScopeTests.cs`

**Interfaces:**
- Produces: `QueenInputIsolationRules.ShouldSuppress(bool modeEnabled, bool inQueenUpdate, int hotKeyValue) -> bool`
- Produces: `QueenInputUpdateScope.IsActive -> bool`
- Produces: `QueenInputUpdateScope.Enter() -> IDisposable`

- [ ] **Step 1: 写入方向隔离失败测试**

```csharp
using Xunit;

namespace GodViewManagement.Tests
{
    public sealed class QueenInputIsolationRulesTests
    {
        private static readonly int[] DirectionHotKeys =
        {
            0, 1, 2, 3,
            4, 5, 6, 7,
            20,
            22, 23, 24, 25,
            27, 28, 29
        };

        [Fact]
        public void ActiveQueenUpdateSuppressesEveryDirectionHotKey()
        {
            foreach (var hotKey in DirectionHotKeys)
            {
                Assert.True(QueenInputIsolationRules.ShouldSuppress(true, true, hotKey));
            }
        }

        [Fact]
        public void ModeOffOrOutsideQueenUpdateAlwaysAllowsInput()
        {
            Assert.False(QueenInputIsolationRules.ShouldSuppress(false, true, 0));
            Assert.False(QueenInputIsolationRules.ShouldSuppress(true, false, 0));
        }

        [Theory]
        [InlineData(8)]
        [InlineData(12)]
        [InlineData(18)]
        [InlineData(19)]
        [InlineData(400)]
        public void NonDirectionHotKeysRemainAvailable(int hotKey)
        {
            Assert.False(QueenInputIsolationRules.ShouldSuppress(true, true, hotKey));
        }
    }
}
```

- [ ] **Step 2: 写入作用域失败测试**

```csharp
using System;
using Xunit;

namespace GodViewManagement.Tests
{
    public sealed class QueenInputUpdateScopeTests
    {
        [Fact]
        public void NestedScopesRemainActiveUntilLastLeaseIsDisposed()
        {
            var scope = new QueenInputUpdateScope();
            var outer = scope.Enter();
            var inner = scope.Enter();

            outer.Dispose();
            Assert.True(scope.IsActive);

            inner.Dispose();
            Assert.False(scope.IsActive);
        }

        [Fact]
        public void LeaseDisposalIsIdempotent()
        {
            var scope = new QueenInputUpdateScope();
            var lease = scope.Enter();

            lease.Dispose();
            lease.Dispose();

            Assert.False(scope.IsActive);
        }

        [Fact]
        public void FinallyCleanupClearsScopeAfterException()
        {
            var scope = new QueenInputUpdateScope();

            Assert.Throws<InvalidOperationException>(() =>
            {
                using (scope.Enter())
                {
                    throw new InvalidOperationException("boom");
                }
            });

            Assert.False(scope.IsActive);
        }
    }
}
```

- [ ] **Step 3: 运行定向测试并确认 RED**

Run:

```powershell
$env:RATOPIA_DIR = 'E:\steam\steamapps\common\Ratopia'
dotnet test .\GodViewManagement.sln -c Release /p:InstallAfterBuild=false --filter "FullyQualifiedName~QueenInput" --no-restore
```

Expected: 编译失败，报告 `QueenInputIsolationRules` 和 `QueenInputUpdateScope` 不存在。

- [ ] **Step 4: 实现最小纯逻辑**

`QueenInputIsolationRules.cs`：

```csharp
namespace GodViewManagement
{
    internal static class QueenInputIsolationRules
    {
        public static bool ShouldSuppress(bool modeEnabled, bool inQueenUpdate, int hotKeyValue)
        {
            if (!modeEnabled || !inQueenUpdate)
            {
                return false;
            }

            return (hotKeyValue >= 0 && hotKeyValue <= 7)
                || hotKeyValue == 20
                || (hotKeyValue >= 22 && hotKeyValue <= 25)
                || (hotKeyValue >= 27 && hotKeyValue <= 29);
        }
    }
}
```

`QueenInputUpdateScope.cs`：

```csharp
using System;

namespace GodViewManagement
{
    internal sealed class QueenInputUpdateScope
    {
        private int _depth;

        public bool IsActive => _depth > 0;

        public IDisposable Enter()
        {
            _depth++;
            return new Lease(this);
        }

        private void Exit()
        {
            if (_depth > 0)
            {
                _depth--;
            }
        }

        private sealed class Lease : IDisposable
        {
            private QueenInputUpdateScope _owner;

            public Lease(QueenInputUpdateScope owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                var owner = _owner;
                _owner = null;
                owner?.Exit();
            }
        }
    }
}
```

- [ ] **Step 5: 运行定向测试并确认 GREEN**

Run the Step 3 command again.

Expected: 所有 `QueenInput` 测试通过，失败数为 0。

---

### Task 2: 精确 Harmony 输入拦截与当前移动停止

**Files:**
- Create: `src/GodViewManagement/Patches/QueenInputIsolationPatches.cs`
- Modify: `src/GodViewManagement/Plugin.cs`
- Modify: `src/GodViewManagement/Runtime/GodViewRuntime.cs`
- Modify: `src/GodViewManagement/Runtime/GodViewCameraController.cs`
- Modify: `tests/GodViewManagement.Tests/GameContractTests.cs`
- Modify: `tests/GodViewManagement.Tests/PluginContractTests.cs`

**Interfaces:**
- Consumes: `QueenInputIsolationRules.ShouldSuppress(bool, bool, int)`
- Consumes: `QueenInputUpdateScope.Enter()` and `IsActive`
- Produces: `GodViewRuntime.IsModeEnabled -> bool`
- Produces: `Plugin.EnterQueenInputUpdateScope() -> IDisposable`
- Produces: `Plugin.ShouldSuppressQueenDirection(HotKeyName hotKey) -> bool`

- [ ] **Step 1: 扩展合同测试并确认新增补丁缺失**

在 `GameContractTests.RuntimeTargetsKeepExactSignatures` 增加：

```csharp
AssertMethod(module, "T_Queen", "Update", "System.Void");
AssertMethod(module, "CasselGames.Input.InputMgr", "GetKey", "System.Boolean", "HotKeyName", "System.Boolean");
```

在 `PluginContractTests.RequiredPatchTypes` 增加：

```csharp
"GodViewManagement.Patches.QueenUpdateInputScopePatch",
"GodViewManagement.Patches.DirectionalInputGetKeyPatch",
```

并增加 `EnableStopsAnyMovementStartedBeforeGodView`：

```csharp
[Fact]
public void EnableStopsAnyMovementStartedBeforeGodView()
{
    using (var module = ModuleDefinition.ReadModule(typeof(ManagementModeState).Assembly.Location))
    {
        var controller = module.Types.Single(type =>
            type.FullName == "GodViewManagement.Runtime.GodViewCameraController");
        var enable = controller.Methods.Single(method => method.Name == "Enable");

        Assert.Contains(enable.Body.Instructions,
            instruction => instruction.Operand is MethodReference called
                && called.DeclaringType.FullName == "T_Queen"
                && called.Name == "CharacterStop");
    }
}
```

- [ ] **Step 2: 运行合同测试并确认 RED**

Run:

```powershell
$env:RATOPIA_DIR = 'E:\steam\steamapps\common\Ratopia'
dotnet test .\GodViewManagement.sln -c Release /p:InstallAfterBuild=false --filter "FullyQualifiedName~ContractTests" --no-restore
```

Expected: `EveryRequiredPatchTypeUsesHarmonyPatchDiscovery` 因两个补丁类不存在而失败，`EnableStopsAnyMovementStartedBeforeGodView` 因缺少 `CharacterStop` 调用而失败。

- [ ] **Step 3: 暴露最小模式只读状态与输入作用域入口**

在 `Plugin.cs` 的 using 区加入：

```csharp
using CasselGames.Input;
```

在 `GodViewRuntime` 增加：

```csharp
internal bool IsModeEnabled => _mode.IsEnabled;
```

在 `Plugin` 字段区增加：

```csharp
private readonly QueenInputUpdateScope _queenInputUpdateScope = new QueenInputUpdateScope();
```

在 `Plugin` 增加：

```csharp
internal static IDisposable EnterQueenInputUpdateScope()
{
    var plugin = Instance;
    if (plugin == null
        || !plugin._patchingSucceeded
        || plugin._runtime == null
        || !plugin._runtime.IsModeEnabled)
    {
        return null;
    }

    return plugin._queenInputUpdateScope.Enter();
}

internal static bool ShouldSuppressQueenDirection(HotKeyName hotKey)
{
    var plugin = Instance;
    return plugin != null
        && plugin._patchingSucceeded
        && plugin._runtime != null
        && QueenInputIsolationRules.ShouldSuppress(
            plugin._runtime.IsModeEnabled,
            plugin._queenInputUpdateScope.IsActive,
            (int)hotKey);
}
```

- [ ] **Step 4: 添加两个精确 Harmony 补丁**

创建 `QueenInputIsolationPatches.cs`：

```csharp
using System;
using CasselGames.Input;
using HarmonyLib;

namespace GodViewManagement.Patches
{
    [HarmonyPatch(typeof(T_Queen), "Update")]
    internal static class QueenUpdateInputScopePatch
    {
        private static void Prefix(ref IDisposable __state)
        {
            __state = Plugin.EnterQueenInputUpdateScope();
        }

        private static void Postfix(IDisposable __state)
        {
            __state?.Dispose();
        }

        private static Exception Finalizer(Exception __exception, IDisposable __state)
        {
            __state?.Dispose();
            return __exception;
        }
    }

    [HarmonyPatch(typeof(InputMgr), nameof(InputMgr.GetKey), new Type[] { typeof(HotKeyName), typeof(bool) })]
    internal static class DirectionalInputGetKeyPatch
    {
        private static bool Prefix(HotKeyName __0, ref bool __result)
        {
            if (!Plugin.ShouldSuppressQueenDirection(__0))
            {
                return true;
            }

            __result = false;
            return false;
        }
    }
}
```

- [ ] **Step 5: 开启相机模式前停止女王当前移动**

在 `GodViewCameraController.Enable` 保存相机位置后、解除相机跟随前加入：

```csharp
queen.CharacterStop();
```

- [ ] **Step 6: 运行定向合同测试并确认 GREEN**

Run the Step 2 command again.

Expected: 所有 `ContractTests` 通过，失败数为 0。

- [ ] **Step 7: 运行完整测试防止回归**

Run:

```powershell
$env:RATOPIA_DIR = 'E:\steam\steamapps\common\Ratopia'
dotnet test .\GodViewManagement.sln -c Release /p:InstallAfterBuild=false --no-restore
```

Expected: 所有测试通过，失败数为 0。

---

### Task 3: Release 打包、安装与行为验证

**Files:**
- Modify: `README.md`
- Modify: `docs/TESTING.md`
- Output: `dist/上帝视角管理-v0.1.0-BepInEx5.zip`
- Install: `E:\steam\steamapps\common\Ratopia\BepInEx\plugins\GodViewManagement\GodViewManagement.dll`

**Interfaces:**
- Consumes: Task 1 and Task 2 completed plugin DLL
- Produces: validated release ZIP and hash-matched installed DLL

- [ ] **Step 1: 更新使用说明和验收清单**

在 README 的开启模式说明中明确：

```markdown
- 开启模式会立即停止女王当前移动，并屏蔽女王的 WASD、方向键和手柄方向输入；这些输入不会改变女王坐标。
```

在 `docs/TESTING.md` 的相机验收步骤中明确记录：

```markdown
4. 开启模式后记录女王坐标，分别持续输入 WASD、方向键和手柄方向；确认相机仅由 WASD/边缘滚屏移动，女王坐标始终不变。关闭模式后确认三类原版移动输入全部恢复。
```

- [ ] **Step 2: 运行打包脚本并读取完整结果**

Run:

```powershell
.\scripts\Package.ps1 -RatopiaDir 'E:\steam\steamapps\common\Ratopia'
```

Expected: 完整测试失败数 0；Release 构建 0 警告、0 错误；生成 `dist\上帝视角管理-v0.1.0-BepInEx5.zip`。

- [ ] **Step 3: 使用 Ratopia 包检查器验证 ZIP**

Run:

```powershell
& '<skill-dir>\scripts\Test-RatopiaPackage.ps1' `
  -Path '.\dist\上帝视角管理-v0.1.0-BepInEx5.zip' `
  -ExpectedPluginName 'GodViewManagement'
```

Expected: `ForbiddenFiles`、`UnexpectedFiles` 和 `Errors` 均为空。

- [ ] **Step 4: 确认游戏退出、备份并安装单一 DLL**

Run from the parent directory of this repository：

```powershell
if (Get-Process -Name 'Ratopia' -ErrorAction SilentlyContinue) {
    throw 'Ratopia 正在运行，拒绝安装。'
}

$source = (Resolve-Path -LiteralPath '.\GodViewManagement\src\GodViewManagement\bin\Release\net472\GodViewManagement.dll').Path
$target = 'E:\steam\steamapps\common\Ratopia\BepInEx\plugins\GodViewManagement\GodViewManagement.dll'
$backupDir = '.\backups\GodViewManagement'
New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
if (Test-Path -LiteralPath $target) {
    $backup = Join-Path $backupDir ("GodViewManagement-{0}.dll" -f (Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
    Copy-Item -LiteralPath $target -Destination $backup
}
Copy-Item -LiteralPath $source -Destination $target -Force

$sourceHash = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
$targetHash = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
if ($sourceHash -ne $targetHash) {
    throw '构建 DLL 与安装 DLL 哈希不一致。'
}
```

Expected: Ratopia 未运行；旧 DLL 被备份；构建与安装 SHA-256 完全一致。

- [ ] **Step 5: 启动加载烟雾测试并检查日志**

启动 Ratopia 后检查：

```powershell
Get-Content -LiteralPath 'E:\steam\steamapps\common\Ratopia\BepInEx\LogOutput.log' -Tail 400 |
    Select-String -Pattern '上帝视角管理|GodViewManagement|Error|Exception|失败'
```

Expected: BepInEx 加载 `上帝视角管理 0.1.0`，新增补丁安装没有异常；进入存档后出现会话初始化日志。

- [ ] **Step 6: 执行原始问题的游戏内验收**

1. 进入存档并记录女王坐标。
2. 开启上帝视角，确认女王当前移动立即停止。
3. 分别持续按 WASD、方向键和手柄方向，确认女王坐标不变。
4. 确认 WASD 和屏幕边缘仍移动相机。
5. 关闭上帝视角，确认键盘、方向键和手柄重新控制女王。
6. 打开设置面板和远程建筑面板后重复方向输入，确认没有遗留或越界屏蔽。

Expected: 原始问题不再复现，模式关闭行为与原版一致。
