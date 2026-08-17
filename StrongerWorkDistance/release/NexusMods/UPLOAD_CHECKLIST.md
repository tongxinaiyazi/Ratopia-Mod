# Nexus Mods 上传检查表

## 页面字段

- 游戏：`Ratopia`
- 英文标题：`Stronger Work Distance`
- 版本：`0.1.0`
- 成人内容：`No`
- 建议分类：优先选择 `Gameplay`；页面没有该分类时选择 `Miscellaneous`
- 简介：复制 `NEXUS_SUMMARY.txt`
- 正文：复制 `NEXUS_DESCRIPTION.txt`
- 封面：上传 `images/StrongerWorkDistance-cover-1600x900.png`

## 主文件

- 上传文件：`dist/更强大的工作距离-v0.1.0-BepInEx5.zip`
- 文件显示名：`Stronger Work Distance v0.1.0 - BepInEx 5`
- 文件版本：`0.1.0`
- 文件说明：复制 `FILE_DESCRIPTION.txt`
- 更新记录：复制 `CHANGELOG.txt`
- Requirements 中添加 `BepInEx 5`，正文保持精确版本 `5.4.23.5`
- 当前主文件 SHA-256：`015B22B2BE375EA3EE62D0E0698DFCED9DB61CA5531EDD18E686AFC6EFEA97F8`

## 已完成的静态检查

- [x] ZIP 路径固定为 `BepInEx/plugins/StrongerWorkDistance/StrongerWorkDistance.dll` 与 `README.md`
- [x] ZIP 不包含游戏、Unity、BepInEx、Harmony DLL、PDB、日志或存档
- [x] 文案中的版本、安装路径、功能范围和 README 一致
- [x] 页面不宣称 Cassel Games、BepInEx 或 Harmony 作者为本 Mod 背书

## 公开上传前必须完成的实机检查

- [ ] 进入受控测试存档，分别验证采矿、建造、拆除和维修在 4 格高与 2 格远的位置可工作，且测试布局没有更近替代站位
- [ ] 验证 5 格高与 3 格远的位置不能工作
- [ ] 保存、退出并重新读档，确认插件再次加载且工作范围正常
- [ ] 临时移除 Mod，确认原版仍能读取存档并恢复默认工作距离；随后重新安装并确认正常加载
- [ ] 发布后下载一次 Nexus 扫描完成的文件，重新检查 ZIP 结构和 SHA-256

## 发布说明

- 根据 Nexus Mods 上传页面当时提供的选项，如实填写作者、权限与 AI 辅助信息
- 不要把未勾选的实机检查描述成已经通过
- 游戏更新后若 `Assembly-CSharp.dll` 哈希变化，应重新做兼容性验证后再更新页面
