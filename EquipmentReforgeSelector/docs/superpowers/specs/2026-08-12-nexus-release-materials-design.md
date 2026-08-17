# 装备重铸自选属性 N 网发布资料设计

## 目标

为 `装备重铸自选属性` `0.1.2` 制作一套可以直接用于 Nexus Mods（N 网）的发布资料。最终交付应让用户无需进入开发目录查找文件：一个独立文件夹内只有页面标题、简介、完整介绍、封面和安装包五项。

## 已确认的发布基线

- Mod 中文名：`装备重铸自选属性`
- 英文名与页面标题：`Equipment Reforge Selector`
- 版本：`0.1.2`
- 插件 GUID：`cn.ratopia.equipmentreforgeselector`
- 游戏：Ratopia（鼠托邦）
- 运行环境：Mono、BepInEx 5、`.NET Framework 4.7.2`
- 已检查程序集：`Assembly-CSharp.dll` SHA-256 `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`
- 已存在发布包：`dist/装备重铸自选属性-v0.1.2-BepInEx5.zip`
- 发布包安装入口：`BepInEx/plugins/EquipmentReforgeSelector/EquipmentReforgeSelector.dll`

## 最终目录合同

新建独立交付目录，目录内严格只有以下五个文件：

1. `1-英文标题.txt`
2. `2-简介.txt`
3. `3-双语完整介绍.txt`
4. `4-封面.png`
5. `5-装备重铸自选属性-v0.1.2-BepInEx5.zip`

不得在交付目录中加入 JPG、SVG、README、更新日志、哈希清单、设计文档、实施计划、验证脚本、源文件、日志、存档或缩略图。开发与验证资料继续留在项目目录，不复制到最终目录。

## 页面文案

### 英文标题

标题文件只包含一行：

`Equipment Reforge Selector`

标题不包含版本号、BepInEx 标签、中文括号或营销口号，方便直接复制到 Nexus Mods 页面标题字段。

### 简介

简介采用英文在前、中文在后的双语短文。它应在简短篇幅内说明：

- 将皇家铁匠铺和熔岩铁匠铺的随机装备重铸改为从原版有效候选中自选。
- 候选直接显示在原版米色效果列表。
- 支持整行鼠标点击、数字键和键盘导航。
- 不改变材料消耗、属性数值或原版存档格式。

简介不写安装步骤、游戏程序集哈希或长篇风险说明。

### 双语完整介绍

完整介绍使用 Nexus BBCode，英文在前、中文在后，两种语言信息对等。章节包括：

1. Overview / 概述
2. Features / 功能
3. What It Does Not Change / 不会修改的内容
4. Requirements / 前置需求
5. Installation / 安装
6. Usage / 使用方法
7. Compatibility and Conflicts / 兼容性与冲突
8. Save Safety and Uninstallation / 存档安全与卸载
9. Troubleshooting / 故障排查
10. Version / 版本信息

文案必须明确：

- 皇家铁匠铺对应第 1 阶重铸，熔岩铁匠铺对应第 2 阶重铸。
- 只允许游戏原版为当前装备类别和阶级定义的属性，不开放跨类别属性，也不允许重复选择当前同阶属性。
- 鼠标划过其他效果格子时，选择应保留；回到重铸效果列表后恢复显示。
- 候选失效或 UI 不可用时回退原版随机，材料仍按原版流程处理。
- 与修改 `T_Queen.ItemEnhance`、属性候选表或同一重铸 UI 的 Mod 可能冲突。
- 游戏更新后需要重新确认兼容性。
- 安装、更新和卸载必须先退出游戏；建议使用测试存档并备份重要存档。
- 日志位置为 `BepInEx/LogOutput.log` 和 `%USERPROFILE%/AppData/LocalLow/CasselGames/Ratopia/Player.log`。

## 真实性边界

允许陈述的验证结果：

- Release 自动测试共 `67/67` 通过。
- Release 构建为 `0` 警告、`0` 错误。
- Ratopia 包结构校验未发现禁止文件或意外文件。
- Release DLL、ZIP 内 DLL 和已安装 DLL 的 SHA-256 一致。
- 游戏启动日志确认插件 `0.1.2` 被发现，三个 Harmony 补丁安装成功，且当次日志没有本 Mod 的 Error/Exception。

不得写成已经完成的验证：

- 不宣称皇家铁匠铺和熔岩铁匠铺所有装备类型均已完成 `0.1.2` 最终实机验收。
- 不宣称已经完成两轮保存、退出、重载。
- 不宣称已经实际临时移除 `0.1.2` DLL 并验证原版读档。

页面以准确描述功能为主；如提到测试，仅区分“自动与包验证已完成”和“建议用户先在测试存档验证”，避免把启动日志当作完整玩法证明。

## 封面设计

### 规格

- 格式：PNG，仅提供一份。
- 尺寸：`1600 × 900`，16:9。
- 语言：英文主标题、中文副标题。
- 英文主标题：`EQUIPMENT REFORGE SELECTOR`
- 中文副标题：`装备重铸自选属性`
- 功能短句：`Choose the result. Keep vanilla balance.`

### 构图

封面以用户提供的 Ratopia 重铸界面截图为功能证据和视觉基础，突出右侧原版米色重铸效果列表及绿色选中箭头。主要布局：

- 右侧或中央保留清晰的候选列表与选中状态。
- 左侧设置深色半透明标题区，避免白字与游戏界面冲突。
- 标题使用高可读粗体无衬线字体；中文副标题尺寸低于英文主标题。
- 使用原界面的米色、锻造橙和选中绿色作为主色，不引入与游戏无关的霓虹或科幻元素。
- 添加小型 `BepInEx 5 • v0.1.2` 标签，但不在封面堆叠安装说明。

封面不得伪造游戏角色、装备、商标或不存在的 UI。不得把旧版窄文字点击状态误当作 `0.1.2` 的整行反馈；必要时使用构图与文案表达功能，而不是伪造未截图的最终控件。

### 视觉验收

- 在原始尺寸与缩略预览下检查标题拼写。
- 检查中文无乱码、缺字或错误字形。
- 检查标题和关键 UI 没有被裁切。
- 检查封面确实为 `1600 × 900` PNG。
- 最终只保留选定封面，不在交付目录留下草稿或多个格式。

## Mod ZIP

复制现有 `0.1.2` 发布 ZIP，并重命名为最终合同中的第 5 项。复制前后分别运行 Ratopia 包校验；ZIP 解压后必须可直接覆盖到游戏根目录。

ZIP 可以包含其原有的插件 DLL、README 和测试说明，但不得包含：

- 游戏或 Unity DLL
- BepInEx 或 Harmony DLL
- Mono.Cecil 或测试 DLL
- PDB、日志、存档、`bin`、`obj` 或开发路径

## 最终验证

先验证第 5 项 ZIP 的 Ratopia 包结构，再运行：

```powershell
& 'C:\Users\ASUS\.codex\skills\publishing-ratopia-nexus-mods\scripts\Test-RatopiaNexusDeliverables.ps1' `
  -Path '<最终交付目录>' `
  -ModName 'EquipmentReforgeSelector' `
  -Version '0.1.2'
```

只有脚本输出 `RATOPIA_NEXUS_DELIVERABLES_VALID=True` 才能宣布交付完成。最终回复只链接交付目录，并按 1 至 5 列出五个文件；封面只链接 PNG。
