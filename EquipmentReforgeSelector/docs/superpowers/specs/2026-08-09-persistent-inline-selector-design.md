# 装备重铸自选属性：持久选择与整行操作设计

## 问题与已确认根因

版本 `0.1.1` 已将候选属性嵌入原版米色 `EnhanceEffect` 提示框，但选择会话仍错误地依附于提示框视图。用户给出的稳定复现路径是：先在右侧列表选择属性，再把鼠标划过中间“效果”和“重铸效果”的四个格子；原版会用其他 `SimpleToolTipList` 内容刷新右侧提示框，之后先前选择失效。

源码数据流确认了根因：

1. `SimpleToolTipPatch.Postfix` 对任何非 `EnhanceEffect` 提示调用 `RuntimeController.CloseInlineSelector()`。
2. `CloseInlineSelector()` 调用 `PanelStateCoordinator.Detach(view)`。
3. `Detach()` 调用 `ResetSelection()`，同时清空候选和当前选择。

因此失效不是重铸候选覆盖失败，也不是装备数据变化，而是一次普通的原版提示框刷新错误地结束了选择会话。`Player.log` 同时证明有效选择能够进入 `T_Queen.ItemEnhance` 并在调用后恢复原列表引用，未出现本 Mod 的运行时异常。

## 采用方案

采用“持久选择会话 + 可暂停视图 + 原版列表整行按钮”：

- 选择会话归属于当前装备和重铸阶级，不归属于临时 `SimpleToolTip` 视图。
- 非 `EnhanceEffect` 提示只暂停并释放当前视图绑定，保留候选与精确选择。
- 同一装备、同一阶级的 `EnhanceEffect` 再次出现时，重新绑定原版行并恢复选择标记。
- 换装备、换阶级、关闭重铸详情、切换场景、插件停用，或成功重铸导致旧候选失效时才重置会话。
- 每个候选创建一个覆盖效果列表可用宽度的透明行按钮，原版 `TextMeshProUGUI` 只负责显示，不再直接承担狭窄的点击热区。
- 行首显示数字序号；选中行显示绿色箭头、浅色背景和“已选择”。数字键 `1` 至 `9`（含小键盘）可直接选择对应候选；原有上下导航与回车提交继续可用。

## 状态边界

`PanelStateCoordinator` 分离以下操作：

- `Attach(panel)`：绑定当前视图，不重建选择会话。
- `Detach(panel)`：只解除当前视图，保留候选和选择。
- `ResetSession()`：清空候选和选择，但不负责销毁 Unity 视图。
- `Clear()`：解除视图并重置会话，用于真正结束详情上下文。

`RuntimeController` 分离以下生命周期：

- `SuspendInlineSelector()`：关闭并解绑临时视图，保留会话；供非重铸提示框刷新使用。
- `Clear()`：关闭视图并清除装备、阶级和会话；供详情关闭、上下文变化、场景切换和插件停用使用。
- `OpenInlineSelector(...)`：验证装备类型和阶级后重建视图；`SelectionSession.Update` 在上下文与候选仍一致时保留精确选择。
- `RefreshAfterReforge()`：先重置旧会话，再按重铸后的当前属性重算候选；没有可见视图时保持空会话，避免旧选择用于下一次重铸。

视图因 `m_EffectFrame` 暂时停用而销毁时只暂停；若同时检测到 `BuildMidUI.Obj_Main` 已关闭，则清理整个上下文。

## 整行按钮与输入

`InlineReforgeSelectorView` 为每个原版文本行创建可复用的同级 `RectTransform`：

- 水平方向锚定父容器左右边缘，垂直位置复制原版文本行。
- 添加透明 `Image` 作为可射线检测的目标，并添加 Mod 专属 `InlineReforgeButton`。
- 添加 `LayoutElement.ignoreLayout = true`，避免进入原版布局计算。
- 命中层放在文本后、背景前；原版文本设置 `raycastTarget = false`，确保整行由同一按钮接收点击。
- 普通行背景近乎透明；悬停和选中时使用低透明绿色，保持原版米色视觉。
- 暂停或销毁时立即禁用按钮、移除监听并停用命中层；同一帧重新显示时复用组件，不执行延迟销毁。

数字键只在当前 `EnhanceEffect` 视图有效且候选存在时处理，不修改游戏的全局按键配置。索引越界时不做任何操作。

## 安全与回退

- `T_Queen.ItemEnhance` 的单次候选列表替换、`__state`、Postfix 和 Finalizer 恢复机制保持不变。
- 重铸前仍用最新数据验证完整 `(AbilityId, Value)`；无效选择回退原版随机并正常消耗材料。
- 不新增存档字段，不改变属性值、材料、成就、小游戏或 `Dic_ItemPlusEffect` 格式。
- 运行时 UI 创建或重绑失败时禁用本次视图并明确记录回退原因，不阻止原版界面和重铸。

## 测试与验收

- 纯逻辑回归：视图暂停后重新绑定同一装备/阶级仍保留选择；`Clear`、换装备和换阶级恢复默认首项；成功重铸重置旧选择。
- 输入回归：数字 `1..N` 映射到 `0..N-1`，零、负数和超出候选数量的数字无效。
- 结构合同：非 `EnhanceEffect` 使用暂停而非清空；行命中层横向拉伸、忽略布局、文本不接收射线；不创建独立 Canvas 或侧栏。
- Release 全量测试和构建关闭自动安装；输出不得包含游戏、Unity、BepInEx、Harmony、Mono.Cecil 或测试 DLL。
- 版本提升为 `0.1.2`，生成并校验 `装备重铸自选属性-v0.1.2-BepInEx5.zip`。
- Ratopia 退出后备份 `0.1.1` DLL，再安装 `0.1.2`；启动日志必须证明插件发现、三个补丁安装和首次内嵌绑定，无本 Mod Error/Exception。
- 实机复现原步骤：选择候选，依次划过四个格子，再回到重铸提示；选择标记保持且限定重铸结果正确。点击属性文字、图标和该行空白区域均可选择，数字键选择可见生效。
