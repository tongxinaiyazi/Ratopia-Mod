# Ratopia 本地调试控制台启用设计

## 目标

在本机《鼠托邦》v1.0.0600 中启用游戏自带调试功能，不修改 `SharedWarehouse.dll`、共享仓库源码或游戏程序集。

## 方案

在游戏目录 `E:\steam\steamapps\common\Ratopia\Ratopia_Data\Log` 中创建空文件 `Admin.txt`。

游戏原生的 `U_SaveData.LoadAll` 会检查该文件；文件存在时，将 `Defines.IsPublicVersion` 设为 `false`，并将 `Defines.Cheat` 设为 `true`。因此无需 Harmony 补丁、BepInEx 插件或修改 `Assembly-CSharp.dll`。

## 热键

- `F8`：切换完整 Cheat/Palette 调试面板。
- `F3`：切换格子坐标调试显示。
- `F4`：切换电力端口调试显示。

本次不重映射按键，避免修改游戏程序集及与原有调试功能发生冲突。

## 安全与恢复

- 创建前确认游戏未运行。
- 若目标文件已存在，保留原文件，不覆盖其内容。
- 卸载或关闭调试功能时，只需删除 `Admin.txt`；不影响存档及 Mod。
- 不改动 `SharedWarehouse` 工程和已安装插件。

## 验证

1. 静态确认 `Admin.txt` 存在于游戏读取的准确路径。
2. 启动游戏并进入存档。
3. 验证 `F8` 可以显示和关闭完整调试面板。
4. 验证 `F3`、`F4` 分别控制原生调试显示。
5. 检查 `Player.log` 和 BepInEx 日志没有新增异常。
