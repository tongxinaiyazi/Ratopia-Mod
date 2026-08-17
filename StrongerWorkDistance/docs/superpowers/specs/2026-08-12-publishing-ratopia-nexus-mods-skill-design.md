# 鼠托邦 Nexus Mods 发布 Skill 设计规格

## 目标

创建个人 Skill `publishing-ratopia-nexus-mods`，用于制作 Ratopia BepInEx 5 Mod 的 Nexus Mods 最终发布资料。Skill 必须让最终交付保持紧凑、直接可用，避免再次把开发文档、检查表、源图片或重复图片格式混入交付目录。

## 触发范围

当用户要求为鼠托邦 Mod 准备 Nexus/N 网发布、上传资料、页面文案、封面或发布包时触发。它是现有 `developing-ratopia-mods` 的发布交付专用补充；Mod 包仍必须按 Ratopia 专用技能的规则进行结构与禁止文件验证。

## 固定交付契约

最终独立目录严格包含以下 5 个文件，不多也不少：

1. `1-英文标题.txt`
2. `2-简介.txt`
3. `3-双语完整介绍.txt`
4. `4-封面.png`，或用户明确要求时改为 `4-封面.jpg`
5. `5-<模组名>-v<版本>-BepInEx5.zip`

默认封面格式为 PNG。若用户明确指定 JPG，则只输出 JPG。不得同时输出 PNG 与 JPG；SVG、PSD、源图、预览图和缩略图不得进入最终目录。

发布资料之外的设计规格、实施计划、验证脚本、哈希清单、上传检查表或 README 可存在于开发仓库，但不得放入最终交付目录。

## 内容要求

- 英文标题适合作为 Nexus 页面标题。
- 简介直接可复制到 Nexus 摘要字段；除非用户明确只要英文，否则使用简短双语简介。
- 双语完整介绍使用 Nexus BBCode，英文在前、中文在后，并覆盖功能、范围边界、环境要求、安装、卸载、存档风险、冲突和排错。
- 不把尚未实际完成的游戏内验证写成已通过。
- 发布 ZIP 是可直接解压到 Ratopia 根目录的最终 Mod 包，不是包含文案和封面的资料合集。

## Skill 结构

安装目录：`C:\Users\ASUS\.codex\skills\publishing-ratopia-nexus-mods`。

- `SKILL.md`：触发条件、固定交付契约、制作顺序、封面格式决策和完成门禁。
- `agents/openai.yaml`：Skill 列表显示名称、简介和默认提示。
- `scripts/Test-RatopiaNexusDeliverables.ps1`：验证最终目录恰好含 5 个文件、只有一种 PNG/JPG 封面、文本不为空、ZIP 命名和版本一致，并调用 Ratopia 包验证脚本检查 Mod ZIP。

不创建 README、变更记录或额外参考文档。

## 验证

- 记录本次真实失败作为无 Skill 的 RED 基线：交付曾包含额外检查表/开发文档，并同时生成 PNG 与 JPG。
- 使用系统 `quick_validate.py` 验证 Skill 元数据、目录名和 YAML。
- 在临时目录建立一个包含双格式封面的错误样例，确认脚本失败。
- 使用当前 `StrongerWorkDistance` 的 5 项交付物建立仅含单一 PNG 的正确样例，确认脚本通过。
- 确认正确样例中的 ZIP 通过 `Test-RatopiaPackage.ps1`，且不含游戏、Unity、BepInEx、Harmony 或调试文件。

