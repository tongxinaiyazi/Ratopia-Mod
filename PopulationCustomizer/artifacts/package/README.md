# 人口自定义

“人口自定义”是一个用于《鼠托邦》（Ratopia）的 BepInEx 5 Mono Mod。点击原版人口数量进入鼠民名单后，可通过名单顶部放大镜按钮左侧的“上限”按钮，为每个存档分别设置鼠民与机器鼠的数量上限。

## 功能

- 鼠民和机器鼠可分别启用自定义上限，允许范围为 `0–999`。
- 手动编辑某一项数值时会自动勾选该项“自定义”，避免数字已输入但仍沿用原版上限。
- 第一次进入存档时保持原版上限，只有点击“应用到当前存档”后才启用自定义值。
- 设置立即影响原版招募、制造判定和数量显示，但需要玩家正常保存游戏后才会写入磁盘。
- 如果当前数量已经超过新上限，Mod 不会删除或赶走任何现有单位，只会停止继续新增。
- “恢复原版”会移除当前存档中属于本 Mod 的设置，并重新使用繁荣度、建筑和遗物提供的原版动态上限。

较高人口会显著增加 CPU、寻路和存档负担。即使允许输入 999，也请根据电脑性能逐步提高。

## 使用

1. 进入已有或新建存档。
2. 点击原版人口数量，打开鼠民名单。
3. 点击名单顶部、放大镜按钮左侧的“上限”按钮。
4. 输入 `0–999` 的整数；编辑数值会自动勾选对应的“自定义”，也可以手动切换。
5. 点击“应用到当前存档”。设置立即生效。
6. 使用原版保存功能正常保存游戏，使设置写入该存档。

输入 `0` 表示不允许继续新增该类单位。关闭面板或按 `Esc` 会放弃尚未应用的输入。

## 每个存档的设置

配置保存在原版预留的 `D_Data.ModsData` 中，键为：

```text
cn.ratopia.populationcustomizer.settings
```

每个存档都有自己的设置，复制存档时设置会随存档一起复制。首次使用、键缺失或数据损坏时，Mod 会安全回退原版上限。它不会修改鼠民或机器鼠列表，也不会自动调用原版保存。

## 安装

1. 完全退出 Ratopia。
2. 确认已经安装 BepInEx 5。
3. 将发布包解压到游戏根目录，最终文件应位于：
   `Ratopia\BepInEx\plugins\PopulationCustomizer\PopulationCustomizer.dll`
4. 启动游戏并检查 `BepInEx\LogOutput.log`，应出现“人口自定义 v0.1.3 已加载”。进入存档后还会明确打印从该存档读取到的两项上限。

## 恢复原版与卸载

若希望彻底清除某个存档的设置，先进入该存档，点击“恢复原版”，再正常保存游戏。

卸载时完全退出游戏，然后删除：

```text
BepInEx\plugins\PopulationCustomizer\PopulationCustomizer.dll
```

即使未先清除设置，原版也会忽略 `ModsData` 中未知的 Mod 键；重新安装本 Mod 后，该存档设置会再次生效。重要存档仍建议在安装、更新和卸载前备份。

## 环境与兼容性

- Mod 版本：`0.1.3`
- 目标：BepInEx `5.4.23.5`、Harmony `2.9.0`、Mono / `.NET Framework 4.7.2`
- 已检查的 `Assembly-CSharp.dll` SHA-256：`C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`

游戏更新导致该哈希变化后，请先重新运行合同测试，不要直接假定补丁仍兼容。本 Mod 只替换两个统一上限接口，并在 `StatisticsCitizenListUI.Initialize()` 后创建设置入口，不使用 IL transpiler；已安装的 Ratopia Citizen List Update Mod 修改的是 `CitizenCaveUI` 招募列表，不是相同接口。

## 从源码构建

```powershell
$env:RATOPIA_DIR = '<Ratopia目录>'
dotnet test .\PopulationCustomizer.sln -c Release /p:InstallAfterBuild=false
dotnet build .\src\PopulationCustomizer\PopulationCustomizer.csproj -c Release /p:InstallAfterBuild=false
.\scripts\Package.ps1 -RatopiaDir $env:RATOPIA_DIR
```

生成的发布包为 `dist\人口自定义-v0.1.3-BepInEx5.zip`。发布包不包含游戏、Unity、BepInEx、Harmony、SavableData 或测试运行时 DLL。

完整验收步骤见 `docs\TESTING.md`。
