# 脚手架

“脚手架”是一个用于《鼠托邦》（Ratopia）的 BepInEx 5 Mono Mod。它新增独立建造项目“脚手架”，不替换或修改原版梯子。

## 功能

- 每格消耗 `1` 个木板，点击后瞬间搭建，不创建搬运或施工任务。
- 可以无支撑地放在空中、水中、植物位置以及任意建筑格，包括门和路障。
- 不允许覆盖实心地形、矿物、原版梯子或另一脚手架。
- 脚手架提供正常的梯子寻路节点；与门或路障同格时，鼠民可以借此越过其阻挡。
- 完成后存在准确的 `7200` 游戏分钟，也就是完整 `5` 个游戏日；暂停期间计时不前进。
- 到期时静默拆除，并在原位置生成 `1` 个木板。
- 原版拆除工具可点选或框选脚手架，立即拆除并返还木板，不派遣鼠民。
- 脚手架和建筑同格时，本次拆除只处理脚手架，不会向底层建筑下达拆除命令。
- 选中脚手架时会显示剩余天数和小时；不足一小时显示“剩余不足1小时”。

## 安装

1. 完全退出 Ratopia。
2. 确认游戏已安装 BepInEx 5。
3. 将 `脚手架-v0.1.0-BepInEx5.zip` 解压到游戏根目录。
4. 最终文件应位于：

   ```text
   Ratopia\BepInEx\plugins\Scaffold\Scaffold.dll
   Ratopia\BepInEx\plugins\Scaffold\Data\world.png
   Ratopia\BepInEx\plugins\Scaffold\Data\menu.png
   Ratopia\BepInEx\plugins\Scaffold\Data\blueprint.png
   ```

5. 启动游戏并检查 `BepInEx\LogOutput.log`，应出现“脚手架 0.1.0 已加载”。

## 存档与卸载

脚手架记录保存在原版预留的 `D_Data.ModsData` 中，键为：

```text
cn.ratopia.scaffold.instances.v1
```

保存原版地图时，Mod 会写入脚手架下方真实节点类型，不会把临时 `Ladder` 覆盖写进原版地图数据。因此停用 Mod 后存档仍可读取，也不会留下隐形梯子。

永久卸载前，建议先进入每个相关存档，手动拆除所有脚手架并正常保存，以取回已经消耗的木板。若直接卸载，原版会忽略未知的 `ModsData` 键，但未拆除脚手架所消耗的木板不会由原版自动返还。

## 环境与兼容性

- Mod 版本：`0.1.0`
- 插件标识：`cn.ratopia.scaffold`
- 目标游戏版本：Ratopia `1.0.0600`
- 目标运行时：BepInEx `5.4.23.5`、Harmony `2.9.0.0`、Mono / `.NET Framework 4.7.2`
- 已验证 `Assembly-CSharp.dll` SHA-256：`C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`

游戏更新导致该哈希变化时，应先重新反编译并运行程序集契约测试。Mod 的库存检查和扣除均调用原版 `BuildingMgr` 接口，因此可沿用 SharedWarehouse 对这些接口的 Harmony 兼容补丁。

素材缺失时，Mod 会回退到原版梯子图像并写入警告日志；功能仍可运行。

## 从源码构建

```powershell
$env:RATOPIA_DIR = 'E:\steam\steamapps\common\Ratopia'
dotnet test .\Scaffold.sln -c Release /p:InstallAfterBuild=false
.\Package.ps1 -RatopiaDir $env:RATOPIA_DIR
```

发布包生成于 `dist\脚手架-v0.1.0-BepInEx5.zip`。游戏、Unity、BepInEx、Harmony、SavableData 和测试运行时 DLL 均只作为引用，不会打包。

完整验收步骤见 `docs\TESTING.md`。
