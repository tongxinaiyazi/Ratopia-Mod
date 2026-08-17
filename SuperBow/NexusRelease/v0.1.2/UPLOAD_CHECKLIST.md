# Nexus Mods 上传检查表

## 页面字段

- 游戏：`Ratopia`
- 英文标题：`Super Bow - Splash Damage and Bleed for the Queen's Bow`
- 版本：`0.1.2`
- 成人内容：`No`
- 建议分类：优先选择 `Gameplay`；Ratopia 页面没有该分类时选择 `Miscellaneous`
- 建议标签：`Gameplay`、`Weapons`、`Quality of Life`，仅选择页面实际提供的标签
- 简介：复制 `NEXUS_SUMMARY.txt`
- 正文：复制 `NEXUS_DESCRIPTION.txt`
- 封面：上传 `images/SuperBow-Cover-1280x720.png`

## 主文件

- 上传文件：`files/SuperBow-v0.1.2-BepInEx5.zip`
- 文件显示名：`Super Bow v0.1.2 - BepInEx 5`
- 文件版本：`0.1.2`
- 文件说明：复制 `FILE_DESCRIPTION.txt`
- 更新记录：复制 `CHANGELOG.txt`
- Requirements 中添加 `BepInEx 5`，并在说明中保持精确版本 `5.4.23.5`

## 权限与披露

- 原版 `WoodBow` 图标已由上传者确认获得 Cassel Games 使用授权；页面 Credits 中保留授权说明
- 原图标只作为 Nexus 页面图片，不放入 Mod 下载 ZIP
- 声明 Mod 代码和发布文档包含 AI 协助，并已由上传者审核
- 不添加 `Nexus Mods Turns 25` 活动标签：2026 活动规则禁止生成式 AI 参与的代码或素材
- 不开启与实际授权范围冲突的资产再利用权限

## 发布前最后检查

- 运行 `scripts/Test-NexusRelease.ps1`
- 确认验证结果为 `NEXUS_RELEASE_VALID=True`
- 确认 ZIP 中只有 `README.md` 与 `BepInEx/plugins/SuperBow/SuperBow.dll`
- 确认页面没有宣称 Cassel Games、BepInEx 或 Harmony 作者为该 Mod 背书
- 确认没有上传存档、日志、游戏 DLL、Unity DLL、BepInEx DLL、Harmony DLL 或 PDB
- 发布后下载一次 Nexus 扫描完成的文件，重新检查目录结构和 SHA-256

相关规则：

- https://help.nexusmods.com/article/28-file-submission-guidelines
- https://help.nexusmods.com/article/136-best-practices-for-mod-authors
- https://help.nexusmods.com/article/117-why-has-my-mod-been-quarantined
