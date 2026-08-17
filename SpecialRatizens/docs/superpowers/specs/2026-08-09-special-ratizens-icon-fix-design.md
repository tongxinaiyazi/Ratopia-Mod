# 特殊鼠鼠 v0.1.4 能力图标修复设计

## 目标

修复 24 个特殊能力在特性卡、状态栏和获得状态飘字中显示 `Missing`、空图标或错误原版图标的问题，同时保证普通鼠和原版特性图标不被覆盖。

## 已确认的根因

1. 独立版已经把 CSV 数据目录迁移到 `BepInEx/plugins/SpecialRatizens/Data`，但 `RegisterCustomInfoIcon` 仍从原整合模组的相对路径 `CustomSetting_Data/Icon` 加载图片。最新 `Player.log` 对实际加载的自定义图标逐一记录了 `load non byte`，而旧目录在游戏和插件目录中均不存在。
2. 自定义图标继续使用 `Icon_Char{CharacterInfo.Index}` 作为 `Func.Dic_Resource` 全局键。键已存在时旧实现直接返回，既不加载当前特殊能力的 PNG，也不为该特性设置 `iconKey`。当前运行日志中 24 个图标只有 12 个进入文件加载，其余 12 个被已有键提前跳过。
3. `BuffIcon.IconSet` 后置补丁按基础 `C_Buff` 名称再次加载图标，没有使用特殊能力已经注册的 `iconKey`，因此状态栏可以继续显示 `Missing` 或错误的基础状态图标。
4. 发行包中的 24 张 PNG 均存在、引用唯一且可以解码；问题位于加载路径、全局键和 UI 消费链，不在图片内容。

## 选定方案

每个特殊能力使用与原版资源完全隔离的确定性主键：

```text
SpecialRatizens.Icon.<trait-name>
```

例如五禽戏使用 `SpecialRatizens.Icon.HT_WQX`。状态、详情和飘字统一使用主键，因此角色数据库索引变化、预加载顺序和原版资源数量都不会造成碰撞。

真实游戏程序集同时证明，多处特性卡 UI 直接拼接 `Icon_Char{CharacterInfo.Index}`，不能改为读取 `RefInfo`。因此注册阶段还会建立一个兼容别名 `Icon_Char{Index}`：只有在 `DB_Mgr.GetCharacterInfo(Index)` 确认该索引当前确实属于同一个自定义特性时，才让该别名指向同一张 Sprite。游戏的索引查询先查特性 1 再查特性 2，所以此检查也能阻止跨分类索引冲突影响普通特性。

## 数据流

1. `ConfigureSpecialRatizens` 保存插件实际 `Data` 根目录。
2. `RegisterCustomCharInfo` 根据特性名生成独立主键，并在任何字典判断前写入该特性的 `iconKey`。
3. PNG 从 `Path.Combine(CustomDataPath, "Icon", iconAddress)` 加载；`iconAddress` 仍来自 `CustomSpecialUnit.csv`，以保持当前 24 张外部图片可替换。
4. 成功加载的 `Sprite` 以独立主键写入 `Func.Dic_Resource`。重复数据库加载使用赋值更新同一键，保持幂等，不新增重复键。
5. 注册函数确认 `CharacterInfo.Index` 由当前自定义特性拥有，再将 `Icon_Char{Index}` 兼容别名指向同一 Sprite；所有权不匹配时中止初始化，不覆盖现有图标。
6. `CitizenBuff.RefInfo.GetIconAddress` 对 24 个自定义特性返回独立主键；原版特性继续执行原方法。
7. `BuffIcon.IconSet` 对自定义 `ReferenceName` 直接使用同一个独立主键；非自定义状态不修改。
8. 游戏的 `RefInfo.GetIcon()`、`GetEffect.GetRefEffect()` 通过 `Func.LoadSprite` 消费主键；候选、市民状态页、Tooltip 和能力槽等特性卡 UI 通过同一个加载器消费已验证的索引别名。

## 失败与隔离行为

- 数据预检继续要求 24 个 CSV 图标引用都对应实际文件。
- 注册时若特性名、图标地址、资源字典或 Sprite 无效，抛出包含特性名和绝对文件路径的初始化错误；插件现有入口会回滚本插件 Harmony 补丁并停用，避免把 `Missing` 静默注册成有效资源。
- 主键只写入 `SpecialRatizens.Icon.*`。索引别名只在游戏数据库证明该索引属于当前自定义特性后更新；不删除原版 Resources，不修改任何由普通特性拥有的键。
- 重置会话不销毁外部 Sprite；角色数据库重新加载时原键被幂等更新，避免仍被 UI 引用的 Sprite 突然失效。
- 本次不改变特性索引、CSV 顺序、角色数据、存档结构、效果数值或皮肤逻辑。

## 测试设计

先建立失败测试，再修改生产代码：

1. 发行数据测试：24 个特性映射到 24 个存在且唯一的 PNG。
2. 图标键纯逻辑测试：24 个特性生成唯一的 `SpecialRatizens.Icon.*` 主键，并为实际数据库索引生成 `Icon_Char{Index}` 兼容别名。
3. 静态合同测试：注册函数从 `CustomDataPath/Icon` 构造路径，不包含 `CustomSetting_Data/Icon`。
4. 静态合同测试：注册逻辑在字典存在判断之外设置 `iconKey`，并使用幂等赋值而非遇到同名键直接返回。
5. 静态合同测试：写入 `Icon_Char{Index}` 前验证 `DB_Mgr.GetCharacterInfo(Index)` 的名称与当前自定义特性一致。
6. 静态合同测试：`BuffIcon_IconSet` 使用 `CustomCharInfo.iconKey`，非自定义状态保持原版路径。
7. 游戏程序集合同：`RefInfo.GetIcon()` 与 `GetEffect.GetRefEffect()` 仍通过 `Func.LoadSprite` 消费图标地址；能力槽、候选卡、市民状态页和 Tooltip 仍通过同一加载器消费索引别名。
8. 全量回归：现有外观、女王、繁荣、24 特性、会话隔离、包结构和版本合同全部通过。

## 发布和验收

- 版本统一升级为 v0.1.4。
- 先构建和测试，不自动安装；包结构再次扫描，禁止包含游戏、Unity、BepInEx 或 Harmony DLL。
- 安装前确认 Ratopia 进程退出，备份当前 v0.1.3 插件和存档，再覆盖插件目录并逐文件比较哈希。
- 不自动启动游戏、不自动保存。
- 游戏内验收至少检查五禽戏以及其余 23 个特殊能力在特性卡、状态栏和飘字中的图标；同时抽查普通鼠和原版特性图标没有变化，并确认日志不存在 `load non byte from CustomSetting_Data/Icon`、空图标键或 `LoadSprite Fail`。
