# YunQingAll（BepInEx 5 迁移版）

这是从 `Program.cs` 迁移得到的《鼠托邦》（Ratopia）BepInEx 5 Mono 模组。迁移版保留原插件 ID、版本、配置键、F9 控制面板和三组可见功能。

## 兼容目标

- 插件 ID：`RatopiaMod.YunQing.YunQingAll`
- 插件名称：`YunQingAll`
- 插件版本：`2.2.0`
- 目标加载器：BepInEx `5.4.23.5`
- 目标 Harmony：`2.9.0`
- 目标运行时：Mono / `.NET Framework 4.7.2`
- 已检查的 `Assembly-CSharp.dll` SHA-256：`C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`

游戏更新后如果程序集哈希发生变化，应先重新检查 Harmony 目标，不能直接假定仍然兼容。

## 迁移说明

收到的唯一源码是 `Program.cs`。源码引用了未提供的 `CheatPanelLocalizer`、`ExchangeRateMode` 和 `BankExchangeMultiplier`。

- 两个枚举已根据原代码中的配置值和分支完整重建。
- `CheatPanelLocalizer` 无法可靠还原，因此迁移版不包含它的额外本地化功能。
- 原源码内已有的中文 F9 控制面板不受影响。
- 鱼功能严格保留原代码行为：开启后，每次进入 `Fish.DrownCheck` 或 `Monkfish.DrownCheck` 都执行 `BeAttacked(-5f)` 并跳过原方法。

## 功能与配置

默认按 `F9` 打开或关闭控制面板。配置保存在 BepInEx 的：

`BepInEx\config\RatopiaMod.YunQing.YunQingAll.cfg`

保留的配置键：

- `Common.IsActiveFishDrownInTheWater = true`
- `Common.CustomExchangeRateMode = COMMON`
- `Common.BankExchangeMultiplier = X1`
- `GUI.GuiToggleKey = F9`

可用汇率模式：正汇率、正汇率最大值、官方正常值、负汇率、负汇率最大值。

可用银行倍率：`x1`、`x10`、`x100`、`x500`。

## 安装（仅供人工测试者）

本迁移任务没有自动安装或启动游戏。需要实机测试时：

1. 完全退出 Ratopia。
2. 确认游戏使用 BepInEx 5，而不是 BepInEx 6。
3. 移除或停用同 GUID 的原 YunQingAll DLL，不能同时加载两个版本。
4. 把发布包直接解压到 Ratopia 游戏根目录。
5. 最终 DLL 路径应为：
   `Ratopia\BepInEx\plugins\YunQingAll\YunQingAll.dll`

## 人工验收清单

本包只完成自动、静态、构建和包结构测试，尚未进行游戏内运行验证。测试者应依次确认：

1. `BepInEx\LogOutput.log` 出现 `Loading [YunQingAll 2.2.0]`。
2. 日志显示四个 Harmony 补丁分别安装完成，没有回滚或停用信息。
3. 触发相关玩法后，日志分别出现四个目标方法的“补丁首次执行”证据。
4. 进入新建或专用测试存档，按 F9 能打开、关闭中文面板。
5. 关闭鱼功能时保持原版 `DrownCheck`；开启时分别观察 Fish 和 Monkfish 的原代码伤害行为。
6. 五种汇率模式分别生成预期汇率券，`COMMON` 保持游戏原始结果。
7. 银行兑换值分别按 `x1`、`x10`、`x100`、`x500` 变化。
8. 退出并重启游戏，确认四项 BepInEx 配置能够保持。
9. 保存、退出、重新读档两轮，确认没有补丁异常或配置倍增。
10. 在游戏关闭后移除 DLL，确认专用测试存档仍可由原版读取。

插件不写入自定义存档字段，但使用插件完成的游戏内交易和数值变化仍会被游戏按原规则保存。请只用可回滚的测试存档进行首次验收。

## 日志与卸载

主要日志：

- `Ratopia\BepInEx\LogOutput.log`
- `%USERPROFILE%\AppData\LocalLow\CasselGames\Ratopia\Player.log`

卸载时先完全退出游戏，再删除：

`Ratopia\BepInEx\plugins\YunQingAll\YunQingAll.dll`

如不再需要旧配置，可另外删除对应 CFG；保留 CFG 不会让模组在 DLL 缺失时运行。

## 从源码构建与打包

```powershell
$env:RATOPIA_DIR = '<Ratopia游戏目录>'
dotnet test .\YunQingAll.sln -c Release /p:InstallAfterBuild=false
dotnet build .\src\YunQingAll\YunQingAll.csproj -c Release /p:InstallAfterBuild=false
.\scripts\Package.ps1 -RatopiaDir $env:RATOPIA_DIR
```

发布包输出：`dist\YunQingAll-v2.2.0-BepInEx5.zip`。

发布包不会包含游戏、Unity、BepInEx、Harmony、测试 DLL、PDB、日志或存档。
