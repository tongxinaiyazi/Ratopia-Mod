# 卫生间澡堂加乐趣 / Restroom & Bathhouse Fun

版本：1.0.0

这是一个适用于《鼠托邦》(Ratopia) 的 BepInEx 5 Mono Mod：鼠民完整使用普通卫生间后增加 25 点乐趣，完整使用澡堂后增加 30 点乐趣。奖励使用游戏原生 `FunUpdate`，因此乐趣仍按原版规则封顶为 100。

## 功能范围

- 普通卫生间：默认增加 25 乐趣。
- 澡堂：默认增加 30 乐趣。
- 电动卫生间不生效。
- 取消或中断设施使用时不奖励。
- 不提供游戏内设置、额外浮字或界面。

## 环境与兼容性

- BepInEx 5 Mono；开发与静态验证环境为 BepInEx 5.4.23.5、Harmony 2.9.0。
- 目标 `Assembly-CSharp.dll` SHA-256：`C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`。
- 由于 `Ratopia.exe` 的文件版本字段不可信，本 Mod 不以该字段声明游戏版本。
- 本发布包已通过单元测试、静态程序集合同、Release 构建与包结构验证，但未进行游戏内实机验收。

## 安装

1. 确认游戏已安装 BepInEx 5 Mono，并关闭游戏。
2. 将 ZIP 内文件直接解压到 Ratopia 游戏根目录。
3. DLL 最终应位于 `BepInEx/plugins/RestroomBathFun/RestroomBathFun.dll`。
4. 启动一次游戏后，BepInEx 会生成配置文件。

## 配置

配置文件：`BepInEx/config/cn.ratopia.restroombathfun.cfg`

```ini
[Rewards]
ToiletFunReward = 25
BathsFunReward = 30
```

两个数值都接受 0–100。`ToiletFunReward` 控制普通卫生间奖励，`BathsFunReward` 控制澡堂奖励。请关闭游戏后编辑；修改后重启游戏才会生效。

## 卸载与存档

关闭游戏后删除 `BepInEx/plugins/RestroomBathFun/RestroomBathFun.dll`（也可删除整个同名插件文件夹）。如不再需要配置，可另外删除对应 CFG。本 Mod 不新增存档字段，也不修改存档结构；静态设计上卸载不会要求迁移存档，但由于未进行游戏内实机验收，建议任何 Mod 环境都保留重要存档备份。

## 冲突与排错

- 可能与其他补丁 `T_Citizen.OnServiceChoreographyEnd(Building)`、替换卫生间/澡堂服务流程或直接改写乐趣的 Mod 发生冲突。
- 若功能未生效，请检查 `BepInEx/LogOutput.log` 中是否出现“卫生设施乐趣奖励功能已启用”。
- 确认使用的是普通卫生间而不是电动卫生间，并确认服务没有被中断。
- 报告问题时请附上 BepInEx 版本、`Assembly-CSharp.dll` 哈希和相关日志片段。
