# Stronger Work Distance — Nexus Mods 发布资料设计规格

## 目标

为 Ratopia Mod“更强大的工作距离”制作一套可直接用于 Nexus Mods 上传的发布资料。页面主标题使用英文，简介与详情页提供英文和中文，封面直接以英文和中文标题构成，不使用游戏截图或角色素材。

## 已确认内容

- 英文标题：`Stronger Work Distance`
- 中文标题：`更强大的工作距离`
- Mod 版本：`0.1.0`
- 游戏：Ratopia
- 运行环境：BepInEx 5 Mono
- 插件 GUID：`cn.ratopia.strongerworkdistance`
- 主文件名：`更强大的工作距离-v0.1.0-BepInEx5.zip`

## 文案交付

在仓库的 `release/NexusMods` 目录中提供：

- `NEXUS_TITLE.txt`：英文页面标题。
- `NEXUS_SUMMARY.txt`：适用于 Nexus Mods 摘要字段的英文短简介，并附中文对照。
- `NEXUS_DESCRIPTION.txt`：使用 Nexus BBCode 编排的完整双语详情页，英文在前、中文在后。
- `FILE_DESCRIPTION.txt`：主文件的双语说明。
- `CHANGELOG.txt`：`0.1.0` 首发更新记录。
- `UPLOAD_CHECKLIST.md`：上传字段、文件路径、封面路径与发布前核验事项。

详情页准确描述以下边界：鼠民通用工具工作站位扩展为横向两格、纵向最高四格的完整 25 格矩形，覆盖采矿、建造、拆除、维修及共用蓝图站位；不修改女王操作距离、战斗射程、建筑效果范围或存档格式。内容包含需求、安装、卸载、兼容性、冲突提示与日志排查方式。

不得把尚未完成的实机存档内行为验收写成已通过。`UPLOAD_CHECKLIST.md` 将实机范围测试、保存重载和卸载恢复列为公开上传前待确认项。

## 封面设计

输出尺寸为 `1600 × 900`，同时保留可编辑 SVG 和 Nexus 上传用 PNG：

- `images/StrongerWorkDistance-cover-1600x900.svg`
- `images/StrongerWorkDistance-cover-1600x900.png`

画面采用深蓝黑背景、低对比度的 5×5 方格与少量青蓝/金色线条，暗示工作距离和格子范围。中央仅显示两行主标题：

1. `STRONGER WORK DISTANCE`
2. `更强大的工作距离`

不放版本号、宣传语、水印、游戏 Logo、截图或生成式角色图。标题通过 SVG 确定性排版，确保英文和中文无拼写错误；PNG 从同一 SVG 渲染得到。

## 验证

- 文案中的版本、安装路径、功能边界与仓库 README 一致。
- Nexus BBCode 标签成对闭合，文件均为 UTF-8。
- SVG 的 `viewBox` 与目标宽高一致，且包含精确的中英文标题。
- PNG 为 1600×900，人工检查标题清晰、无裁切、无乱码。
- 发布资料不包含游戏、Unity、BepInEx 或 Harmony DLL，也不修改现有 Mod 包和游戏安装目录。

