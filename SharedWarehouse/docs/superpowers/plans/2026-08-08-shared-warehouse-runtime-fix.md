# Shared Warehouse Runtime Fix Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 发布“共享仓库”v0.1.1，使《鼠托邦》完成载入后可靠初始化共享库存、无限容量、新建仓库接入以及单次统计/保存保护。

**Architecture:** 使用与 Unity 解耦的 `RuntimeSessionDriver<TManager>` 管理会话切换、初始化一次和失败重试；由 `TileMgr.Update` Harmony 后置补丁调用插件的节流轮询入口。库存合并、容量覆盖和单仓视图继续由现有协调器负责。

**Tech Stack:** C#、.NET Framework 4.7.2、BepInEx 5.4.23.5、Harmony、Unity 2021.3.21f1、xUnit、PowerShell。

## Global Constraints

- 游戏版本固定为《鼠托邦》v1.0.0600。
- BepInEx 固定为 5.4.23.5 Mono。
- 插件 GUID 保持 `cn.ratopia.sharedwarehouse`，名称保持“共享仓库”，版本更新为 `0.1.1`。
- 仅处理 `Storage / 100` 与 `MiniStorage / 181`；不处理 `ElecStorage / 360`。
- 不修复、不推测、不去重旧存档；当前序列化的全部材料按既有规则保留。
- 不打包或分发游戏、Unity、BepInEx、Harmony DLL。
- 自动测试必须传入 `/p:InstallAfterBuild=false`，避免覆盖游戏插件。

## File Structure

- Create: `src/SharedWarehouse/Core/RuntimeSessionDriver.cs` — 管理会话身份、就绪状态、重置和失败重试。
- Create: `src/SharedWarehouse/Patches/RuntimeSessionPatch.cs` — 从 `TileMgr.Update` 接入插件轮询。
- Create: `tests/SharedWarehouse.Tests/RuntimeSessionDriverTests.cs` — 覆盖载入、切换、重置与重试。
- Create: `tests/SharedWarehouse.Tests/RuntimePatchContractTests.cs` — 验证补丁目标和版本契约。
- Modify: `src/SharedWarehouse/Plugin.cs` — 使用会话驱动器并移除 Unity `Update()`。
- Modify: `src/SharedWarehouse/SharedWarehouse.csproj` — 更新程序集版本。
- Modify: `README.md`、`NEXUS_DESCRIPTION.md`、`docs/TESTING.md`、`scripts/Package.ps1` — 更新发布内容。

---

### Task 1: 可测试的运行时会话驱动器

**Files:**
- Create: `src/SharedWarehouse/Core/RuntimeSessionDriver.cs`
- Create: `tests/SharedWarehouse.Tests/RuntimeSessionDriverTests.cs`

**Interfaces:**
- Consumes: `Action<TManager> initialize`、`Action reset`。
- Produces: `Poll(TManager, bool, Action<TManager>, Action)`、`MarkDirty()`、`Reset(Action)`、`IsInitialized`。

- [ ] **Step 1: 写入单次初始化失败测试**

```csharp
using System;
using SharedWarehouse.Core;
using Xunit;

namespace SharedWarehouse.Tests
{
    public sealed class RuntimeSessionDriverTests
    {
        [Fact]
        public void Poll_waits_for_loading_then_initializes_once()
        {
            var manager = new FakeManager();
            var driver = new RuntimeSessionDriver<FakeManager>();
            var initializeCount = 0;

            driver.Poll(manager, true, _ => initializeCount++, () => { });
            driver.Poll(manager, false, _ => initializeCount++, () => { });
            driver.Poll(manager, false, _ => initializeCount++, () => { });

            Assert.Equal(1, initializeCount);
            Assert.True(driver.IsInitialized);
        }

        private sealed class FakeManager
        {
        }
    }
}
```

- [ ] **Step 2: 运行测试并确认 RED**

```powershell
dotnet test .\tests\SharedWarehouse.Tests\SharedWarehouse.Tests.csproj -c Debug --no-restore /p:InstallAfterBuild=false --filter "FullyQualifiedName~Poll_waits_for_loading_then_initializes_once"
```

Expected: FAIL，编译器报告 `RuntimeSessionDriver<>` 不存在。

- [ ] **Step 3: 写入最小实现**

```csharp
using System;

namespace SharedWarehouse.Core
{
    internal sealed class RuntimeSessionDriver<TManager>
        where TManager : class
    {
        private TManager _activeManager;

        public bool IsInitialized { get; private set; }

        public void Poll(
            TManager manager,
            bool isLoading,
            Action<TManager> initialize,
            Action reset)
        {
            if (initialize == null)
            {
                throw new ArgumentNullException(nameof(initialize));
            }

            if (reset == null)
            {
                throw new ArgumentNullException(nameof(reset));
            }

            if (manager == null)
            {
                Reset(reset);
                return;
            }

            if (!ReferenceEquals(manager, _activeManager))
            {
                Reset(reset);
                _activeManager = manager;
            }

            if (isLoading || IsInitialized)
            {
                return;
            }

            initialize(manager);
            IsInitialized = true;
        }

        public void MarkDirty()
        {
            IsInitialized = false;
        }

        public void Reset(Action reset)
        {
            if (reset == null)
            {
                throw new ArgumentNullException(nameof(reset));
            }

            if (_activeManager == null && !IsInitialized)
            {
                return;
            }

            reset();
            _activeManager = null;
            IsInitialized = false;
        }
    }
}
```

- [ ] **Step 4: 运行测试并确认 GREEN**

Run the command from Step 2.

Expected: PASS，1 test passed。

- [ ] **Step 5: 添加会话切换、消失和异常重试测试**

在 `FakeManager` 前添加：

```csharp
[Fact]
public void Poll_resets_old_session_and_initializes_new_manager()
{
    var first = new FakeManager();
    var second = new FakeManager();
    var driver = new RuntimeSessionDriver<FakeManager>();
    var resetCount = 0;
    var initialized = string.Empty;

    driver.Poll(first, false, _ => initialized += "first", () => resetCount++);
    driver.Poll(second, false, _ => initialized += "second", () => resetCount++);

    Assert.Equal("firstsecond", initialized);
    Assert.Equal(1, resetCount);
    Assert.True(driver.IsInitialized);
}

[Fact]
public void Poll_resets_when_manager_disappears()
{
    var driver = new RuntimeSessionDriver<FakeManager>();
    var resetCount = 0;
    driver.Poll(new FakeManager(), false, _ => { }, () => resetCount++);

    driver.Poll(null, false, _ => { }, () => resetCount++);
    driver.Poll(null, false, _ => { }, () => resetCount++);

    Assert.Equal(1, resetCount);
    Assert.False(driver.IsInitialized);
}

[Fact]
public void Poll_retries_after_initializer_throws()
{
    var manager = new FakeManager();
    var driver = new RuntimeSessionDriver<FakeManager>();
    var attempts = 0;

    Assert.Throws<InvalidOperationException>(() =>
        driver.Poll(manager, false, _ =>
        {
            attempts++;
            throw new InvalidOperationException("simulated failure");
        }, () => { }));

    Assert.False(driver.IsInitialized);
    driver.Poll(manager, false, _ => attempts++, () => { });

    Assert.Equal(2, attempts);
    Assert.True(driver.IsInitialized);
}

[Fact]
public void MarkDirty_allows_same_manager_to_initialize_again()
{
    var manager = new FakeManager();
    var driver = new RuntimeSessionDriver<FakeManager>();
    var attempts = 0;
    driver.Poll(manager, false, _ => attempts++, () => { });

    driver.MarkDirty();
    driver.Poll(manager, false, _ => attempts++, () => { });

    Assert.Equal(2, attempts);
    Assert.True(driver.IsInitialized);
}
```

- [ ] **Step 6: 运行驱动器测试并确认 GREEN**

```powershell
dotnet test .\tests\SharedWarehouse.Tests\SharedWarehouse.Tests.csproj -c Debug --no-restore /p:InstallAfterBuild=false --filter "FullyQualifiedName~RuntimeSessionDriverTests"
```

Expected: PASS，5 tests passed。

- [ ] **Step 7: 提交会话驱动器**

```powershell
git add src/SharedWarehouse/Core/RuntimeSessionDriver.cs tests/SharedWarehouse.Tests/RuntimeSessionDriverTests.cs
git commit -m "test: define runtime session lifecycle"
```

---

### Task 2: 从游戏生命周期驱动初始化

**Files:**
- Create: `src/SharedWarehouse/Patches/RuntimeSessionPatch.cs`
- Create: `tests/SharedWarehouse.Tests/RuntimePatchContractTests.cs`
- Modify: `src/SharedWarehouse/Plugin.cs:17-24,28,53-62,82-85,112-150,216-235`

**Interfaces:**
- Consumes: Task 1 的 `RuntimeSessionDriver<BuildingMgr>`、`TileMgr.Update`、现有协调器。
- Produces: `Plugin.TickGameSession()` 和 `RuntimeSessionPatch`。

- [ ] **Step 1: 写入生命周期回归测试**

```csharp
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Xunit;

namespace SharedWarehouse.Tests
{
    public sealed class RuntimePatchContractTests
    {
        [Fact]
        public void Plugin_does_not_depend_on_Unity_Update_message()
        {
            var update = typeof(Plugin).GetMethod(
                "Update",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            var tick = typeof(Plugin).GetMethod(
                "TickGameSession",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            Assert.Null(update);
            Assert.NotNull(tick);
        }

        [Fact]
        public void Runtime_patch_targets_TileMgr_Update()
        {
            var patchType = typeof(Plugin).Assembly.GetType(
                "SharedWarehouse.Patches.RuntimeSessionPatch",
                false);

            Assert.NotNull(patchType);
            var attribute = CustomAttributeData.GetCustomAttributes(patchType)
                .Single(data => data.AttributeType == typeof(HarmonyPatch));
            var values = attribute.ConstructorArguments
                .Select(argument => argument.Value)
                .ToArray();
            Assert.Contains(typeof(TileMgr), values);
            Assert.Contains(nameof(TileMgr.Update), values);
        }
    }
}
```

- [ ] **Step 2: 运行回归测试并确认 RED**

```powershell
dotnet test .\tests\SharedWarehouse.Tests\SharedWarehouse.Tests.csproj -c Debug --no-restore /p:InstallAfterBuild=false --filter "FullyQualifiedName~RuntimePatchContractTests"
```

Expected: FAIL；`Plugin.Update` 仍存在，且找不到 `RuntimeSessionPatch`。

- [ ] **Step 3: 新增 `TileMgr.Update` 补丁**

```csharp
using System;
using HarmonyLib;

namespace SharedWarehouse.Patches
{
    [HarmonyPatch(typeof(TileMgr), nameof(TileMgr.Update))]
    internal static class RuntimeSessionPatch
    {
        private static void Postfix()
        {
            try
            {
                Plugin.Instance?.TickGameSession();
            }
            catch (Exception error)
            {
                Plugin.LogPatchError("运行时会话轮询", error);
            }
        }
    }
}
```

- [ ] **Step 4: 用会话驱动器改造 `Plugin`**

添加命名空间和字段：

```csharp
using SharedWarehouse.Core;

private RuntimeSessionDriver<BuildingMgr> _sessionDriver;
```

在 `Awake()` 创建协调器后创建驱动器：

```csharp
_sessionDriver = new RuntimeSessionDriver<BuildingMgr>();
```

替换就绪属性：

```csharp
internal bool RuntimeReady =>
    _patchingSucceeded && _sessionDriver != null && _sessionDriver.IsInitialized;
```

删除 `private void Update()`，增加内部入口：

```csharp
internal void TickGameSession()
{
    if (!_patchingSucceeded || Time.unscaledTime < _nextPollTime)
    {
        return;
    }

    _nextPollTime = Time.unscaledTime + PollIntervalSeconds;
    PollGameSession();
}
```

将 `MarkSessionDirty` 的状态修改替换为：

```csharp
_sessionDriver?.MarkDirty();
```

将会话轮询与重置替换为：

```csharp
private void PollGameSession()
{
    try
    {
        var game = GameMgr.Instance;
        var manager = game?._BuildingMgr;
        var tileManager = game?._TileMgr;

        if (manager == null || tileManager == null)
        {
            EndCurrentSession();
            return;
        }

        _sessionDriver.Poll(
            manager,
            tileManager.m_GameLoading,
            currentManager =>
            {
                _coordinator.Initialize(currentManager);
                _initializationFailureLogged = false;
            },
            ResetCoordinatorSession);
    }
    catch (Exception error)
    {
        _sessionDriver?.MarkDirty();
        if (!_initializationFailureLogged)
        {
            Logger.LogError(
                "共享仓库初始化失败；库存绑定已回滚，Mod 将继续重试：" + error);
            _initializationFailureLogged = true;
        }
    }
}

private void ResetCoordinatorSession()
{
    _coordinator.ResetSession();
    _initializationFailureLogged = false;
}

private void EndCurrentSession()
{
    try
    {
        _sessionDriver?.Reset(ResetCoordinatorSession);
    }
    catch (Exception error)
    {
        Logger.LogError("清理上一局共享库存状态失败：" + error);
    }
}
```

删除旧字段 `_activeManager`、`_initializedForManager` 及旧 `EndCurrentSession` 实现。

- [ ] **Step 5: 运行回归测试并确认 GREEN**

Run the command from Step 2.

Expected: PASS，2 tests passed。

- [ ] **Step 6: 运行全部自动测试**

```powershell
dotnet test .\SharedWarehouse.sln -c Debug --no-restore /p:InstallAfterBuild=false
```

Expected: PASS，原有 19 tests 加新增 7 tests，共 26 tests passed，0 failed。

同时确认现有 `SaveSingleViewPatch` 仍以 `PlayDataMgr.Save` 为目标，库存汇总、嵌套单仓视图和异常恢复测试没有回归。

- [ ] **Step 7: 提交生命周期修复**

```powershell
git add src/SharedWarehouse/Plugin.cs src/SharedWarehouse/Patches/RuntimeSessionPatch.cs tests/SharedWarehouse.Tests/RuntimePatchContractTests.cs
git commit -m "fix: initialize from game runtime lifecycle"
```

---

### Task 3: 版本、文档与发布包

**Files:**
- Modify: `src/SharedWarehouse/Plugin.cs:15`
- Modify: `src/SharedWarehouse/SharedWarehouse.csproj:6-8`
- Modify: `README.md`
- Modify: `NEXUS_DESCRIPTION.md`
- Modify: `docs/TESTING.md`
- Modify: `scripts/Package.ps1:10`
- Modify: `tests/SharedWarehouse.Tests/RuntimePatchContractTests.cs`

**Interfaces:**
- Consumes: Task 2 的运行时修复。
- Produces: v0.1.1 程序集、说明和 `dist/共享仓库-v0.1.1-BepInEx5.zip`。

- [ ] **Step 1: 写入版本一致性失败测试**

在 `RuntimePatchContractTests` 顶部增加 `using System;`，并增加：

```csharp
[Fact]
public void Plugin_and_assembly_report_version_0_1_1()
{
    Assert.Equal("0.1.1", Plugin.PluginVersion);
    Assert.Equal(new Version(0, 1, 1, 0), typeof(Plugin).Assembly.GetName().Version);
}
```

- [ ] **Step 2: 运行版本测试并确认 RED**

```powershell
dotnet test .\tests\SharedWarehouse.Tests\SharedWarehouse.Tests.csproj -c Debug --no-restore /p:InstallAfterBuild=false --filter "FullyQualifiedName~Plugin_and_assembly_report_version_0_1_1"
```

Expected: FAIL，实际为 `0.1.0`。

- [ ] **Step 3: 更新插件与程序集版本**

```csharp
public const string PluginVersion = "0.1.1";
```

```xml
<Version>0.1.1</Version>
<AssemblyVersion>0.1.1.0</AssemblyVersion>
<FileVersion>0.1.1.0</FileVersion>
```

- [ ] **Step 4: 更新发布文字**

- `README.md` 与 `NEXUS_DESCRIPTION.md` 中 Mod 版本改为 `0.1.1`。
- 故障排查增加：完成读档后必须出现“共享仓库初始化完成”。
- `docs/TESTING.md` 增加两个新测试类，并将预期 Harmony 补丁组数量改为 7。
- `scripts/Package.ps1` 使用：

```powershell
$archivePath = Join-Path $distDir '共享仓库-v0.1.1-BepInEx5.zip'
```

- [ ] **Step 5: 运行版本测试并确认 GREEN**

Run the command from Step 2.

Expected: PASS，1 test passed。

- [ ] **Step 6: 运行 Release 测试与构建但不安装**

```powershell
dotnet test .\SharedWarehouse.sln -c Release --no-restore /p:InstallAfterBuild=false
dotnet build .\src\SharedWarehouse\SharedWarehouse.csproj -c Release --no-restore /p:InstallAfterBuild=false /p:RatopiaDir="E:\steam\steamapps\common\Ratopia"
```

Expected: 两条命令均 exit code 0；27 tests passed；0 warnings；0 errors。

- [ ] **Step 7: 生成并检查 Nexus Mods 包**

```powershell
.\scripts\Package.ps1 -RatopiaDir "E:\steam\steamapps\common\Ratopia"
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::OpenRead(
    (Resolve-Path '.\dist\共享仓库-v0.1.1-BepInEx5.zip'))
try { $zip.Entries.FullName | Sort-Object } finally { $zip.Dispose() }
```

Expected: 包含 `BepInEx/plugins/SharedWarehouse/SharedWarehouse.dll`、三个说明/启动文件，且不含游戏 DLL。

- [ ] **Step 8: 提交发布更新**

```powershell
git add src/SharedWarehouse/Plugin.cs src/SharedWarehouse/SharedWarehouse.csproj tests/SharedWarehouse.Tests/RuntimePatchContractTests.cs README.md NEXUS_DESCRIPTION.md docs/TESTING.md scripts/Package.ps1
git commit -m "release: prepare shared warehouse 0.1.1"
```

---

### Task 4: 安装与游戏内验收

**Files:**
- Read: `src/SharedWarehouse/bin/Release/net472/SharedWarehouse.dll`
- Backup: `backups/runtime-fix-0.1.0/SharedWarehouse.dll`
- Install: `E:/steam/steamapps/common/Ratopia/BepInEx/plugins/SharedWarehouse/SharedWarehouse.dll`
- Read: `C:/Users/ASUS/AppData/LocalLow/CasselGames/Ratopia/Player.log`

**Interfaces:**
- Consumes: Task 3 的 Release DLL。
- Produces: 已安装的 v0.1.1 与运行日志/游戏内验收证据。

- [ ] **Step 1: 执行完整验证门禁**

```powershell
dotnet test .\SharedWarehouse.sln -c Release --no-restore /p:InstallAfterBuild=false
dotnet build .\src\SharedWarehouse\SharedWarehouse.csproj -c Release --no-restore /p:InstallAfterBuild=false /p:RatopiaDir="E:\steam\steamapps\common\Ratopia"
git status --short
```

Expected: 27 tests passed；构建 0 warnings、0 errors；工作树干净。

- [ ] **Step 2: 备份当前 DLL**

```powershell
$projectRoot = 'D:\SOFTWARE\项目\鼠托邦mod\SharedWarehouse'
$installed = 'E:\steam\steamapps\common\Ratopia\BepInEx\plugins\SharedWarehouse\SharedWarehouse.dll'
$backupDir = Join-Path $projectRoot 'backups\runtime-fix-0.1.0'
New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
Copy-Item -LiteralPath $installed -Destination (Join-Path $backupDir 'SharedWarehouse.dll') -Force
```

Expected: 备份和原安装文件都存在。

- [ ] **Step 3: 安装 v0.1.1 并验证哈希**

```powershell
$built = 'D:\SOFTWARE\项目\鼠托邦mod\SharedWarehouse\src\SharedWarehouse\bin\Release\net472\SharedWarehouse.dll'
$installed = 'E:\steam\steamapps\common\Ratopia\BepInEx\plugins\SharedWarehouse\SharedWarehouse.dll'
Copy-Item -LiteralPath $built -Destination $installed -Force
$builtHash = (Get-FileHash -LiteralPath $built -Algorithm SHA256).Hash
$installedHash = (Get-FileHash -LiteralPath $installed -Algorithm SHA256).Hash
if ($builtHash -ne $installedHash) { throw '安装后的 DLL 哈希不一致。' }
$builtHash
```

Expected: 输出 SHA256 且没有异常。

- [ ] **Step 4: 启动游戏并执行人工验收**

```powershell
Start-Process -FilePath 'E:\steam\steamapps\common\Ratopia\Ratopia.exe' -WorkingDirectory 'E:\steam\steamapps\common\Ratopia'
```

载入任意存档；检查普通仓库和迷你仓库显示 `/∞`；在一座仓库存取一个易识别材料并在另一座确认即时变化；新建一座目标仓库并确认共享；记录一个材料总数并手动保存后退出。重新启动并载入刚保存的存档，确认该材料总数不因仓库数量倍增，再退出游戏。

- [ ] **Step 5: 检查本次运行日志**

```powershell
$log = 'C:\Users\ASUS\AppData\LocalLow\CasselGames\Ratopia\Player.log'
rg -n --no-heading 'Loading \[共享仓库 0\.1\.1\]|RuntimeSessionPatch|共享仓库初始化完成|共享仓库初始化失败|Harmony 补丁安装失败|Exception|Error|Fatal' $log
```

Expected: 加载 v0.1.1；安装 `RuntimeSessionPatch`；每次完成读档后恰好一次初始化成功；没有 Mod 初始化失败。

- [ ] **Step 6: 核对插件目录**

```powershell
Get-ChildItem -LiteralPath 'E:\steam\steamapps\common\Ratopia\BepInEx\plugins' -File -Filter '*.dll' -Recurse | Select-Object FullName
Get-ChildItem -LiteralPath 'E:\steam\steamapps\common\Ratopia\BepInEx\patchers' -File -Filter '*.dll' -Recurse | Select-Object FullName
```

Expected: plugins 只有 `SharedWarehouse.dll`；patchers 没有第三方 DLL。

- [ ] **Step 7: 最终核对交付物**

```powershell
Get-Item -LiteralPath 'D:\SOFTWARE\项目\鼠托邦mod\SharedWarehouse\dist\共享仓库-v0.1.1-BepInEx5.zip','E:\steam\steamapps\common\Ratopia\BepInEx\plugins\SharedWarehouse\SharedWarehouse.dll' | Select-Object FullName,Length,LastWriteTime
git -C 'D:\SOFTWARE\项目\鼠托邦mod\SharedWarehouse' log -4 --oneline
git -C 'D:\SOFTWARE\项目\鼠托邦mod\SharedWarehouse' status --short
```

Expected: 发布包和已安装 DLL 都存在；提交覆盖设计、会话驱动、生命周期修复和 v0.1.1 发布；工作树干净。
