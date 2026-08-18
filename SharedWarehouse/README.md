# 共享仓库

《鼠托邦》仓库共享 Mod。普通仓库与迷你仓库拥有无限材料种类容量，并使用同一份即时共享库存。

## 兼容性

- 游戏：《鼠托邦》v1.0.0600
- Mod 加载器：BepInEx 5.4.23.5（Mono）
- Mod 版本：0.1.0
- 插件 GUID：`cn.ratopia.sharedwarehouse`

电力仓库不受本 Mod 影响。第一版不提供配置文件、快捷键或游戏内开关。

## 功能

- 普通仓库与迷你仓库不再受材料种类数量限制，界面显示为“当前种类数/∞”。
- 任意目标仓库存入的物资会立即出现在其他目标仓库中。
- 保留每座仓库自己的准入、取出、材料数量限制、鼠民等级与分组设置。
- 旧存档第一次载入时合并所有目标仓库的库存，保留工作预留编号。
- 新建仓库自动接入；拆除非最后一座仓库不会掉落、删除或复制共享物资。
- 保存、建造判断、库存统计、食物统计和账本只计算一次共享库存。

## 安装

1. 确认游戏已经安装并成功运行过 BepInEx 5。
2. 解压发布包到游戏根目录，允许合并 `BepInEx` 文件夹。
3. 最终文件应位于：
   `Ratopia\BepInEx\plugins\SharedWarehouse\SharedWarehouse.dll`
4. 如果游戏所在完整路径包含中文，请双击游戏根目录的 `Launch_SharedWarehouse.cmd` 启动；纯英文路径可以照常启动游戏。
5. 在 `BepInEx\LogOutput.log` 中搜索“共享仓库”。

当前这份非 Steam 游戏封装使用的旧 Mono/Harmony 不能从中文路径可靠生成补丁。随 Mod 提供的启动器会临时选择 R: 至 Z: 之间的空闲盘符映射游戏目录，游戏关闭后自动取消映射，不会移动或复制游戏文件。

请先备份重要存档。不要同时启用其他会共享仓库列表、修改仓库容量或替换库存统计逻辑的 Mod。

## 卸载

1. 在仍启用本 Mod 时进入游戏并手动保存一次。
2. 退出游戏。
3. 删除 `BepInEx\plugins\SharedWarehouse\SharedWarehouse.dll`。

本 Mod 保存时会把共享物资只写入主仓库，因此存档可由原版读取。卸载后其他仓库会为空；如果物资种类超过原版容量，已有物资仍可取出，但需要腾出种类槽位后才能存入新的种类。

## 从源代码构建

需要 .NET SDK。项目目标为 .NET Framework 4.7.2，引用程序集由 NuGet 私有依赖提供。

```powershell
dotnet test .\SharedWarehouse.sln -c Debug
dotnet build .\src\SharedWarehouse\SharedWarehouse.csproj -c Release `
  /p:RatopiaDir="E:\steam\steamapps\common\Ratopia"
```

Release 构建默认只把 `SharedWarehouse.dll` 安装到游戏的插件目录。若只想构建而不安装，增加 `/p:InstallAfterBuild=false`。

生成 Nexus Mods 发布包：

```powershell
.\scripts\Package.ps1 -RatopiaDir "E:\steam\steamapps\common\Ratopia"
```

## 故障排查

- 启动日志显示“补丁安装失败”：检查游戏版本、BepInEx 版本以及是否有冲突 Mod。
- 日志提示“旧版 Mono 无法处理当前中文游戏路径”：退出游戏，改用 `Launch_SharedWarehouse.cmd`。
- 仓库仍显示有限容量：确认 DLL 路径正确，并检查 `LogOutput.log` 是否出现插件加载信息。
- 第一次载入旧存档失败：立即退出且不要覆盖存档，保留日志和备份存档用于排查。
- 材料统计异常：先停用其他仓库/库存类 Mod，再重新测试。
