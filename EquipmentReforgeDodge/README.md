# 装备重铸闪避属性

这是一个适用于《鼠托邦》（Ratopia）Mono 版的 BepInEx 5 Mod。它为装备重铸系统新增“闪避率”候选属性：

- 一阶重铸（皇家铁匠铺，普通强化）：+20% 闪避率。
- 二阶重铸（地狱铁匠铺，地狱强化）：+30% 闪避率。
- **只有饰品**会出现闪避重铸选项；武器与装甲的重铸列表保持原样。

游戏原版中居民只能通过生活用品获得 30 点闪避率，女王无法通过装备获取。本 Mod 让玩家可以通过饰品重铸主动堆叠闪避，作为对原版生存体系的补充。

## 实现原理

- 在 `DB_Mgr.Awake()` 装载完数据库后，把 `Res_Ability.Dodge` 追加进饰品对应 `ItemEnhanceInfo` 的一阶/二阶候选列表（仅修改当前会话内存，不写回游戏文件）。
- 原版计算女王闪避时只统计 Buff（生活用品），不统计装备重铸值；本 Mod 补上 `T_Queen.GetEnhancValue(Dodge)`，使重铸获得的闪避真正生效。
- 修正重铸槽位上闪避的显示格式（`+20%`）。
- 游戏内闪避上限仍为原版的 90%（`Defines.Maximum_Dodge`）。

## 兼容环境

- BepInEx 5.4.23.x。
- Harmony 2.x（随 BepInEx 5 内置）。
- 单人 Mono 版《鼠托邦》。

## 与“装备重铸自选属性”（EquipmentReforgeSelector）的关系

两个 Mod 可以同时安装、互不冲突：

- 本 Mod 只往饰品的重铸候选列表里追加闪避条目；“装备重铸自选属性”读取同一份候选列表供玩家自选，因此闪避也会出现在它的自选界面里。
- 若不安装“装备重铸自选属性”，闪避会以原版随机方式参与重铸抽取。
- 卸载本 Mod 后，存档中已重铸出的闪避词条不会报错，只是不再生效。

## 安装

1. 确保已经正确安装 BepInEx 5，并完全退出游戏。
2. 把发布 ZIP 直接解压到游戏根目录。
3. 确认 DLL 位于 `BepInEx/plugins/EquipmentReforgeDodge/EquipmentReforgeDodge.dll`。
4. 启动游戏，在 `BepInEx/LogOutput.log` 中查找“装备重铸闪避属性”和“闪避重铸注入完成”。

## 卸载

完全退出游戏后，删除 `BepInEx/plugins/EquipmentReforgeDodge` 文件夹即可。

## 存档风险

本 Mod 只在内存中追加重铸候选项，不读写任何存档字段。已重铸出闪避的饰品会在存档里保留一条 `Dodge` 词条数据；卸载 Mod 后该词条被游戏忽略，重新安装后自动恢复生效。

## 从源码构建

构建依赖环境变量 `RATOPIA_DIR`（指向鼠托邦游戏根目录），所有游戏路径均由它派生，项目内不硬编码任何目录。

```powershell
dotnet test .\EquipmentReforgeDodge.sln -c Release
dotnet build .\src\EquipmentReforgeDodge\EquipmentReforgeDodge.csproj -c Release --no-restore
```

Release 构建成功后会自动把 DLL 部署到 `%RATOPIA_DIR%\BepInEx\plugins\EquipmentReforgeDodge\EquipmentReforgeDodge.dll`，直接启动游戏即可生效。如需跳过自动部署，追加参数 `/p:InstallAfterBuild=false`。

生成发布包（打包流程内部会自行跳过部署）：

```powershell
.\scripts\Package.ps1
```

如需指定其他游戏目录，可传 `-RatopiaDir '<RatopiaDir>'` 覆盖。
