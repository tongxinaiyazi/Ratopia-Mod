# “广播站信号覆盖全图”Nexus Mods Release Materials Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 为 `广播站信号覆盖全图` v0.1.1 生成经过验证、可直接上传或复制到 Nexus Mods 的严格五文件发布目录。

**Architecture:** 文案、封面和 Mod ZIP 分别生成并独立验证，最后复制到全新的交付目录。交付目录不承载任何过程文件；Ratopia ZIP 验证和 Nexus 五文件验证作为两个独立发布门禁。

**Tech Stack:** Nexus BBCode、PNG、BepInEx 5 Mono、PowerShell、Ratopia 发布验证脚本、内置 ImageGen。

## Global Constraints

- Mod 名称为 `广播站信号覆盖全图`，程序集为 `BroadcastStationGlobalCoverage`，版本为 `0.1.1`。
- 英文标题为 `Broadcast Station Global Coverage`。
- 最终交付目录严格包含三个 TXT、一个 PNG 和一个 ZIP，共五个文件。
- 封面至少 1280×720，不使用官方 Logo，不绘制跨地图长电线。
- 完整介绍使用 Nexus BBCode，英文在前、中文在后，不夸大未完成的游戏内验收。
- ZIP 必须可解压到游戏根目录，且不得包含游戏、Unity、BepInEx、Harmony、PDB、日志或存档。

---

### Task 1: 文案与目录骨架

**Files:**
- Create: `artifacts/nexus-work/1-英文标题.txt`
- Create: `artifacts/nexus-work/2-简介.txt`
- Create: `artifacts/nexus-work/3-双语完整介绍.txt`

**Interfaces:**
- Consumes: `README.md`、`Plugin.cs`、已确认的发布设计。
- Produces: 三个 UTF-8 文本文件，供最终目录直接复用。

- [ ] **Step 1: 写入英文标题**

内容必须只有一行：

```text
Broadcast Station Global Coverage
```

- [ ] **Step 2: 写入双语简介**

简介说明全图电视信号，并明确不修改电视服务距离或电路范围。

- [ ] **Step 3: 写入双语 BBCode 完整介绍**

按英文功能、排除项、要求、安装、卸载、存档与冲突、排错、更新记录，再按相同顺序写中文。

- [ ] **Step 4: 静态复核**

运行：

```powershell
rg -n "363|覆盖全图电路|full-map electricity|fully tested" .\artifacts\nexus-work\*.txt
```

预期：无匹配。

### Task 2: PNG 封面

**Files:**
- Create: `artifacts/nexus-work/4-封面.png`

**Interfaces:**
- Consumes: 已批准的封面设计与准确英文标题。
- Produces: 一张至少 1280×720 的横向 PNG。

- [ ] **Step 1: 用内置 ImageGen 生成原创横向插画**

提示词必须包含地下鼠鼠城市、广播站、青蓝信号波纹、远端电视、棕金配色，以及禁止跨地图电线、官方 Logo、水印和额外文字。

- [ ] **Step 2: 复制生成图到工作目录**

保留单一 PNG，不在最终目录保留生成中间稿。

- [ ] **Step 3: 视觉与像素验证**

检查标题拼写、裁切、可读性、错误电路暗示，并确认宽度至少 1280、高度至少 720。

### Task 3: Mod ZIP 复验与最终交付

**Files:**
- Copy: `dist/广播站信号覆盖全图-v0.1.1-BepInEx5.zip`
- Create: `Nexus-发布资料-广播站信号覆盖全图-v0.1.1/5-广播站信号覆盖全图-v0.1.1-BepInEx5.zip`

**Interfaces:**
- Consumes: 三个文本文件、封面 PNG 和当前 Release ZIP。
- Produces: 严格五文件的最终交付目录。

- [ ] **Step 1: 运行 Ratopia ZIP 验证**

运行：

```powershell
& 'C:\Users\ASUS\.codex\skills\developing-ratopia-mods\scripts\Test-RatopiaPackage.ps1' `
  -Path '.\dist\广播站信号覆盖全图-v0.1.1-BepInEx5.zip' `
  -ExpectedPluginName 'BroadcastStationGlobalCoverage'
```

预期：`Errors` 为空，禁止文件与异常路径均为空。

- [ ] **Step 2: 创建全新最终目录并复制五个文件**

最终文件名严格匹配全局约束，不包含任何第六个文件。

- [ ] **Step 3: 运行 Nexus 交付验证**

运行：

```powershell
& 'C:\Users\ASUS\.codex\skills\publishing-ratopia-nexus-mods\scripts\Test-RatopiaNexusDeliverables.ps1' `
  -Path '.\Nexus-发布资料-广播站信号覆盖全图-v0.1.1' `
  -ModName 'BroadcastStationGlobalCoverage' `
  -Version '0.1.1'
```

预期：`RATOPIA_NEXUS_DELIVERABLES_VALID=True`。

- [ ] **Step 4: 最终检查**

确认交付目录恰好五个文件，Ratopia 未运行，项目源码和已安装 DLL 均未被发布资料制作过程修改。
