# 特殊鼠鼠外观刷新与普通市民隔离修复设计

## 背景

`SpecialRatizens` 0.1.0 在 Ratopia 1.0.0600、BepInEx 5.4.23.5 和 Harmony 2.9.0 环境中能够注册 12 名特殊鼠鼠及 24 个特性，但实机生成特殊鼠鼠后出现身体或服装附件缺失。当前迁移版还保留了原模组的普通市民自定义皮肤状态，可能在同一游戏进程第二次读档时把普通市民错误地纳入换装流程。

日志已证明特殊鼠鼠在创建时完成了皮肤数据注册。对当前 `Assembly-CSharp.dll` 的 Mono.Cecil 检查证明，原版 `Sp_SkinInfo.UpdateCombinedSkin()` 会依次调用 `Skeleton.SetSkin(m_Skin)` 和 `Skeleton.SetSlotsToSetupPose()`。迁移代码只执行 `ClearSkins → AssembleData → SkinSet → SetSlotsToSetupPose`，没有把新组合皮肤重新安装到 Spine 骨架。

## 目标

- 特殊鼠鼠生成、读档和默认/工作服切换后都显示完整身体与服装。
- 普通市民外观完全由原版游戏管理，`SpecialRatizens` 不创建、不读取、不保存也不应用普通市民自定义皮肤。
- 同一进程切换或重复读取存档时，不复用上一会话的任何皮肤运行时状态。
- 保留现有特殊鼠鼠生成概率、属性、特性和特性效果。
- 发布版本升级为 `0.1.1`，重新打包并在游戏关闭时覆盖安装。

## 非目标

- 不恢复原模组的普通市民自定义皮肤界面或功能。
- 不修改 Ratopia 存档格式、游戏文件或其他已安装 Mod。
- 不自动启动游戏、不自动读档或保存。
- 不重写整套特殊鼠鼠外观系统。

## 修复方案

### 特殊鼠鼠 Spine 刷新

`CustomMOD.UpdateUnitCustomSkin` 使用与当前游戏原版一致的最终刷新步骤：

1. 市民对象先调用 `ClearSkins()`，清空旧的部位选择。
2. `SpineDresserMgr.AssembleData` 按特殊鼠鼠名称把组合数据写入 `Sp_SkinInfo`。
3. `SkinSet(m_Skin, m_SkeletonData)` 重建组合 `Spine.Skin`。
4. `UpdateCombinedSkin()` 把组合皮肤安装到 `Skeleton` 并恢复插槽姿势。

现有直接调用 `m_Skeleton.SetSlotsToSetupPose()` 的代码删除，避免重复执行并确保 `Skeleton.SetSkin` 不再缺失。

### 普通市民隔离

`LoadCitizenDatas` 对无法识别为特殊鼠鼠的市民只记录和维护与特殊特性相关的通用数据，不再调用 `TryGetCitizenCustomSkin` 或 `UpdateUnitSpineDress`。

`UpdateClothes` 只处理已确认的特殊鼠鼠。普通市民立即返回 `false`，使 Harmony Prefix 放行原版 `DefaultClothesUpdate` 或 `ClothesUpdate`。

原有普通市民皮肤字段可以保留为兼容遗留源码的内部实现，但不能再进入生成、读档或服装切换运行路径。

### 会话清理

`ResetSpecialRatizensSession` 除现有特殊鼠鼠字典外，还清空：

- `CitizenCustomSkins`
- `OpenedCitizenInfo`
- `OpenedSpcialCitizen`
- `EditingCustomSkins`
- `EditingCustomSkinIndex`

清理只影响 Mod 的内存状态，不写入游戏存档。读档完成后由现有会话补丁重新发现特殊鼠鼠并重建其皮肤数据。

## 异常处理

- 外观 Prefix 仍由 `LegacyPatchAdapters` 隔离异常；发生异常时返回 `true`，放行原版服装逻辑。
- 会话加载和清理由 `Plugin.RunSafely` 及 `OnDestroy` 的现有保护处理，不让 Mod 异常传播到游戏主循环。
- 不吞掉诊断信息；异常继续写入 BepInEx 日志并包含补丁操作名。

## 测试设计

先增加会在 0.1.0 上失败的 Mono.Cecil 合同测试，再修改生产代码：

- `UpdateUnitCustomSkin` 必须调用一次 `Sp_SkinInfo.UpdateCombinedSkin`，且不再直接调用 `Skeleton.SetSlotsToSetupPose`。
- `LoadCitizenDatas` 不得调用 `TryGetCitizenCustomSkin` 或为普通市民调用 `UpdateUnitSpineDress`。
- `UpdateClothes` 的普通市民分支不得读取 `CitizenCustomSkins`。
- `ResetSpecialRatizensSession` 必须清空普通市民皮肤字典和编辑会话状态。
- 插件、程序集、包名和 README 版本统一为 `0.1.1`。

随后运行完整 Release 测试、干净构建、包结构检查和 ZIP 完整性检查。离线测试不能证明最终画面正确，因此安装后仍需用户进入备份存档进行一次不保存的人工验收。

## 安装与回滚

安装前必须确认 Ratopia 已退出，并备份：

- 当前已安装的 `SpecialRatizens.dll` 与 Data 目录。
- 当前 Ratopia 存档。

只覆盖 `E:\steam\steamapps\common\Ratopia\BepInEx\plugins\SpecialRatizens` 内的本 Mod 文件。安装后比较构建 DLL、打包 DLL 和已安装 DLL 的 SHA-256 与程序集版本，不改动其他插件。

若人工验收仍出现外观问题，关闭游戏后恢复安装前的插件备份；存档未保存时无需恢复存档。若已经保存，则同时恢复安装前存档备份。

## 验收标准

- 自动测试全部通过，Release 构建为 0 错误、0 警告。
- 发布包无游戏、Unity、BepInEx、Harmony、PDB、测试或存档文件。
- 已安装 DLL 与发布 DLL 哈希一致，其他插件哈希不变。
- 新生成特殊鼠鼠身体、衣服和附件完整。
- 已有特殊鼠鼠读档后外观完整，默认服装和工作服切换后仍完整。
- 普通市民首次读档和同一进程第二次读档均保持原版外观。
