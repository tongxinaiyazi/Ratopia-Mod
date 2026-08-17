# 人口自定义 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 创建可安装、可测试、按存档保存设置的 Ratopia 人口上限 Mod。

**Architecture:** 纯 C# 核心负责限制选择、输入校验和设置编解码；Harmony 层只替换两个原版上限返回值并接入场景生命周期；UGUI 层负责面板和输入所有权。设置存入原版预留的 `ModsData` 容器，不修改鼠民或机器鼠列表。

**Tech Stack:** C# / net472、BepInEx 5.4.23.5、Harmony 2.9.0、Unity UGUI、TextMeshPro、xUnit、Mono.Cecil。

## Global Constraints

- 独立目录名和程序集名均为 `PopulationCustomizer`。
- 插件名为“人口自定义”，GUID 为 `cn.ratopia.populationcustomizer`，版本为 `0.1.0`。
- 自定义值范围固定为 `0..999`；首次使用保持原版行为。
- 存档键固定为 `cn.ratopia.populationcustomizer.settings`，格式固定为 `v1|鼠民启用|鼠民上限|机器鼠启用|机器鼠上限`。
- 不删除超额单位，不自动触发游戏保存，不打包任何游戏或运行时依赖 DLL。
- 目标 `Assembly-CSharp.dll` SHA-256 为 `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`。
- 工作区不是 Git 仓库，不执行 commit；审查以完整文件、构建输出和测试证据为准。

---

### Task 1: 核心规则与设置编解码

- [ ] 建立 solution、net472 插件工程和测试工程，但不实现 Unity/Harmony 适配器。
- [ ] 先写上限选择、0/999 边界、非法输入和设置编解码测试，运行并记录 RED。
- [ ] 实现最小纯 C# 类型使测试通过，再运行完整 Task 1 测试并记录 GREEN。
- [ ] 自审公共接口、空值、损坏数据回退和测试命名。

### Task 2: 游戏程序集接入与人口设置面板

- [ ] 先写目标游戏合同、插件元数据、补丁发现和禁止依赖测试，运行并记录 RED。
- [ ] 实现插件入口、逐补丁安装、存档会话、ModsData 存取、两个上限 Postfix、人口栏按钮和模态面板。
- [ ] 实现 Action Map 所有权、场景清理、应用、恢复原版和一次性告警。
- [ ] 运行 Task 1/2 全部测试并完成实现审查。

### Task 3: 文档、打包和安装

- [ ] 先写发布输出与包内容合同测试，运行并记录 RED。
- [ ] 编写 README、实机测试说明和安全打包脚本。
- [ ] 运行 Release 测试、Release 构建和包验证，确认 ZIP 白名单与禁止 DLL 扫描。
- [ ] 确认 Ratopia 已退出，备份存档和旧 DLL，安装并比对 SHA-256。
- [ ] 完成加载、面板、招募/制造、保存重载、存档隔离、恢复原版和卸载兼容实机验收。

