# 卫生间澡堂加乐趣设计规格

## 目标

创建独立的 Ratopia BepInEx 5 Mono Mod“卫生间澡堂加乐趣”1.0.0。普通卫生间完整使用后增加 25 乐趣，澡堂完整使用后增加 30 乐趣；电动卫生间和中断服务不奖励。

## 运行时设计

- Harmony 补丁 `T_Citizen.OnServiceChoreographyEnd(Building)`。
- Prefix 在原方法重置状态前捕获服务是否中断以及建筑类别；Postfix 仅在原方法正常返回后应用奖励。
- 只把 `BuildingName.Toilet` 映射为普通卫生间，把 `BuildingName.Baths` 映射为澡堂。`BuildingName.ElecToilet` 和所有其他建筑映射为不支持。
- 通过 `T_Citizen.FunUpdate(float)` 应用奖励，沿用原版 0–100 上限，不直接写 `m_Fun`。
- 纯奖励策略不引用 Unity、游戏程序集或 Harmony；补丁层只采集参数并调用运行时协调器。
- 补丁异常不得传播。安装任一补丁失败时撤销本插件的全部 Harmony 补丁；插件销毁时卸载自身补丁。

## 配置

插件 GUID 为 `cn.ratopia.restroombathfun`。BepInEx 自动生成 `BepInEx/config/cn.ratopia.restroombathfun.cfg`：

- `[Rewards] ToiletFunReward = 25`，允许 0–100。
- `[Rewards] BathsFunReward = 30`，允许 0–100。

没有游戏内设置、热重载、浮字或其他 UI。编辑配置后重启游戏生效。

## 兼容性与安全

- 目标为 Mono、BepInEx 5.4.23.5、Harmony 2.9.0。
- 目标 `Assembly-CSharp.dll` SHA-256：`C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`。
- 不新增存档字段，不修改数据库或游戏配置，不包含游戏/Unity/BepInEx/Harmony DLL。
- 本次只做静态、单元、构建和包结构验证，不安装、不启动游戏，不宣称完成实机验收。

## 发布合同

ZIP 只包含 `BepInEx/plugins/RestroomBathFun/RestroomBathFun.dll` 和 `README.md`。最终 Nexus 发布目录严格包含英文标题、简介、双语完整介绍、PNG 封面和 Mod ZIP 五个文件。

