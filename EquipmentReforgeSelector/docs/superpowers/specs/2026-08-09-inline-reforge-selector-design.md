# 装备重铸自选属性：原版面板内嵌选择器设计

## 问题与根因

当前 Mod 在 `BuildMidUI.Obj_Main` 右侧创建固定宽度的独立深色面板。Ratopia 的重铸候选实际显示在常驻的米色 `SimpleToolTip` 面板中；该面板拥有自己的显示层级，并覆盖独立面板的大部分区域，导致用户只能看到右侧残片，无法正常操作。

程序集检查确认：

- `BuildMidUI.ItemDetail_Open(ItemInfo, bool, bool, int)` 在升级模式下调用 `BuildMidUI_ItemEffect.CustomEffectSet`。
- 原版效果条目将 `SimpleToolTipList.EnhanceEffect`、装备类型和重铸等级传给 `SimpleToolTip.SimpleToolTipSet`。
- `EnhanceEffect` 分支使用私有字段 `SimpleToolTip.m_EffectFrame`，调用 `Batch_ResEffect.ResEffectSet` 将候选写入公开的 `TextMeshProUGUI[] Txt_Value`。

因此应复用原版米色面板中的效果行，而不是继续调整独立面板的坐标或排序。

## 方案比较

### A. 复用原版 `Batch_ResEffect` 条目（采用）

把 `Txt_Value` 中当前可见的原版效果行转换为可点击按钮，继续使用原版图标、字体、米色背景和布局。选择项使用原版绿色箭头样式标记。

优点：自然适配分辨率、本地化、滚动和原版层级；没有额外浮层。缺点：需要严格管理临时添加的按钮组件和事件监听。

### B. 在米色面板内另建子面板

在 `SimpleToolTip` 下创建新的候选容器，不复用原有效果行。

优点：布局自由。缺点：容易与原版自动高度、背景尺寸和文本布局冲突，需要复制原版视觉。

### C. 用独立 Canvas 覆盖米色效果区

保持自绘面板，使用更高排序并对准米色区域。

优点：改动现有代码较少。缺点：坐标和排序依赖分辨率、UI 缩放与 Canvas 模式，仍容易发生遮挡或错位。

## 运行时结构

### Harmony 接入

- 保留 `BuildMidUI.ItemDetail_Open` Postfix，用于确认装备、重铸等级和候选会话，但不再创建右侧面板。
- 新增 `SimpleToolTip.SimpleToolTipSet(SimpleToolTipList, float, float, float)` Postfix。
- Postfix 仅在 `_value == EnhanceEffect`，且装备类型、等级与当前重铸会话一致时绑定内嵌选择器。
- `T_Queen.ItemEnhance` 的 Prefix/Postfix/Finalizer 逻辑保持不变，仍只在单次原版调用期间替换候选列表并恢复原引用。

### 内嵌视图

- `InlineReforgeSelectorView` 绑定 `Batch_ResEffect.Txt_Value`，不创建新的面板或 Canvas。
- 每个有效候选占用一个原版文本行；文字仍由 `Helpers.GetToolTipString` 生成。
- 每行临时添加 `Button`，整行可点击；按钮索引映射到当前候选快照。
- 当前选择使用原版 `<sprite name=FS_P_Right_White>` 箭头和 `Defines.Hex_DeepGreen` 颜色。
- 候选获得焦点后，上下键按行移动，回车通过 Unity `Button` 的标准 Submit 确认；不主动抢占初始焦点。
- 不再实例化 `EquipmentReforgeSelectorPanel` 深色侧栏。

### 状态与数据流

1. `ItemDetail_Open` 完成后，控制器解析并排除当前同阶属性，默认选择第一个有效候选。
2. 原版打开 `EnhanceEffect` 米色面板并完成 `ResEffectSet`。
3. `SimpleToolTipSet` Postfix 验证装备类型和等级，将候选快照交给内嵌视图。
4. 点击行后，候选索引立即转换为完整 `ReforgeCandidate(AbilityId, Value)` 并保存；视图只更新行文字与高亮。
5. 重铸时再次根据最新游戏数据验证完整候选，然后沿用现有作用域覆盖与恢复逻辑。

## 生命周期与回退

- 米色面板关闭、换装备、换等级、切场景或插件销毁时，立即禁用本 Mod 专属按钮、清除监听并释放绑定。
- 原版会在同一帧内停用并重新启用效果区，而 Unity `Selectable` 禁止重复 Button；因此专属按钮组件保持禁用并由下一次绑定安全复用，避免延迟销毁竞争。禁用状态没有监听，不会影响其他提示框。
- 如果原版字段、候选或视图状态失效，不阻止重铸；在原版效果区显示“使用原版随机”警告并记录日志。
- 其他类型的 `SimpleToolTip` 不添加按钮、不修改文字。
- 不写入新存档字段，也不改变材料、数值、成就或原版 `Dic_ItemPlusEffect`。

## 测试与验收

- TDD 增加纯逻辑测试：候选行映射、精确选中重复 AbilityId 的不同 Value、默认选择、失效索引、上下导航。
- Mono.Cecil 合同锁定 `SimpleToolTipSet` 签名、`EnhanceEffect` 枚举值、`m_EffectFrame` 与 `Batch_ResEffect.Txt_Value`。
- 源码/结构合同确认不再创建右侧面板或使用固定右侧锚点，并确认只对 `EnhanceEffect` 绑定。
- Release 全量测试、构建、打包验证继续关闭自动安装。
- Ratopia 关闭后备份旧 DLL，再安装新构建。
- 实机确认皇家铁匠铺与熔岩铁匠铺的米色效果列表可直接点击，选择高亮可见，键盘可操作，重铸结果与选择一致；其他提示框不受影响。
