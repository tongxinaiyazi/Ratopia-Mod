# “睡觉加速”设计规格

## 目标

创建独立的 Ratopia BepInEx 5 Mono Mod“睡觉加速”0.1.0。女王通过任意 `BuildAbility.QueenBed` 女王床进入 `AniState.Sleep_bed` 后，连续度过三个未暂停的真实秒即临时切换为五倍速；离床后恢复触发前的玩家速度。

## 运行时设计

- `T_Queen.Update()` Postfix 只采集女王状态、暂停状态和 `Time.unscaledDeltaTime`，并调用纯 C# 状态机。
- 仅 `CharState.Queen_Action + AniState.Sleep_bed` 计时，上床动画不计入三秒。
- 触发时读取 `PlayDataMgr.m_UserGameSpeed` 并调用 `SystemMgr.SetTimeScale(5f)`，不写玩家选项或存档。
- `SystemMgr.ApplyUserGameSpeed(float)` Postfix 将已激活的本次睡眠标记为已抑制；玩家新速度立即生效，离床不再恢复旧值。
- 离床、管理器变化或插件销毁时恢复捕获速度。失败保持可重试状态，异常不传播到游戏帧循环。
- 补丁逐项安装，任一失败则撤销本插件全部补丁。记录发现、安装、首次调用、激活、恢复、取消和故障阶段。

## 兼容性和交付

- Ratopia 1.0.0600，Mono，BepInEx 5.4.23.5，Harmony 2.9.0，net472。
- `Assembly-CSharp.dll` SHA-256：`C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`。
- 不新增配置、UI 或存档字段。
- ZIP 只包含 `BepInEx/plugins/SleepAcceleration/SleepAcceleration.dll` 和根目录 `README.md`。
