# 广播站信号覆盖全图

适用于《鼠托邦（Ratopia）》的 BepInEx 5 Mono Mod。让地图任意位置的电视都能发现并使用工作的广播站。

本 Mod 只接管电视查找广播信号源的两个入口。电视仍按原版方式服务附近居民，不改变居民使用电视的服务距离，也不修改广播站的电路范围、`BuildInfo.Range` 或 `Building.m_Range`。广播站原版范围设置保留，用于电路连接。

## 版本与要求

- Mod：`0.1.1`
- GUID：`cn.ratopia.broadcaststationglobalcoverage`
- 程序集：`BroadcastStationGlobalCoverage.dll`
- 运行环境：BepInEx 5.4.23.5、Mono、Harmony 2.9.0
- 已检查的 `Assembly-CSharp.dll` SHA-256：`C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`

目标游戏更新后，如果上述哈希不一致，请先停用 Mod，并等待兼容性确认。

## 安装

1. 先安装 BepInEx 5.4.23.5，并至少运行一次游戏。
2. 退出游戏。
3. 解压发布包到游戏根目录，最终路径应为：
   `BepInEx/plugins/BroadcastStationGlobalCoverage/BroadcastStationGlobalCoverage.dll`
4. 启动游戏，在 `BepInEx/LogOutput.log` 中搜索 `广播站信号覆盖全图`。

## 卸载

退出游戏后，删除目录 `BepInEx/plugins/BroadcastStationGlobalCoverage`。本 Mod 不写配置文件，也不修改或接管范围存档字段；移除 DLL 后原版可以直接读取存档。

## 存档安全

- 不修改广播站数据库范围或建筑实例范围。
- 不修改 `BuildingData.m_Range`，也不补丁保存/读档方法。
- 手动选台时仅补齐全图广播站候选；自动选台时仅选择最近的工作广播站。
- 广播站电路查找继续使用原版范围。
- 建议任何 Mod 安装或卸载前备份重要存档。

## 兼容性与冲突

本 Mod 不修改全局 `Defines.m_MaxCustomBuildingRange`、广播站范围、居民娱乐检索或电视服务范围。它可能与其他修改以下内容的 Mod 冲突：

- `UI_StorageSelect.TelevisionSelectSet` 的电视手动选台候选；
- `Building_ElecBandstand.Building_Update2` 的电视自动选台逻辑。

若任一 Harmony 补丁安装失败，本 Mod 会撤销自己的全部补丁并停用功能；详情见 `BepInEx/LogOutput.log`。

## 人工验收

建议在游戏内完成以下人工验收：

1. 新建与载入已有广播站，确认范围设置与电路范围保持原版。
2. 保存、退出，连续读档两次，确认广播站仍正常工作。
3. 在地图远端放置电视，确认可以指定并使用正在工作的广播站。
4. 确认电视附近居民的原版使用距离没有变大。
5. 确认远距离电池不会因为本 Mod 自动连接广播站。
6. 使用 Mod 保存后退出，临时移走 DLL，再次读档，确认存档可读。

## 从源码构建

需要 .NET SDK 和已安装 BepInEx 5 的鼠托邦目录：

```powershell
$env:RATOPIA_DIR = 'E:\steam\steamapps\common\Ratopia'
dotnet test .\BroadcastStationGlobalCoverage.sln -c Release /p:InstallAfterBuild=false
.\scripts\Package.ps1 -RatopiaDir $env:RATOPIA_DIR
```

Release 构建默认不会自动安装；只有显式传入 `/p:InstallAfterBuild=true` 才会复制 DLL。

## 更新记录

### 0.1.1

- 修复广播站信号覆盖全图时，电路连接范围也被错误扩大到全图的问题。
- 不再修改任何建筑范围字段；恢复广播站原版范围调节入口和电路连接距离。
- 全图效果仅作用于电视的手动选台与自动信号源选择。
