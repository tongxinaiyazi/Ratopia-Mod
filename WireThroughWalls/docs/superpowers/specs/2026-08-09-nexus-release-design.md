# Wire Through Walls Nexus Mods Release Design

Date: 2026-08-09

## Goal

Create an unpublished Nexus Mods draft for Ratopia, upload the verified v0.1.0 installer archive, and prepare an original bilingual cover and an English-first description with Simplified Chinese below it. Keep the page unpublished until the final publish confirmation and manual in-game behavior gate.

## Release assets

- Main file: `dist/电线可穿墙-v0.1.0-BepInEx5.zip`
- Main file display name: `Wire Through Walls v0.1.0`
- File version: `0.1.0`
- Source code: not uploaded
- Cover: one original AI-generated 16:9 industrial blueprint illustration
- Cover text, verbatim:
  - `WIRE THROUGH WALLS`
  - `电线可穿墙`
- Cover scene: a cutaway wood-and-stone wall with one glowing electrical cable passing cleanly through it; warm Ratopia-compatible workshop mood without copying official game artwork, characters, logos, or UI.

## Nexus metadata

- Game: `Ratopia`
- Mod name: `Wire Through Walls - 电线可穿墙`
- Category: `Miscellaneous`
- Version: `0.1.0`
- Adult content: No
- Donation Points: Enabled
- Requirements: BepInEx 5
- Tags, when available:
  - Gameplay
  - Quality of Life
  - Utilities for Players
  - AI-Generated Content

## Short summary

Allows normal electrical wires to share tiles with walls, roads and other buildings while preserving blueprint, construction, demolition and save-loading behavior.

## Description

### English

## About this mod

Wire Through Walls turns Ratopia's normal electrical wire (`HeavyWire`) into a background-style utility. Wires can share a tile with walls, roads and other buildings, making compact layouts easier to plan.

## Features

- Place normal wire through existing walls and buildings.
- Build walls, roads and other buildings over existing wire.
- Supports either blueprint placement order.
- Preserves construction, cancellation and demolition behavior.
- Selects the wire first when demolishing an overlapped tile, helping prevent accidental removal of the foreground building.
- Reconciles supported overlaps after loading or switching saves.
- Does not modify the road-integrated `Wireroad`; only normal `HeavyWire` is affected.

## Requirements

- Ratopia (Mono build)
- BepInEx 5

## Installation

1. Install BepInEx 5 for Ratopia.
2. Extract the archive directly into the Ratopia game directory.
3. Confirm the DLL is located at:

   `Ratopia/BepInEx/plugins/WireThroughWalls/WireThroughWalls.dll`

4. Start the game and check `BepInEx/LogOutput.log` for `Wire Through Walls` / `电线可穿墙`.

## Compatibility

Other mods may conflict if they patch building or blueprint placement checks, tile destruction/building updates, normal wire construction or demolition, or demolition-tool target selection.

## Save safety and uninstalling

This mod does not add custom fields to save files, but it enables layouts that the vanilla planner cannot create. Back up important saves before using it.

Before uninstalling, demolish normal wires that overlap other buildings, save, exit the game, and then remove `WireThroughWalls.dll`. Vanilla Ratopia is not guaranteed to handle unsupported overlap layouts correctly after the mod is removed.

## Tested environment

- BepInEx 5.4.23.5
- Harmony 2.9.0
- Assembly-CSharp.dll SHA-256: `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`
- Automated test suite: 55/55 passed
- Release build: 0 warnings, 0 errors
- Package validator: passed
- BepInEx startup smoke test: plugin loaded with 0 Error/Fatal log lines

The public release remains gated on manual in-game placement, demolition, and save/reload testing.

---

### 简体中文

## 关于本 Mod

“电线可穿墙”把《鼠托邦》的普通电线（`HeavyWire`）作为背景设施处理。电线可以和墙、道路及其他建筑共用同一格，让紧凑布局更容易规划。

## 功能

- 普通电线可以穿过已有的墙和建筑。
- 墙、道路和其他建筑可以建在已有电线上。
- 支持两种蓝图放置顺序。
- 保持施工、取消和拆除流程。
- 拆除重叠格时优先选择电线，降低误拆前景建筑的风险。
- 读档或切换存档后重新协调受支持的重叠格。
- 不修改道路自带的 `Wireroad`，只处理普通电线 `HeavyWire`。

## 安装要求

- 《鼠托邦》Mono 版本
- BepInEx 5

## 安装方法

1. 为《鼠托邦》安装 BepInEx 5。
2. 把压缩包直接解压到游戏根目录。
3. 确认 DLL 位于：

   `Ratopia/BepInEx/plugins/WireThroughWalls/WireThroughWalls.dll`

4. 启动游戏，并在 `BepInEx/LogOutput.log` 中确认出现“电线可穿墙”。

## 兼容性

其他同时修改建筑/蓝图放置检查、格子销毁和建筑更新、普通电线施工或拆除、拆除工具目标选择的 Mod 可能发生冲突。

## 存档安全与卸载

本 Mod 不向存档添加自定义字段，但会产生原版规划界面无法创建的重叠布局。使用前请备份重要存档。

卸载前请先拆除所有与其他建筑重叠的普通电线，保存并退出游戏，然后删除 `WireThroughWalls.dll`。移除 Mod 后，原版游戏不保证能正确处理这些特殊重叠布局。

## 当前验证状态

- 自动测试：55/55 通过
- Release 构建：0 警告、0 错误
- 发布包结构验证：通过
- BepInEx 启动冒烟：插件成功加载，日志 0 Error/Fatal

公开发布前仍需完成实际放置、拆除和存读档测试。

## Permissions

- Modifications and translations are allowed with clear credit to the original author.
- Reuploading this file or modified copies to other sites requires explicit permission.
- Commercial use and use in paid mods are not allowed.
- Conversion to other games requires explicit permission.
- No third-party game assets are included in the release archive.

## Upload sequence and safety gates

1. Generate and inspect the cover image; save it inside the project.
2. Re-run the package validator and verify the archive SHA-256.
3. Open Nexus Mods using the user's existing authenticated Chrome profile.
4. Create a Ratopia mod draft with the metadata above.
5. Upload the cover and the main archive.
6. Configure requirements, permissions, tags and Donation Points.
7. Review the rendered page and file metadata.
8. Stop before the final public publish action.
9. Publish only after explicit action-time confirmation and the manual gameplay gate.
