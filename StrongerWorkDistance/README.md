# 更强大的工作距离

这是一个适用于《鼠托邦》（Ratopia）Mono 版的 BepInEx 5 Mod。它把所有使用通用鼠民工具站位的工作范围扩大为：

- 横向 2 格。
- 最高 4 格高。
- 包含斜角在内的完整 25 格矩形。

覆盖常规采矿、建造、拆除、维修，以及使用特殊蓝图站位表的工作。Mod 不改变女王操作距离、战斗射程或建筑效果范围。

## 兼容环境

- BepInEx 5.4.23.5。
- Harmony 2.9.0.0。
- 已验证的 `Assembly-CSharp.dll` SHA-256：`C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`。

游戏更新后如果该哈希发生变化，请先重新验证合同测试，不要继续使用旧构建并假定补丁仍然兼容。

## 安装

1. 确保已经正确安装 BepInEx 5，并完全退出游戏。
2. 把发布 ZIP 直接解压到游戏根目录。
3. 确认 DLL 位于 `BepInEx/plugins/StrongerWorkDistance/StrongerWorkDistance.dll`。
4. 启动游戏，在 `BepInEx/LogOutput.log` 中查找“更强大的工作距离”和“工作距离已应用”。

## 卸载

完全退出游戏后，删除 `BepInEx/plugins/StrongerWorkDistance` 文件夹即可。下次启动时，游戏会由原版 `SystemMgr.Awake()` 恢复默认工作站位。

## 存档风险

本 Mod 只修改当前运行会话中的工作站位列表，不读取或写入存档字段。启用、重复读档和卸载都不应复制或残留数据。重要存档仍建议在安装任何 Mod 前自行备份。

## 冲突说明

其他直接修改 `SystemMgr.List_WM_EnableArea`、`SystemMgr.List_BP_Ld_EnableArea` 或补丁 `SystemMgr.Awake()` 的 Mod 可能覆盖本 Mod 的结果。出现问题时，请检查 `BepInEx/LogOutput.log` 中各插件的补丁顺序和错误信息。

## 从源码构建

```powershell
$env:RATOPIA_DIR = '<RatopiaDir>'
dotnet test .\StrongerWorkDistance.sln -c Release /p:InstallAfterBuild=false
dotnet build .\src\StrongerWorkDistance\StrongerWorkDistance.csproj -c Release /p:InstallAfterBuild=false --no-restore
```

生成发布包：

```powershell
.\scripts\Package.ps1 -RatopiaDir '<RatopiaDir>'
```
