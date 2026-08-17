# 上帝视角管理

“上帝视角管理”是一个用于《鼠托邦》（Ratopia）的 BepInEx 5 Mono Mod。开启后，女王无需走到建筑旁边，即可用鼠标左键打开全图已建成建筑的原版配置面板；若女王已有移动会立即停止。WASD、方向键和手柄方向输入不会驱动女王移动，而相机现有的 WASD 与屏幕边缘滚屏行为保持不变。

## 环境与兼容性

- 插件 ID：`cn.ratopia.godviewmanagement`
- 版本：`0.1.3`
- 目标：BepInEx `5.4.23.5`、Harmony `2.9.0`、Mono / `.NET Framework 4.7.2`
- 已验证 `Assembly-CSharp.dll` SHA-256：`C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`

如果游戏更新后该哈希发生变化，请先重新运行测试，不要直接假定 Mod 仍兼容。

## 安装

1. 完全退出 Ratopia。
2. 确认游戏已经安装 BepInEx 5。
3. 把 `GodViewManagement.dll` 放到：
   `Ratopia\BepInEx\plugins\GodViewManagement\GodViewManagement.dll`
4. 启动游戏。BepInEx 日志应出现“上帝视角管理 v0.1.3 已加载”。
5. 本 Mod 会自行保护 BepInEx 宿主并可独立运行，不需要“共享仓库”或其他功能 Mod。日志中的“独立运行时驱动已启动”表示帧驱动已经生效。

移除 Mod 时，只需在游戏退出后删除上述 DLL。Mod 不写入自定义存档字段，原版仍可读取存档。

## 使用

- 每次启动游戏、进入存档或切换存档时，模式均默认为关闭。
- 默认切换键为 `M`。
- HUD 只保留一个向左避让原版 UI 的“上帝视角设置”入口；模式开关使用当前切换键。
- 设置面板显示模式状态，并支持重新绑定、恢复默认、隐藏 HUD 和冲突提示；`Esc` 取消捕获，Shift/Ctrl/Alt/Meta 等修饰键不能单独绑定。
- 点击“隐藏 HUD”后本会话不再显示 Mod HUD；按 `Shift + 当前切换键` 可恢复。重启或切换存档也会恢复显示。
- 开启时，女王已有的移动会立即停止；WASD、方向键和手柄方向输入均不会驱动女王。现有 WASD 相机移动与距离屏幕边缘 24 像素内的边缘滚屏保持可用。
- 没有其他面板时，左键点击远处已建成建筑即可打开原版配置面板。

远程面板保留配方、工人、启停、优先级、仓储、范围和重命名等配置。修理、拆除、交付及特殊女王互动不会远程执行，仍需女王走近后按原版 `F`。

以下目标不会被远程处理：空地、蓝图、Wallpaper、EnemyNexus、没有 `BuildInfoUI` 的对象、加载阶段对象，以及女王处于不安全状态时的目标。点击其他 UI 也不会被 Mod 接管。

配置由 BepInEx 保存到 `BepInEx\config\cn.ratopia.godviewmanagement.cfg`，公开配置项仅为 `Input.ToggleKey`。

## 从源码构建

```powershell
$env:RATOPIA_DIR = 'E:\steam\steamapps\common\Ratopia'
dotnet test .\GodViewManagement.sln -c Release /p:InstallAfterBuild=false
dotnet build .\src\GodViewManagement\GodViewManagement.csproj -c Release /p:InstallAfterBuild=false
```

生成发布包：

```powershell
.\scripts\Package.ps1 -RatopiaDir 'E:\steam\steamapps\common\Ratopia'
```

输出为 `dist\上帝视角管理-v0.1.3-BepInEx5.zip`。发布包只包含插件 DLL 和本 README，不包含游戏、Unity、BepInEx、Harmony 或测试运行时 DLL。

完整实机验收步骤见 `docs\TESTING.md`。
