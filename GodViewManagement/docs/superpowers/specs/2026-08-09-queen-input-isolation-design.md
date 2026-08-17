# “上帝视角管理”女王输入隔离修复设计

## 问题与根因

上帝视角开启后，Mod 把原版 `PlayerInput` 切换到 `UI` Action Map，并直接读取原始键盘状态来移动相机。但 Ratopia 的 `UI_Action_Move` 仍将键盘 WASD、方向键和手柄方向输入映射到与玩家移动相同的 `HotKeyName.LeftDir`、`RightDir`、`BottomDir` 和 `UpDir`。

`T_Queen.Update` 不区分当前 Action Map，仍通过 `InputMgr.GetKey(HotKeyName, bool)` 读取这些方向热键，因此 UI Action Map 不能隔离女王移动。

## 目标行为

- 上帝视角关闭时，所有女王移动输入保持原版行为。
- 上帝视角开启时，键盘 WASD、方向键和手柄方向输入都不能驱动女王。
- 上帝视角开启前已经开始的女王移动必须立即停止。
- 相机继续通过原始键盘 WASD 和屏幕边缘输入移动。
- 不跳过 `T_Queen.Update`，女王的非移动状态仍正常更新。
- 设置面板、远程建筑面板、存档切换和异常清理不能留下输入拦截状态。

## 选定方案

增加一个异常安全、可嵌套的“女王输入读取作用域”。

1. 为 `T_Queen.Update` 添加 Harmony Prefix，在上帝视角开启时进入作用域。
2. Postfix 释放作用域；Finalizer 在原版或其他补丁抛异常时也释放同一作用域。释放操作必须幂等。
3. 精确补丁 `InputMgr.GetKey(HotKeyName, bool)` 重载。仅当上帝视角开启、当前位于女王更新作用域内且请求属于方向热键时，返回 `false` 并跳过原方法。
4. 进入上帝视角时调用一次 `T_Queen.CharacterStop()`，终止已经开始的移动协程；之后由输入拦截阻止新移动。

方向拦截覆盖三组离散方向热键及方向复合值：`LeftDir`/`RightDir`/`BottomDir`/`UpDir`、后缀 `2` 和 `3` 的对应方向，以及 `AllDir`、`HorizontalDir`、`VerticalDir`、`AllDir2`。物理输入来源由原版 Input System 汇聚，因此键盘、方向键和手柄会统一被阻断。

作用域只包围 `T_Queen.Update`，不会全局禁用原版 UI 导航，也不会影响 Mod 直接读取 `Keyboard.current` 的相机控制。

## 组件边界

- `QueenInputIsolationRules`：纯 C# 规则，判断指定热键在给定模式和作用域状态下是否应被屏蔽。
- `QueenInputUpdateScope`：可嵌套、幂等释放的纯状态对象；不引用 Unity。
- `QueenUpdateInputScopePatch`：只负责进入和释放作用域。
- `DirectionalInputGetKeyPatch`：只负责调用纯规则并在需要时令 `GetKey` 返回 `false`。
- `Plugin`/`GodViewRuntime`：提供当前模式是否开启的只读查询。
- `GodViewCameraController`：开启时停止女王当前移动，继续负责相机原始输入。

## 异常与恢复

- Postfix 和 Finalizer 对同一作用域执行幂等释放，避免早退或异常造成永久输入屏蔽。
- 插件未初始化、会话正在重置或模式关闭时一律放行原版输入。
- 任一新增 Harmony 补丁安装失败时，沿用现有策略撤销本插件全部补丁并停用。
- 不修改存档字段，不改变 Input System 绑定或 Action Map 资源。

## 测试与验收

自动测试先失败、后实现：

- 模式关闭时所有方向热键放行。
- 模式开启但不在女王更新作用域时放行，保证 UI 不受影响。
- 模式开启且位于女王更新作用域时，所有方向热键均被屏蔽，非方向热键放行。
- 作用域支持嵌套、重复释放和异常清理。
- Mono.Cecil 合同测试锁定 `T_Queen.Update` 与 `InputMgr.GetKey(HotKeyName, bool)` 的精确签名，并确认两个新增补丁类存在。

Release 验证包括完整测试、0 警告构建、发布包内容检查、游戏退出状态下备份并安装、构建与安装 DLL 哈希比对和 BepInEx 加载日志。游戏内需记录女王坐标，分别测试 WASD、方向键和手柄方向输入，确认相机可移动而女王坐标不变；关闭模式后确认全部原版移动恢复。
