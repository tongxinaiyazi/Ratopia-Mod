# 电线可穿墙

适用于《鼠托邦》的 BepInEx 5 Mono Mod。它把普通电线（`HeavyWire`）从前景占位中分离，让电线能够和墙、道路及其他建筑共用同一格，同时保留供电连接、施工、取消、拆除和存档重载行为。

当前版本：`0.1.3`

## 功能范围

- 普通电线可以与墙、门、铁路、普通/电动电梯、升降轨道、抽水螺杆、壁纸、道路及其他建筑共格。
- 电池、变电站、发电机、特斯拉塔、电动电梯和线缆道路可与普通电线在同一格保留各自端口并继续供电。
- 其他建筑、特殊设施和道路也可以建在已有普通电线上。
- 支持已建成对象与蓝图的两种放置顺序。
- 拆除重叠格时默认选择前景建筑；按住左 `Alt` 或右 `Alt` 时选择背景普通电线。
- 当电线与建筑重叠时，高亮的建筑与实际交互目标保持一致，`F` 查看详情不再被电线碰撞体抢占。
- 进入存档、切换存档和读档后会重新验证同格电力端口，但不会重写前景节点或重跑全城建筑供电判定。
- 支持道路自带的线缆道路（`Wireroad`）与普通电线（`HeavyWire`）共格。
- 同一格仍不允许重复放置两根普通电线。

“任何地方”指所有建筑、建筑蓝图、墙、道路及电力设施。地图边界、水体、世界对象、地图机关和其他原版无效建造区域仍遵守原版限制。

本 Mod 始终启用，目前没有配置项，也不会写入自定义存档字段。

## 安装

1. 确认游戏使用 Mono 版 BepInEx 5。
2. 解压发布包到《鼠托邦》游戏根目录，并保留包内目录结构。
3. 最终文件应位于：

   `Ratopia/BepInEx/plugins/WireThroughWalls/WireThroughWalls.dll`

4. 启动游戏，在 `BepInEx/LogOutput.log` 中确认出现：

   `电线可穿墙 v0.1.3 已加载`

本版本针对以下本地环境构建和验证：

- 游戏程序集：`Assembly-CSharp.dll`
- SHA-256：`C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`
- BepInEx：`5.4.23.5`
- Harmony：`2.9.0`

游戏更新后如果程序集哈希变化，请先重新验证 Mod，再继续使用。

## 存档与卸载

Mod 不向存档写入自定义字段，但它允许形成原版规划界面不能创建的重叠布局。首次使用前请备份存档。

如果要卸载，建议先在游戏内拆除所有与其他建筑重叠的普通电线，保存并退出后再删除 DLL。直接卸载不会留下自定义序列化数据，但原版游戏不保证能正确处理这些重叠对象。

## 兼容性

其他同时修改下列逻辑的 Mod 可能冲突：

- 建筑/蓝图放置检查；
- 格子销毁与建筑更新；
- 普通电线施工或拆除；
- `BuildingMgr.NewConnectCheck`、`DeleteConnectCheck` 或电网线路合并；
- 拆除工具的目标选择。

发生问题时，请附上 `BepInEx/LogOutput.log`、游戏版本、复现步骤和相关存档的备份副本。

## 从源码构建

需要 .NET SDK、已安装 BepInEx 5 的《鼠托邦》目录，以及可读取的游戏程序集。

```powershell
dotnet test .\WireThroughWalls.sln -c Release "/p:RatopiaDir=E:\steam\steamapps\common\Ratopia" /p:InstallAfterBuild=false
dotnet build .\src\WireThroughWalls\WireThroughWalls.csproj -c Release "/p:RatopiaDir=E:\steam\steamapps\common\Ratopia" /p:InstallAfterBuild=false --no-restore
```

生成发布包：

```powershell
.\scripts\Package.ps1 -RatopiaDir 'E:\steam\steamapps\common\Ratopia'
```

手工验收矩阵见 [docs/TESTING.md](docs/TESTING.md)。
