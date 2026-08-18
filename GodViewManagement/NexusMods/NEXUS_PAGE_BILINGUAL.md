# God View Management

## English

Manage your colony from anywhere on the map.

God View Management lets you move the camera freely and open the original configuration panel of completed buildings without walking the Queen over to them. It keeps building management fast while deliberately reserving Queen-only actions for normal close-range interaction.

### Features

- Toggle God View with `M` or your rebound key.
- Open settings from one compact launcher shifted left from Ratopia's original upper-right UI, or hide the Mod HUD completely for the current session.
- Move the camera with `WASD` or by moving the cursor within 24 pixels of a screen edge.
- Keep the Queen stationary while using keyboard, arrow-key, or controller direction input in God View.
- Left-click a completed building anywhere on the map to open its original configuration panel.
- Remotely configure recipes, workers, enable/disable state, priority, storage, range, and building names where supported by the original panel.
- Automatically reset God View to off whenever the game starts or a save is entered/switched.
- Rebind the toggle key from the in-game settings panel, with conflict detection against the game's current input bindings.

### Safety limits

God View does not handle empty tiles, blueprints, Wallpaper, EnemyNexus, objects without `BuildInfoUI`, clicks over other UI, loading-state objects, or targets reached while the Queen is in an unsafe state.

Repair, demolition, delivery, and other special Queen interactions are intentionally unavailable through a remotely opened panel. Walk the Queen close to the target and use the original `F` interaction for those actions.

### Requirements

- Ratopia for PC, Mono build
- BepInEx 5 (`5.4.23.5` tested)
- Tested against Ratopia `1.0.0600`
- Tested `Assembly-CSharp.dll` SHA-256: `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`

This build is not intended for BepInEx 6 or an IL2CPP version of the game. A future Ratopia update may require a compatibility update even if the plugin still loads.

### Installation

1. Close Ratopia completely.
2. Install BepInEx 5 for the Mono version of the game.
3. Extract the archive into the Ratopia game folder.
4. Confirm the DLL is located at `Ratopia\BepInEx\plugins\GodViewManagement\GodViewManagement.dll`.
5. Start the game and check `BepInEx\LogOutput.log` for `God View Management 0.1.3` if the HUD does not appear.

God View Management is standalone. Shared Warehouse and other gameplay Mods are not required.

### Controls and configuration

- Default toggle key: `M`
- HUD: one compact settings launcher shifted left from the upper-right corner
- Hide/restore HUD: use `Hide HUD` in settings; press `Shift + current toggle key` to restore it
- Camera movement: `WASD` and screen-edge scrolling
- Remote building selection: left mouse button while no other panel is open
- Config file: `BepInEx\config\cn.ratopia.godviewmanagement.cfg`

Close the settings panel or a remote building panel before toggling God View off.

### Compatibility and saves

The mod adds no custom fields to Ratopia save files. Removing it should leave saves readable by the original game. Backing up important saves before using any mod is still recommended.

Mods that also replace Queen input, camera movement, global building selection, or the original building configuration panel may conflict. If the game updates and the assembly hash above changes, wait for a compatibility check before continuing to use the mod.

### Uninstallation

1. Close Ratopia.
2. Remove `BepInEx\plugins\GodViewManagement\GodViewManagement.dll`.
3. Optionally remove `BepInEx\config\cn.ratopia.godviewmanagement.cfg` to delete the saved hotkey setting.

### Troubleshooting

- No HUD: confirm BepInEx 5 is installed and check `BepInEx\LogOutput.log`.
- Toggle key does not bind: the key may conflict with a current Ratopia input mapping; modifier-only bindings are rejected.
- A building cannot be selected: verify that it is fully constructed, has an original configuration panel, and no other UI panel is open.
- Queen-only action is hidden: this is intentional; move the Queen close and press `F`.

### Changelog

#### 0.1.3

- Removes the permanent mode-toggle HUD button and keeps one compact settings launcher.
- Shifts the launcher left to avoid Ratopia's original upper-right UI.
- Adds session-only HUD hiding with `Shift + current toggle key` recovery.

#### 0.1.2

- Protects the BepInEx plugin host directly, fixing the accidental dependency on Shared Warehouse during scene startup.
- Adds a self-owned runtime update driver while retaining the Harmony fallback.
- Prevents another Mod's presence or load timing from determining whether the HUD and controls initialize.
- Deduplicates both driver sources so input and camera logic still run only once per frame.

#### 0.1.1

- Stops existing Queen movement when God View is enabled.
- Prevents `WASD`, arrow keys, and controller direction input from moving the Queen while camera controls remain available.
- Adds scope-safe input isolation and cleanup for exceptions and session changes.

#### 0.1.0

- Initial release with free camera controls, HUD/settings, hotkey rebinding, and full-map building configuration.

---

## 中文

在地图任意位置管理你的殖民地。

“上帝视角管理”允许玩家自由移动相机，并在女王无需走近建筑的情况下打开全图已建成建筑的原版配置面板。建筑管理会更快捷，但修理、拆除等女王专属动作仍保留原版近距离交互要求。

### 功能

- 使用 `M` 或重新绑定后的按键开启、关闭上帝视角。
- 通过向左避让原版右上角 UI 的紧凑入口打开设置，也可在当前会话完全隐藏 Mod HUD。
- 使用 `WASD`，或把鼠标移动到距离屏幕边缘 24 像素以内来移动相机。
- 上帝视角开启时，键盘、方向键和手柄方向输入不会驱动女王移动。
- 左键点击地图任意位置的已建成建筑，打开它的原版配置面板。
- 在原版面板支持的情况下，远程调整配方、工人、启停、优先级、仓储、范围和建筑名称。
- 每次启动游戏、进入存档或切换存档时，上帝视角都会自动恢复为关闭。
- 可在游戏内设置面板重新绑定切换键，并检测与当前原版按键的冲突。

### 安全限制

空地、蓝图、Wallpaper、EnemyNexus、缺少 `BuildInfoUI` 的对象、其他 UI 上的点击、加载阶段对象，以及女王处于不安全状态时的目标不会被远程处理。

修理、拆除、交付和其他女王特殊互动不能通过远程面板执行。需要让女王走到目标附近并使用原版 `F` 互动。

### 环境要求

- PC 版 Ratopia，Mono 环境
- BepInEx 5（已测试 `5.4.23.5`）
- 已测试 Ratopia `1.0.0600`
- 已测试 `Assembly-CSharp.dll` SHA-256：`C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`

本版本不适用于 BepInEx 6 或 IL2CPP 游戏环境。Ratopia 更新后，即使插件仍能加载，也可能需要兼容性更新。

### 安装

1. 完全退出 Ratopia。
2. 为 Mono 版游戏安装 BepInEx 5。
3. 把压缩包解压到 Ratopia 游戏根目录。
4. 确认 DLL 位于 `Ratopia\BepInEx\plugins\GodViewManagement\GodViewManagement.dll`。
5. 启动游戏。如果没有出现 HUD，请检查 `BepInEx\LogOutput.log` 中是否存在 `God View Management 0.1.3`。

“上帝视角管理”可独立运行，不需要“共享仓库”或其他功能 Mod。

### 操作与配置

- 默认切换键：`M`
- HUD：从屏幕右上角向左避让的紧凑设置入口
- 隐藏/恢复 HUD：在设置中点击“隐藏 HUD”；按 `Shift + 当前切换键` 恢复
- 相机移动：`WASD` 和屏幕边缘滚动
- 远程选择建筑：没有打开其他面板时使用鼠标左键
- 配置文件：`BepInEx\config\cn.ratopia.godviewmanagement.cfg`

退出上帝视角前，请先关闭设置面板或远程建筑面板。

### 兼容性与存档

本 Mod 不会向 Ratopia 存档添加自定义字段，移除后原版应仍能读取存档。使用任何 Mod 前仍建议备份重要存档。

其他同时修改女王输入、相机移动、全局建筑选择或原版建筑配置面板的 Mod 可能产生冲突。如果游戏更新后上述程序集哈希发生变化，请等待重新验证兼容性后再继续使用。

### 卸载

1. 完全退出 Ratopia。
2. 删除 `BepInEx\plugins\GodViewManagement\GodViewManagement.dll`。
3. 如需删除已保存的热键设置，可一并删除 `BepInEx\config\cn.ratopia.godviewmanagement.cfg`。

### 故障排查

- 没有 HUD：确认安装的是 BepInEx 5，并检查 `BepInEx\LogOutput.log`。
- 无法绑定切换键：该按键可能与 Ratopia 当前输入映射冲突；Shift、Ctrl、Alt 等修饰键不能单独绑定。
- 无法选择某个建筑：确认建筑已经完工、拥有原版配置面板，并且当前没有打开其他 UI 面板。
- 女王动作按钮被隐藏：这是预期行为，请让女王走近目标后按 `F`。

### 更新日志

#### 0.1.3

- 移除常驻模式开关，只保留一个紧凑设置入口。
- 设置入口向左移动，避让 Ratopia 原版右上角 UI。
- 增加会话内隐藏 HUD，并可用 `Shift + 当前切换键` 恢复。

#### 0.1.2

- 由上帝视角自身保护 BepInEx 插件宿主，修复切场景时对“共享仓库”的意外依赖。
- 增加插件自有的运行时更新驱动，同时保留 Harmony 后备驱动。
- 不再让其他 Mod 是否存在或加载时序决定 HUD 与控制功能能否初始化。
- 两个驱动来源按帧去重，输入与相机逻辑每帧仍只执行一次。

#### 0.1.1

- 开启上帝视角时立即停止女王已有的移动。
- 阻止 `WASD`、方向键和手柄方向输入驱动女王，同时保留相机控制。
- 加入作用域安全的输入隔离，并在异常和会话切换时正确清理。

#### 0.1.0

- 首次发布：包含自由相机、HUD/设置、热键重绑和全图建筑配置。
