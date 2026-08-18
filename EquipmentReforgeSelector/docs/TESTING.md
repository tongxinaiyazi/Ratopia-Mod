# 测试与发布验收

本文档记录 `装备重铸自选属性` `0.1.2` 的自动化验证命令与手动验收矩阵。自动化检查不会安装 DLL 或启动 Ratopia。

## 自动化验证

在仓库根目录执行；显式指定 `InstallAfterBuild=false`：

```powershell
dotnet test .\EquipmentReforgeSelector.sln -c Release `
  /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false

dotnet build .\src\EquipmentReforgeSelector\EquipmentReforgeSelector.csproj -c Release `
  /p:RatopiaDir='E:\steam\steamapps\common\Ratopia' /p:InstallAfterBuild=false

.\scripts\Package.ps1
& 'C:\Users\ASUS\.codex\skills\developing-ratopia-mods\scripts\Test-RatopiaPackage.ps1' `
  -Path .\dist\装备重铸自选属性-v0.1.2-BepInEx5.zip `
  -ExpectedPluginName EquipmentReforgeSelector
```

如需由打包脚本显式执行构建或测试，使用 `.\scripts\Package.ps1 -Build -Test`。不带这两个开关时，脚本只从已有的 Release DLL 创建发布包。

## 手动验收矩阵（安装与游戏启动仅在后续验收阶段进行）

先备份测试存档和已安装插件；每一行记录存档名、物品、操作、材料数量、数值、截图和 `BepInEx\LogOutput.log` 的时间段。

| 场景 | 输入与操作 | 预期结果 |
| --- | --- | --- |
| Royal 1 级 | 在皇家铁匠铺（Royal）以武器、衣服、饰品各执行一次 | 候选直接显示在原版米色效果列表；仅包含同类别、1 级原版候选；点击文字、图标或整行空白均能选择；键盘数字键、上下键和回车有效；没有右侧深色面板。 |
| HellAnvil 2 级 | 在地狱铁砧（HellAnvil）以武器、衣服、饰品各执行一次 | 候选直接显示在原版米色效果列表；仅包含同类别、2 级原版候选；鼠标整行与数字键操作均可用；记录材料与数值变化。 |
| 四个格子刷新 | 选择第二项后，依次把鼠标划过中间“效果”和“重铸效果”的四个格子，再回到该阶重铸效果 | 其他提示内容保持原版；返回候选列表后第二项仍保持绿色箭头、背景和“已选择”，限定重铸结果仍与该项一致。 |
| 其他提示框 | 在装备详情中依次打开非“重铸效果”的米色提示 | 不出现可点击重铸候选，原版文字与交互不变；临时刷新不会清除相同装备和阶级的选择。 |
| 跨等级保留 | 在 1 级选择后，于 2 级打开同一装备；再反向重复 | 选择不错误地跨等级复用；各等级只显示本等级有效候选。 |
| 原版随机回退 | 清除选择、制造无效选择或无候选条件后执行 | 可见/日志警告，并使用原版随机；正常原版资源消耗。 |
| 两轮读档 | 保存、退出、重进并读档；重复保存、退出、重进并读档（两轮） | 不复制属性数据、不丢失物品；材料与数值记录在两轮后可逐项比对。 |
| 临时移除 DLL | 完成一次启用 Mod 的保存后，退出游戏，临时移除 DLL，再用原版读取 | 原版可读取最近一次测试存档；完成后恢复 DLL 前先保持游戏退出。 |
| 日志复核 | 每次首次进入、打开界面、选择、回退及读档后检查日志 | `LogOutput.log` 有插件发现、补丁安装与相关警告/异常；没有未解释错误。 |

不要把验收保存当作唯一备份。若出现崩溃、重复效果、物品丢失或原版无法读档，停止继续写入，保留日志和存档副本后再排查。
