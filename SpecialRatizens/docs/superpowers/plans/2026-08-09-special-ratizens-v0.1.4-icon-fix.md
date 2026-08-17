# 特殊鼠鼠 v0.1.4 能力图标修复实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Ratopia Mod 工作禁止使用子代理。Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 让全部 24 个特殊能力在特性卡、状态栏和飘字中使用各自外部 PNG，同时保持普通鼠和原版特性图标不变。

**Architecture:** 为每个特性建立 `SpecialRatizens.Icon.<trait-name>` 独立主键，并在确认游戏数据库索引所有权后更新 `Icon_Char{Index}` 兼容别名。注册从插件实际 `Data/Icon` 目录直接读取 PNG；`RefInfo` 和 `BuffIcon` 统一消费主键，游戏中写死索引图标的 UI 消费兼容别名。

**Tech Stack:** C# / .NET Framework 4.7.2、BepInEx 5.4.23.5、Harmony 2.9.0、Unity Mono、xUnit、Mono.Cecil、PowerShell。

## Global Constraints

- 目标游戏固定为 `E:\steam\steamapps\common\Ratopia`，Ratopia 1.0.0600 Mono。
- 只修改独立模组 `D:\SOFTWARE\项目\鼠托邦mod\SpecialRatizens`；不修改原始 RAR。
- 不改变 CSV 顺序、特性索引、效果数值、皮肤、角色或存档结构。
- 新主键必须以 `SpecialRatizens.Icon.` 开头；索引别名只能在数据库确认该索引属于同一自定义特性后更新。
- 先测试和构建，Ratopia 关闭且备份完成后才能安装；不自动启动游戏、不自动保存。
- 项目不是 Git 仓库，所有“提交”检查点改为文件清单和测试证据检查点。

## File Map

- Create `src/SpecialRatizens/Core/CustomIconKeys.cs`: 纯 C# 图标主键和索引别名生成器。
- Create `tests/SpecialRatizens.Tests/CustomIconKeysTests.cs`: 主键、别名、非法输入单元测试。
- Create `tests/SpecialRatizens.Tests/IconRegistrationContractTests.cs`: 外部路径、幂等双键、索引所有权和状态栏消费合同。
- Modify `src/SpecialRatizens/Legacy/CustomMOD.cs`: 加载实际 PNG、注册双键、状态栏使用主键。
- Modify `tests/SpecialRatizens.Tests/GameContractTests.cs`: 固定游戏 UI 对 `Func.LoadSprite`、主键地址和索引别名的消费方式。
- Modify `tests/SpecialRatizens.Tests/PluginContractTests.cs`: v0.1.4 插件版本合同。
- Modify `tests/SpecialRatizens.Tests/PackagingContractTests.cs`: v0.1.4 程序集、README 和包名合同。
- Modify `src/SpecialRatizens/Plugin.cs`: 插件版本 0.1.4。
- Modify `src/SpecialRatizens/SpecialRatizens.csproj`: 程序集版本 0.1.4.0。
- Modify `scripts/Package.ps1`: v0.1.4 压缩包名称。
- Modify `README.md`: v0.1.4 图标修复和实机验收说明。

---

### Task 1: 建立图标键与迁移缺陷的失败测试

**Files:**
- Create: `src/SpecialRatizens/Core/CustomIconKeys.cs`（本任务只在 GREEN 阶段创建）
- Create: `tests/SpecialRatizens.Tests/CustomIconKeysTests.cs`
- Create: `tests/SpecialRatizens.Tests/IconRegistrationContractTests.cs`

**Interfaces:**
- Consumes: 现有 `CustomMOD.RegisterCustomInfoIcon(CharacterInfo)`、`CustomMOD.BuffIcon_IconSet(BuffIcon, BuffInfo)` 源码。
- Produces: `CustomIconKeys.ForTrait(string): string` 与 `CustomIconKeys.ForCharacterIndex(int): string` 的测试合同；生产代码尚不实现。

- [ ] **Step 1: 写纯逻辑失败测试**

```csharp
using System;
using SpecialRatizens.Core;
using Xunit;

namespace SpecialRatizens.Tests
{
    public sealed class CustomIconKeysTests
    {
        [Fact]
        public void TraitKeyUsesAnIsolatedStableNamespace()
        {
            Assert.Equal("SpecialRatizens.Icon.HT_WQX", CustomIconKeys.ForTrait("HT_WQX"));
        }

        [Fact]
        public void CharacterIndexKeyMatchesRatopiaUiConvention()
        {
            Assert.Equal("Icon_Char153", CustomIconKeys.ForCharacterIndex(153));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TraitKeyRejectsMissingNames(string value)
        {
            Assert.Throws<ArgumentException>(() => CustomIconKeys.ForTrait(value));
        }

        [Fact]
        public void CharacterIndexKeyRejectsNegativeIndexes()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CustomIconKeys.ForCharacterIndex(-1));
        }
    }
}
```

- [ ] **Step 2: 写迁移缺陷合同测试**

`IconRegistrationContractTests.cs` 读取 `CustomMOD.cs` 中两个方法体并断言：

```csharp
[Fact]
public void RegistrationUsesTheConfiguredPluginIconDirectory()
{
    var body = MethodBody("static void RegisterCustomInfoIcon(CharacterInfo info)");
    Assert.Contains("Path.Combine(CustomDataPath, \"Icon\"", body);
    Assert.DoesNotContain("CustomSetting_Data/Icon", body);
}

[Fact]
public void RegistrationUsesAnOwnedIdempotentPrimaryAndIndexKey()
{
    var body = MethodBody("static void RegisterCustomInfoIcon(CharacterInfo info)");
    Assert.Contains("CustomIconKeys.ForTrait(info.Name)", body);
    Assert.Contains("CustomIconKeys.ForCharacterIndex(info.Index)", body);
    Assert.Contains("DBMgr.GetCharacterInfo(info.Index)", body);
    Assert.Contains("sprites[iconKey] = sprite", body);
    Assert.Contains("sprites[indexKey] = sprite", body);
    Assert.DoesNotContain("if (DicSprits.ContainsKey(spriteKey))", body);
    Assert.True(body.IndexOf("DBMgr.GetCharacterInfo(info.Index)", StringComparison.Ordinal) <
                body.IndexOf("sprites[indexKey] = sprite", StringComparison.Ordinal));
}

[Fact]
public void BuffIconUsesTheRegisteredCustomTraitKey()
{
    var body = MethodBody("public static void BuffIcon_IconSet(BuffIcon __instance, BuffInfo _info)");
    Assert.Contains("TryGetCustomCharInfo(_info.ReferenceName, out CustomCharInfo customInfo)", body);
    Assert.Contains("Func.Instance.LoadSprite(customInfo.iconKey)", body);
    Assert.DoesNotContain("$\"Icon_{_info.Name}\"", body);
}
```

辅助 `MethodBody` 和 `GetProjectRoot` 复制 `EffectSafetyContractTests` 的平衡花括号实现，不依赖运行 Unity。

- [ ] **Step 3: 运行 RED 测试**

Run:

```powershell
dotnet test .\SpecialRatizens.sln -c Release /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false --filter "FullyQualifiedName~CustomIconKeysTests|FullyQualifiedName~IconRegistrationContractTests" --verbosity minimal
```

Expected: 编译失败，提示 `CustomIconKeys` 不存在。该失败证明纯策略合同能够阻止缺失实现进入发布；不要提前修改生产 `CustomMOD`。

- [ ] **Step 4: 记录 RED 检查点**

记录失败测试名称和原因；确认失败来自缺少新图标策略及现有错误实现，而不是路径拼写或测试编译错误。

---

### Task 2: 实现纯图标键策略

**Files:**
- Create: `src/SpecialRatizens/Core/CustomIconKeys.cs`
- Test: `tests/SpecialRatizens.Tests/CustomIconKeysTests.cs`

**Interfaces:**
- Consumes: 非空特性内部名、非负 `CharacterInfo.Index`。
- Produces: `internal static string ForTrait(string traitName)`；`internal static string ForCharacterIndex(int index)`。

- [ ] **Step 1: 写最小实现**

```csharp
using System;

namespace SpecialRatizens.Core
{
    internal static class CustomIconKeys
    {
        private const string TraitPrefix = "SpecialRatizens.Icon.";

        internal static string ForTrait(string traitName)
        {
            if (string.IsNullOrWhiteSpace(traitName))
                throw new ArgumentException("特殊能力名称不能为空。", nameof(traitName));

            return TraitPrefix + traitName.Trim();
        }

        internal static string ForCharacterIndex(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "特性索引不能为负数。");

            return $"Icon_Char{index}";
        }
    }
}
```

- [ ] **Step 2: 运行纯逻辑测试**

Run: 与 Task 1 Step 3 相同，但过滤仅保留 `CustomIconKeysTests`。

Expected: 6 个测试用例通过，0 失败。

- [ ] **Step 3: 运行迁移合同确认第二阶段 RED**

Run: Task 1 Step 3 的命令，但过滤仅保留 `IconRegistrationContractTests`。

Expected: 旧 `CustomMOD` 因 `CustomSetting_Data/Icon`、遇到已有 `Icon_Char` 就返回，以及状态栏读取 `Icon_{_info.Name}` 而失败。

- [ ] **Step 4: 检查点**

确认新文件不引用 Unity、BepInEx、Harmony 或游戏程序集；不修改任何已有运行时行为。

---

### Task 3: 从真实 Data/Icon 注册双键并修复状态栏

**Files:**
- Modify: `src/SpecialRatizens/Legacy/CustomMOD.cs` 的 `RegisterCustomInfoIcon` 与 `BuffIcon_IconSet`
- Test: `tests/SpecialRatizens.Tests/IconRegistrationContractTests.cs`

**Interfaces:**
- Consumes: `CustomIconKeys.ForTrait`、`CustomIconKeys.ForCharacterIndex`、`CustomDataPath`、`CustomCharInfo.iconAddress`、`Func.Dic_Resource`。
- Produces: `customInfo.iconKey` 独立主键；主键和已验证索引别名指向同一 `Sprite`。

- [ ] **Step 1: 替换图标注册方法**

```csharp
static void RegisterCustomInfoIcon(CharacterInfo info)
{
    if (info == null || !TryGetCustomCharInfo(info.Name, out CustomCharInfo customInfo))
        throw new InvalidDataException("注册特殊能力图标时找不到特性数据。");

    string iconKey = CustomIconKeys.ForTrait(info.Name);
    string indexKey = CustomIconKeys.ForCharacterIndex(info.Index);
    customInfo.iconKey = iconKey;

    string spriteName = customInfo.iconAddress;
    if (string.IsNullOrWhiteSpace(spriteName))
        throw new InvalidDataException($"特殊能力 {info.Name} 的图标地址为空。");
    string iconPath = Path.Combine(CustomDataPath, "Icon", $"{spriteName}.png");
    Sprite sprite = BaseCommand.LoadSpriteFromTexture2D(BaseCommand.LoadTextureFromFile(iconPath));
    if (sprite == null)
        throw new InvalidDataException($"特殊能力 {info.Name} 图标加载失败：{iconPath}");

    CharacterInfo indexedInfo = DBMgr.GetCharacterInfo(info.Index);
    if (indexedInfo == null || !string.Equals(indexedInfo.Name, info.Name, StringComparison.Ordinal))
        throw new InvalidDataException(
            $"特殊能力 {info.Name} 的图标索引 {info.Index} 被其他特性占用：{indexedInfo?.Name ?? "<null>"}");

    Dictionary<string, Sprite> sprites = DicSprits;
    if (sprites == null)
        throw new InvalidDataException($"特殊能力 {info.Name} 无法访问游戏图标资源表。");

    sprites[iconKey] = sprite;
    sprites[indexKey] = sprite;
}
```

- [ ] **Step 2: 让状态栏使用主键**

```csharp
public static void BuffIcon_IconSet(BuffIcon __instance, BuffInfo _info)
{
    if (!ActiveCustomSpecialUnit ||
        !TryGetCustomCharInfo(_info.ReferenceName, out CustomCharInfo customInfo))
        return;

    _info.T_Name = _info.ReferenceName;
    __instance.m_Spr.sprite = Func.Instance.LoadSprite(customInfo.iconKey);
}
```

- [ ] **Step 3: 运行合同测试**

Run: Task 1 Step 3 的完整过滤命令。

Expected: `CustomIconKeysTests` 与 `IconRegistrationContractTests` 全部通过。

- [ ] **Step 4: 运行现有数据与补丁回归**

Run:

```powershell
dotnet test .\SpecialRatizens.sln -c Release /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false --filter "FullyQualifiedName~SpecialDataCatalogTests|FullyQualifiedName~ReleaseDataContractTests|FullyQualifiedName~PluginContractTests" --verbosity minimal
```

Expected: 24 个图标引用、24 个运行时特性和 38 个适配入口合同保持通过。

---

### Task 4: 固定真实游戏的图标消费合同

**Files:**
- Modify: `tests/SpecialRatizens.Tests/GameContractTests.cs`

**Interfaces:**
- Consumes: Ratopia `Assembly-CSharp.dll` 当前哈希和 Mono.Cecil。
- Produces: 对状态主键及 `Icon_Char` 索引别名消费链的更新保护。

- [ ] **Step 1: 添加调用合同辅助函数**

新增 `AssertCallsLoadSprite`：选定类型和方法的 IL 必须调用 `Func.LoadSprite(string)`；`requireIconCharLiteral=true` 时还必须包含以 `Icon_Char` 开头的字符串操作数。

```csharp
private static void AssertCallsLoadSprite(
    ModuleDefinition module, string typeName, string methodName, bool requireIconCharLiteral)
{
    var methods = FindType(module, typeName).Methods
        .Where(method => method.Name == methodName && method.HasBody)
        .ToArray();
    Assert.NotEmpty(methods);
    Assert.Contains(methods, method => method.Body.Instructions.Any(instruction =>
        instruction.Operand is MethodReference call &&
        call.DeclaringType.FullName == "Func" && call.Name == "LoadSprite"));
    if (requireIconCharLiteral)
    {
        Assert.Contains(methods, method => method.Body.Instructions.Any(instruction =>
            instruction.Operand is string text && text.StartsWith("Icon_Char", StringComparison.Ordinal)));
    }
}
```

- [ ] **Step 2: 添加消费链测试**

```csharp
[Fact]
public void CustomIconConsumersStillUseTheSharedSpriteRegistry()
{
    using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
    {
        AssertCallsLoadSprite(module, "CitizenBuff/RefInfo", "GetIcon", false);
        AssertCallsLoadSprite(module, "GetEffect", "GetRefEffect", false);
        AssertCallsLoadSprite(module, "CC_CitizenSlot", "SlotSet", true);
        AssertCallsLoadSprite(module, "CharStatusTab", "TabSet", true);
        AssertCallsLoadSprite(module, "Char_Tooltip", "CharInfoSet", true);
        AssertCallsLoadSprite(module, "CasselGames.UI.AbilityBuffSlotUI", "SetData", true);
        AssertCallsLoadSprite(module, "CasselGames.UI.AbilityStatusCitizenSlotUI", "SetData", true);
    }
}
```

- [ ] **Step 3: 运行游戏合同测试**

Run:

```powershell
dotnet test .\SpecialRatizens.sln -c Release /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false --filter "FullyQualifiedName~GameContractTests" --verbosity minimal
```

Expected: 全部通过；该测试固定的是已检查的真实游戏行为，不改变生产代码。

---

### Task 5: 升级 v0.1.4 并更新说明

**Files:**
- Modify: `tests/SpecialRatizens.Tests/PluginContractTests.cs`
- Modify: `tests/SpecialRatizens.Tests/PackagingContractTests.cs`
- Modify: `src/SpecialRatizens/Plugin.cs`
- Modify: `src/SpecialRatizens/SpecialRatizens.csproj`
- Modify: `scripts/Package.ps1`
- Modify: `README.md`

**Interfaces:**
- Consumes: 完成的图标修复和验收范围。
- Produces: 一致的插件、程序集、文档和 ZIP 版本 0.1.4。

- [ ] **Step 1: 先把版本合同改为 0.1.4**

将两个测试文件中所有当前发行版本期望从 `0.1.3`/`0.1.3.0` 改为 `0.1.4`/`0.1.4.0`，包名期望改为 `特殊鼠鼠-v0.1.4-BepInEx5.zip`。

- [ ] **Step 2: 运行版本合同确认 RED**

Run:

```powershell
dotnet test .\SpecialRatizens.sln -c Release /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false --filter "FullyQualifiedName~PluginContractTests|FullyQualifiedName~PackagingContractTests" --verbosity minimal
```

Expected: 版本一致性测试失败，实际仍为 0.1.3。

- [ ] **Step 3: 同步生产版本和 README**

- `Plugin.PluginVersion = "0.1.4"`
- csproj `Version=0.1.4`、`AssemblyVersion=0.1.4.0`、`FileVersion=0.1.4.0`
- 包名 `特殊鼠鼠-v0.1.4-BepInEx5.zip`
- README 标题和启动日志改为 v0.1.4，并增加：真实插件路径、独立主键、索引所有权别名、状态栏主键、24 图标验收和不影响普通特性的说明。

- [ ] **Step 4: 运行版本合同确认 GREEN**

Run: Task 5 Step 2 命令。

Expected: 全部通过。

---

### Task 6: 全量回归、Release 构建和包校验

**Files:**
- Generated: `dist/package/**`
- Generated: `dist/特殊鼠鼠-v0.1.4-BepInEx5.zip`

**Interfaces:**
- Consumes: v0.1.4 源码、README、Data 和 24 PNG。
- Produces: 可直接解压到游戏根目录的 BepInEx 5 包。

- [ ] **Step 1: 跑全量测试**

```powershell
dotnet test .\SpecialRatizens.sln -c Release /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false --verbosity minimal
```

Expected: 0 失败、0 跳过；测试总数大于 v0.1.3 的 64。

- [ ] **Step 2: 执行正式打包**

```powershell
.\scripts\Package.ps1 -RatopiaDir 'E:\steam\steamapps\common\Ratopia'
```

Expected: 测试通过，Release 构建 0 错误；生成 v0.1.4 ZIP。

- [ ] **Step 3: 使用 Ratopia 技能脚本扫描包**

```powershell
& 'C:\Users\ASUS\.codex\skills\developing-ratopia-mods\scripts\Test-RatopiaPackage.ps1' `
  -Path '.\dist\特殊鼠鼠-v0.1.4-BepInEx5.zip' `
  -ExpectedPluginName 'SpecialRatizens'
```

Expected: 插件路径正确，且没有 Assembly-CSharp、UnityEngine、BepInEx、0Harmony、PDB、存档或日志。

- [ ] **Step 4: 逐文件校验 ZIP**

只读比较 `dist/package` 与 ZIP 中每个相对路径和 SHA-256；确认文件数相等、无缺失、无额外、无哈希差异，DLL 程序集和文件版本均为 `0.1.4.0`。

---

### Task 7: 备份 v0.1.3、安装并做最终验收

**Files:**
- Backup: `backups/pre-install-v0.1.4-<timestamp>/Plugin/**`
- Backup: `backups/pre-install-v0.1.4-<timestamp>/SaveFile/**`
- Install: `E:\steam\steamapps\common\Ratopia\BepInEx\plugins\SpecialRatizens\**`

**Interfaces:**
- Consumes: 已验证 `dist/package/BepInEx/plugins/SpecialRatizens`。
- Produces: 游戏目录中的 v0.1.4 和可逐文件回滚的 v0.1.3/存档备份。

- [ ] **Step 1: 安装门禁**

确认 `Get-Process Ratopia` 返回空；枚举 `BepInEx/plugins` 与 `BepInEx/patchers`，确认没有第二份 `SpecialRatizens.dll` 或重复 GUID。记录当前安装版本和哈希。

- [ ] **Step 2: 复制并校验备份**

把当前插件目录复制到 `Plugin`，把 `Ratopia_Data/SaveFile` 复制到 `SaveFile`。用相对路径 + SHA-256 比较源和备份；任何缺失或差异都停止安装。

- [ ] **Step 3: 再次确认进程并逐文件覆盖**

再次确认 Ratopia 未运行；只把暂存插件目录中的文件逐一复制到现有 `SpecialRatizens` 目录，不递归删除游戏或插件根目录。失败时从本次 `Plugin` 备份逐文件回滚。

- [ ] **Step 4: 最终验证**

重新运行全量测试；比较暂存与已安装文件树和 SHA-256；确认安装 DLL 版本 `0.1.4.0`、游戏进程数 0、备份文件数非零。记录 ZIP 和 DLL SHA-256。

- [ ] **Step 5: 交付游戏内验收清单**

要求用户实机检查五禽戏及其余 23 个特性在候选/市民特性卡、状态栏、状态 Tooltip 和获得状态飘字中的图标；抽查普通鼠和原版特性；确认新日志没有 `CustomSetting_Data/Icon`、`load non byte`、空图标键或本插件触发的 `LoadSprite Fail`。不自动启动游戏。
