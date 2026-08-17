# Equipment Reforge Selector Nexus Release Materials Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Produce one validator-clean Nexus Mods delivery folder containing exactly the five approved `0.1.2` release files.

**Architecture:** Treat the existing Ratopia ZIP and repository documentation as truth sources. Create three copy-ready text artifacts, generate and visually verify one 1600×900 PNG cover from the supplied gameplay screenshot, then assemble only those four artifacts plus a renamed copy of the validated Mod ZIP into an independent delivery folder. Validate the Mod archive and the five-file Nexus contract separately.

**Tech Stack:** Nexus BBCode, UTF-8 text, PNG, Ratopia BepInEx 5 Mono package, PowerShell 5-compatible validation, OpenAI image generation/editing.

## Global Constraints

- Final delivery directory is `D:\SOFTWARE\项目\鼠托邦mod\Nexus发布资料\装备重铸自选属性-v0.1.2`.
- Final directory contains exactly `1-英文标题.txt`, `2-简介.txt`, `3-双语完整介绍.txt`, `4-封面.png`, and `5-装备重铸自选属性-v0.1.2-BepInEx5.zip`.
- English page title is exactly `Equipment Reforge Selector`.
- Mod name is `装备重铸自选属性`; assembly name is `EquipmentReforgeSelector`; version is `0.1.2`.
- Cover is one `1600 × 900` PNG with English main title, Chinese subtitle, and no JPG/SVG alternative.
- Page copy is English first and Chinese second, and must not claim uncompleted final in-game save/reload or uninstall acceptance.
- Existing Mod ZIP must remain game-root extractable with `BepInEx/plugins/EquipmentReforgeSelector/EquipmentReforgeSelector.dll`.
- Do not include game, Unity, BepInEx, Harmony, Mono.Cecil, test, PDB, log, save, source, plan, or validation files in the final folder.
- Ratopia publishing work remains in the primary agent; do not use subagents.

---

### Task 1: Verify the source package and freeze factual claims

**Files:**
- Read: `src/EquipmentReforgeSelector/Plugin.cs`
- Read: `README.md`
- Read: `docs/TESTING.md`
- Read: `dist/装备重铸自选属性-v0.1.2-BepInEx5.zip`

**Interfaces:**
- Consumes: plugin identity, supported runtime, packaging layout, and recorded verification boundaries.
- Produces: confirmed constants `Equipment Reforge Selector`, `EquipmentReforgeSelector`, `0.1.2`, and the validated source ZIP path.

- [ ] **Step 1: Re-run the Ratopia package validator**

```powershell
& 'C:\Users\ASUS\.codex\skills\developing-ratopia-mods\scripts\Test-RatopiaPackage.ps1' `
  -Path 'D:\SOFTWARE\项目\鼠托邦mod\EquipmentReforgeSelector\dist\装备重铸自选属性-v0.1.2-BepInEx5.zip' `
  -ExpectedPluginName 'EquipmentReforgeSelector'
```

Expected: `ForbiddenFiles`, `UnexpectedFiles`, and `Errors` are empty; the only plugin DLL is under `BepInEx/plugins/EquipmentReforgeSelector/`.

- [ ] **Step 2: Verify version and title sources**

```powershell
rg -n 'PluginName|PluginGuid|PluginVersion|0\.1\.2|BepInEx 5|C94847D8' `
  'D:\SOFTWARE\项目\鼠托邦mod\EquipmentReforgeSelector\src\EquipmentReforgeSelector\Plugin.cs' `
  'D:\SOFTWARE\项目\鼠托邦mod\EquipmentReforgeSelector\README.md'
```

Expected: name `装备重铸自选属性`, GUID `cn.ratopia.equipmentreforgeselector`, version `0.1.2`, BepInEx 5, and the pinned assembly hash are present.

---

### Task 2: Create the three copy-ready Nexus text files

**Files:**
- Create: `D:\SOFTWARE\项目\鼠托邦mod\Nexus发布资料\装备重铸自选属性-v0.1.2\1-英文标题.txt`
- Create: `D:\SOFTWARE\项目\鼠托邦mod\Nexus发布资料\装备重铸自选属性-v0.1.2\2-简介.txt`
- Create: `D:\SOFTWARE\项目\鼠托邦mod\Nexus发布资料\装备重铸自选属性-v0.1.2\3-双语完整介绍.txt`

**Interfaces:**
- Consumes: verified facts from Task 1.
- Produces: UTF-8 title, bilingual summary, and bilingual Nexus BBCode description.

- [ ] **Step 1: Create the independent final directory after checking its exact resolved path**

Resolve the target and require it to remain below `D:\SOFTWARE\项目\鼠托邦mod\Nexus发布资料`. If an earlier directory with the exact version name exists, move it to a timestamped sibling backup rather than recursively deleting it.

- [ ] **Step 2: Create `1-英文标题.txt` with exactly one line**

```text
Equipment Reforge Selector
```

- [ ] **Step 3: Create `2-简介.txt`**

```text
Choose the exact vanilla reforge effect for equipment at the Royal Smithy and Lava Smithy. Select directly from Ratopia's original beige effect list with full-row mouse controls, number keys, or keyboard navigation—without changing material costs, effect values, achievements, or the vanilla save format.

在皇家铁匠铺和熔岩铁匠铺为装备自选原版重铸属性。直接在游戏原有的米色效果列表中整行点击，或使用数字键与键盘导航选择；不改变材料消耗、属性数值、成就和原版存档格式。
```

- [ ] **Step 4: Create `3-双语完整介绍.txt` using this exact BBCode structure and content**

```bbcode
[center][size=6][b]Equipment Reforge Selector[/b][/size]
[size=3]Choose the result. Keep vanilla balance.[/size][/center]

[size=5][b]Overview[/b][/size]
Ratopia normally chooses a random effect whenever equipment is reforged. This BepInEx 5 mod lets you select one of the valid vanilla effects before reforging at the Royal Smithy (tier 1) or Lava Smithy (tier 2).

Choices are shown inside the game's original beige effect list. Click the full candidate row, press its displayed number key, or use Up/Down and Enter after focusing a row.

[size=5][b]Features[/b][/size]
[list]
[*]Choose tier 1 reforge effects at the Royal Smithy.
[*]Choose tier 2 reforge effects at the Lava Smithy.
[*]Supports weapons, clothing, and accessories when vanilla data defines valid candidates.
[*]Uses only effects and values defined by the game for the current equipment category and tier.
[*]Excludes the effect already equipped at the same tier.
[*]Full-row mouse hit areas, number keys 1-9, and standard keyboard navigation.
[*]Keeps the current choice while other effect tooltips temporarily replace the beige panel, then restores its selection marker when the reforge list returns.
[*]Falls back to vanilla random reforging if the selection or game data becomes unavailable.
[/list]

[size=5][b]What This Mod Does Not Change[/b][/size]
[list]
[*]Material costs
[*]Effect strength or values
[*]Achievements and the original reforge flow
[*]The vanilla T_Queen.Dic_ItemPlusEffect save format
[*]Cross-category or otherwise invalid effects are not unlocked
[/list]

[size=5][b]Requirements[/b][/size]
[list]
[*]Ratopia (Mono build)
[*]BepInEx 5
[*].NET Framework 4.7.2 runtime environment used by the game/mod loader
[/list]

Compatibility was built against the local Assembly-CSharp.dll dated 2026-07-24 with SHA-256 C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D. Recheck compatibility after a game update.

[size=5][b]Installation[/b][/size]
[list=1]
[*]Close Ratopia completely.
[*]Install BepInEx 5 if it is not already installed.
[*]Extract this archive into the Ratopia game directory while preserving its folders.
[*]Confirm the DLL is located at BepInEx/plugins/EquipmentReforgeSelector/EquipmentReforgeSelector.dll.
[*]Do not keep duplicate copies or older DLLs with the same plugin GUID.
[/list]

[size=5][b]Usage[/b][/size]
[list=1]
[*]Open supported equipment at the Royal Smithy or Lava Smithy.
[*]Open the reforge-effect tooltip.
[*]Click a candidate row or press its number key. The selected row shows a green marker and selected status.
[*]Continue with the original reforge action.
[/list]

[size=5][b]Compatibility and Conflicts[/b][/size]
Mods that patch T_Queen.ItemEnhance, replace the same reforge UI, or edit the equipment-effect candidate tables may conflict. When troubleshooting, test with only one reforge-related mod enabled.

[size=5][b]Save Safety and Uninstallation[/b][/size]
This mod adds no custom save fields and writes the final result through the game's original equipment-effect dictionary. Back up important saves and test on a non-critical save first. Final two-cycle save/reload and temporary-removal acceptance are not claimed for this release.

To uninstall, close Ratopia and remove BepInEx/plugins/EquipmentReforgeSelector/EquipmentReforgeSelector.dll (or its dedicated folder). Never replace or remove the DLL while the game is running.

[size=5][b]Troubleshooting[/b][/size]
If the selector is unavailable, the mod records a warning and allows the vanilla random path instead of blocking reforging. Include the relevant log section and your game/mod versions when reporting a problem.

Logs:
[list]
[*]BepInEx/LogOutput.log
[*]%USERPROFILE%/AppData/LocalLow/CasselGames/Ratopia/Player.log
[/list]

[size=5][b]Version[/b][/size]
0.1.2 | Plugin GUID: cn.ratopia.equipmentreforgeselector

[center]────────────────────────[/center]

[center][size=6][b]装备重铸自选属性[/b][/size]
[size=3]自选结果，保持原版平衡。[/size][/center]

[size=5][b]概述[/b][/size]
《鼠托邦》的装备重铸原本会随机选择属性。本 BepInEx 5 Mod 允许玩家在皇家铁匠铺（第 1 阶）或熔岩铁匠铺（第 2 阶）重铸前，从当前装备可用的原版属性中自行选择。

候选直接显示在游戏原有的米色效果列表内。可以点击候选整行、按该行显示的数字键，或在候选获得焦点后使用上下键和回车。

[size=5][b]功能[/b][/size]
[list]
[*]在皇家铁匠铺自选第 1 阶重铸属性。
[*]在熔岩铁匠铺自选第 2 阶重铸属性。
[*]当原版数据存在有效候选时，支持武器、衣服和饰品。
[*]只使用游戏为当前装备类别和阶级定义的属性与数值。
[*]排除当前同阶已经拥有的属性。
[*]支持整行鼠标热区、数字键 1-9 和标准键盘导航。
[*]鼠标划过其他效果提示导致米色面板临时刷新时保留当前选择；重铸列表返回后恢复选中标记。
[*]选择或游戏数据不可用时，安全回退到原版随机重铸。
[/list]

[size=5][b]不会修改的内容[/b][/size]
[list]
[*]材料消耗
[*]属性强度和数值
[*]成就与原版重铸流程
[*]原版 T_Queen.Dic_ItemPlusEffect 存档格式
[*]不会开放跨装备类别或其他无效属性
[/list]

[size=5][b]前置需求[/b][/size]
[list]
[*]Ratopia（《鼠托邦》Mono 版本）
[*]BepInEx 5
[*]游戏与 Mod 加载器使用的 .NET Framework 4.7.2 运行环境
[/list]

本版本基于本地日期为 2026-07-24、SHA-256 为 C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D 的 Assembly-CSharp.dll 构建。游戏更新后请重新确认兼容性。

[size=5][b]安装[/b][/size]
[list=1]
[*]完全退出《鼠托邦》。
[*]如果尚未安装，请先安装 BepInEx 5。
[*]将压缩包解压到《鼠托邦》游戏根目录，并保持目录结构。
[*]确认 DLL 位于 BepInEx/plugins/EquipmentReforgeSelector/EquipmentReforgeSelector.dll。
[*]不要保留同一插件 GUID 的重复副本或旧版 DLL。
[/list]

[size=5][b]使用方法[/b][/size]
[list=1]
[*]在皇家铁匠铺或熔岩铁匠铺打开支持的装备。
[*]显示该阶级的重铸效果提示。
[*]点击候选整行或按对应数字键；选中行会显示绿色标记和“已选择”状态。
[*]继续执行原版重铸操作。
[/list]

[size=5][b]兼容性与冲突[/b][/size]
修改 T_Queen.ItemEnhance、替换同一重铸界面或编辑装备属性候选表的 Mod 可能发生冲突。排查问题时，请只启用一个重铸相关 Mod 进行测试。

[size=5][b]存档安全与卸载[/b][/size]
本 Mod 不添加自定义存档字段，最终结果仍写入游戏原有的装备效果字典。建议备份重要存档，并先在非关键存档中测试。本发布版本不宣称已经完成两轮保存/重载和临时移除 DLL 的最终实机验收。

卸载时请先退出游戏，再删除 BepInEx/plugins/EquipmentReforgeSelector/EquipmentReforgeSelector.dll（或其独立文件夹）。不要在游戏运行时替换或删除 DLL。

[size=5][b]故障排查[/b][/size]
如果选择器不可用，Mod 会记录警告并允许使用原版随机流程，不会阻止重铸。报告问题时请附上相关日志片段以及游戏和 Mod 版本。

日志位置：
[list]
[*]BepInEx/LogOutput.log
[*]%USERPROFILE%/AppData/LocalLow/CasselGames/Ratopia/Player.log
[/list]

[size=5][b]版本[/b][/size]
0.1.2 | 插件 GUID：cn.ratopia.equipmentreforgeselector
```

- [ ] **Step 5: Verify text encoding and content contracts**

```powershell
$folder = 'D:\SOFTWARE\项目\鼠托邦mod\Nexus发布资料\装备重铸自选属性-v0.1.2'
$title = Get-Content -LiteralPath (Join-Path $folder '1-英文标题.txt') -Raw
$summary = Get-Content -LiteralPath (Join-Path $folder '2-简介.txt') -Raw
$description = Get-Content -LiteralPath (Join-Path $folder '3-双语完整介绍.txt') -Raw
if ($title.Trim() -ne 'Equipment Reforge Selector') { throw 'Title mismatch.' }
foreach ($token in @('Royal Smithy','Lava Smithy','BepInEx 5','皇家铁匠铺','熔岩铁匠铺','存档','LogOutput.log')) {
    if (-not ($summary + $description).Contains($token)) { throw "Missing copy token: $token" }
}
```

Expected: no exception; all three files decode as UTF-8 and contain the approved factual claims.

---

### Task 3: Generate and visually verify the single PNG cover

**Files:**
- Source image: `C:\Users\ASUS\AppData\Local\Temp\codex-clipboard-2a725df1-120c-42cb-8a83-a76f302be1eb.png`
- Create: `D:\SOFTWARE\项目\鼠托邦mod\Nexus发布资料\装备重铸自选属性-v0.1.2\4-封面.png`

**Interfaces:**
- Consumes: approved 16:9 cover design and the supplied gameplay screenshot.
- Produces: one upload-ready 1600×900 PNG.

- [ ] **Step 1: Read the image-generation skill and inspect the source screenshot at original detail**

Confirm the screenshot contains the Royal Smithy UI, the beige reforge-effect panel, and the green selected-arrow state without cropping those elements.

- [ ] **Step 2: Generate/edit the cover with the supplied screenshot as the reference**

Prompt requirements:

```text
Create a polished 16:9 Nexus Mods cover from the supplied Ratopia gameplay screenshot. Preserve the real game UI and the beige reforge-effect candidate panel as recognizable evidence. Build a dark translucent title area on the left, keep the reforge list and green selected arrow clear on the right, and use the game's beige, forge-orange, teal, and selected-green palette. Exact text, with correct spelling and no additional words:
EQUIPMENT REFORGE SELECTOR
装备重铸自选属性
Choose the result. Keep vanilla balance.
BepInEx 5 • v0.1.2
Use highly legible bold sans-serif typography, correct Chinese glyphs, safe margins, and a professional mod-page composition. Do not invent characters, weapons, logos, buttons, UI states, or fake gameplay. Output one 1600×900 PNG.
```

- [ ] **Step 3: Inspect the generated cover visually**

Check all exact English text, every Chinese character, key UI visibility, safe margins, cropping, and absence of invented gameplay. If any text or crop is wrong, edit the generated cover with a narrowly scoped correction prompt and inspect again.

- [ ] **Step 4: Verify the file contract**

Use local image metadata to require PNG format and exact dimensions `1600 × 900`. Confirm the final directory contains no JPG, SVG, alternate cover, or thumbnail.

---

### Task 4: Add the Mod ZIP and validate the exact five-file delivery

**Files:**
- Copy from: `D:\SOFTWARE\项目\鼠托邦mod\EquipmentReforgeSelector\dist\装备重铸自选属性-v0.1.2-BepInEx5.zip`
- Create: `D:\SOFTWARE\项目\鼠托邦mod\Nexus发布资料\装备重铸自选属性-v0.1.2\5-装备重铸自选属性-v0.1.2-BepInEx5.zip`

**Interfaces:**
- Consumes: validated source ZIP and Tasks 2-3 artifacts.
- Produces: exact five-file folder accepted by the Nexus deliverables validator.

- [ ] **Step 1: Copy the ZIP without rebuilding or modifying its contents**

Use `Copy-Item -LiteralPath` and compare SHA-256 hashes before and after; require equality.

- [ ] **Step 2: Validate the copied Ratopia package independently**

```powershell
& 'C:\Users\ASUS\.codex\skills\developing-ratopia-mods\scripts\Test-RatopiaPackage.ps1' `
  -Path 'D:\SOFTWARE\项目\鼠托邦mod\Nexus发布资料\装备重铸自选属性-v0.1.2\5-装备重铸自选属性-v0.1.2-BepInEx5.zip' `
  -ExpectedPluginName 'EquipmentReforgeSelector'
```

Expected: no forbidden files, unexpected files, or errors.

- [ ] **Step 3: Enumerate the final directory and require the exact whitelist**

```powershell
$expected = @(
  '1-英文标题.txt',
  '2-简介.txt',
  '3-双语完整介绍.txt',
  '4-封面.png',
  '5-装备重铸自选属性-v0.1.2-BepInEx5.zip'
)
$actual = @(Get-ChildItem -LiteralPath 'D:\SOFTWARE\项目\鼠托邦mod\Nexus发布资料\装备重铸自选属性-v0.1.2' -File | Select-Object -ExpandProperty Name)
if (@(Compare-Object $expected $actual).Count -ne 0) { throw 'Final folder does not match the five-file contract.' }
```

- [ ] **Step 4: Run the publishing skill's final validator**

```powershell
& 'C:\Users\ASUS\.codex\skills\publishing-ratopia-nexus-mods\scripts\Test-RatopiaNexusDeliverables.ps1' `
  -Path 'D:\SOFTWARE\项目\鼠托邦mod\Nexus发布资料\装备重铸自选属性-v0.1.2' `
  -ModName 'EquipmentReforgeSelector' `
  -Version '0.1.2'
```

Expected: `RATOPIA_NEXUS_DELIVERABLES_VALID=True`.

- [ ] **Step 5: Final visual and repository audit**

Re-open `4-封面.png`, read the three text files, check `git status --short`, and confirm no delivery helper file was added to the five-file folder. Do not claim unperformed gameplay acceptance.
