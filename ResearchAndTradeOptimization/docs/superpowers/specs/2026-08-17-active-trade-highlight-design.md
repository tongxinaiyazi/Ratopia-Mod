# 国家详情贸易中商品高亮设计

## 目标

在国家详情右侧的进口、出口列表中，为当前正在执行贸易协议的商品增加视觉高亮，让玩家一眼区分"正在交易"与"仅可交易"的候选商品。高亮采用背景色块加图标描边，并区分期限类型：有限期贸易默认 `rgb(145, 135, 106)`，无限期贸易默认 `rgb(96, 169, 23)`，两种颜色均可通过 BepInEx 配置自定义。

## 已确认原版结构

- `DiplomaticWorldDetailUI.Refresh()` 分别调用 `_importsLayoutUI.SetData(countryToHometownArray, ...)` 和 `_exportsLayoutUI.SetData(hometownToCountryArray, ...)` 填充两个方向的候选商品列表。
- `DiplomaticWorldDetailResourceLayoutUI.AddData(TileType, int, int)` 为每个候选商品通过对象池创建/复用 `DiplomaticWorldDetailResourceSlotUI`（继承 `Button`），槽位内含 `_icon`（物品图片）、`_prosperityText` 和 `_disableObject`。
- 槽位列表保存在 `DiplomaticWorldDetailResourceLayoutUI._slotsUI`；每个槽位的当前物品记录在 `DiplomaticWorldDetailResourceSlotUI._tileType`。
- 国家数据 `DiplomaticCountryData.Sheets` 是公开的贸易单列表；`DiplomaticCountryTradeSheetData.Resource`/`IsEnded()`/`IsInfinitePeriod()`（`_goalTradeCount == 0`）均为公开 API，可直接判断某个商品的协议是否正在执行、是否为无限期。

## 方案

在现有 `TradeWorldDetailPatch`（已挂载 `DiplomaticWorldDetailUI.Refresh`）的 Postfix 中追加一个运行时调用，不新增 Harmony 补丁类，保持契约测试对补丁类数量的锁定。

- `TradeResourceStateRuntime.ApplyActiveTradeHighlight(detail)`：
  - 读取 `_country`，为空直接返回；
  - 分别遍历进口、出口两个 layout 的 `_slotsUI`；
  - 只处理当前激活（`IsActivate`）的可见槽位；
  - `GetHighlightKind(country, tileType)` 遍历该国 `Sheets`，找到 `Resource == tileType && !IsEnded()` 的协议，用 `IsInfinitePeriod()` 区分有限期/无限期，返回 `TradeHighlightKind`（`None`/`Limited`/`Infinite`）；
  - `None` 时 `HideHighlight`，否则按类型取对应颜色 `ShowHighlight`。
- 高亮视觉：
  - 背景：在槽位 `transform` 下动态创建一个铺满锚点的纯色 `Image` 子物体（`raycastTarget = false`），放在最底层；
  - 描边：在槽位 `_icon` 上启用 `Outline` 组件（`effectColor` 同背景色，`effectDistance` 为 `(2, -2)`）；
  - 取消高亮时隐藏背景子物体并禁用 `Outline`，完全可逆，不影响原版交互。
- 背景子物体按槽位实例缓存于 `ConditionalWeakTable`，对象池复用槽位时只创建一次，后续仅切换显隐与颜色。

## 配置

- 使用 BepInEx `ConfigEntry<string>`，节 `TradeDetailSlot`：
  - 键 `ActiveTradeBackgroundColor`，默认 `145,135,106`，描述"有限期贸易商品的高亮背景色"；
  - 键 `InfiniteTradeBackgroundColor`，默认 `96,169,23`，描述"无限期贸易商品的高亮背景色"。
- 纯逻辑解析 `TradeResourceStateRules.ParseColorOrDefault(text, fallback)`：接受 `R,G,B` 三段 0–255 整数（允许空白），非法输入回退对应默认色；不依赖 UnityEngine，便于单元测试。
- 高亮类型纯逻辑 `GetHighlightKind(可见, 正在贸易, 无限期)` 返回 `TradeHighlightKind`，供运行时选色。
- 绑定位于 `Plugin.Awake()` 内、补丁安装之前；解析失败回退默认色并记录一次日志。

## 回退与异常处理

- `_slotsUI`、`_icon` 等任一 FieldRef 或组件缺失时，跳过该槽位。
- 整体流程包在 try/catch 中，异常只记录一次，不向游戏主循环传播。
- 高亮不修改国家数据、贸易协议、价格或存档；卸载 Mod 后无任何残留。

## 测试

纯逻辑测试覆盖：

- `ShouldHighlight` 真值表（可见+正在贸易才高亮）；
- `GetHighlightKind` 真值表（区分有限期/无限期/无高亮）；
- 两种默认颜色常量与默认值通道；
- 合法 RGB 文本解析（含空白）；
- 非法文本（空、缺段、多段、非数字、越界、负值）全部回退默认。

静态契约测试固定：

- `TradeWorldDetailPatch.Postfix` 同时调用 `UpdateWorldDetailLabel` 与 `ApplyActiveTradeHighlight`；
- `TradeResourceStateRuntime` 无独立 Harmony 方法，结构调用链（`GetHighlightKind` → `Sheets`/`IsEnded`/`IsInfinitePeriod` → `ShowHighlight`/`HideHighlight` → `GetOrCreateBackground` 的创建与锚点设置）；
- 新增依赖的原版私有字段 `_slotsUI`、`_tileType`、`_icon` 与方法 `IsInfinitePeriod`/`IsEnded`/`get_Resource`/`get_Sheets` 的精确契约；
- 程序集仍不引用任何名字含 `Configuration` 的程序集，Mod 类型名仍不含 `Save`/`Config`。

游戏内验收覆盖：

- 有限期与无限期正在贸易的商品在进出口列表中显示不同颜色背景与描边；
- 未贸易的候选商品保持原版外观；
- 切换国家、刷新列表后高亮随贸易状态实时更新；
- 修改两种配置颜色并重启后生效；配置非法时回退对应默认色。
