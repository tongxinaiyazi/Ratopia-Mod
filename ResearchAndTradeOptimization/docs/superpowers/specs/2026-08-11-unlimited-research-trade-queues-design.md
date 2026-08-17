# 贸易站和研究去除最大队列限制：设计说明

## 目标

解除原版研究最多预约三项、贸易最多进行三项的限制，同时保留原版队列和存档数据结构。研究和贸易界面必须随实际数量扩展，并在兼容性不确定时安全停用。

## 研究设计

`Tech_RPInfo.UpgradBtn()` 的结构化 Transpiler 只替换两处与 `List<UpgradeNode>.Count` 紧邻的常量 3。新的上限函数先要求 `ResearchQueueRuntime.EnsureVisibleCapacity` 为下一项准备可见节点；成功返回 `int.MaxValue`，失败返回原版 3。

`ResearchingGroup.ResearchingGroupSet()` Prefix 按当前类别的队列长度扩展私有 `Arr_Technode`。扩展对象克隆原版最后节点，保留父节点与原生组件。位置延续最后两个节点的向量；向量不可用时水平增加 100 像素。数组只增长，缩短时由原版刷新逻辑隐藏并复用。

## 贸易设计

`DiplomaticCountryData.IsFullTradeAgreement()` Postfix 将结果设为 `false`，但不修改 `MaxTradeAgreementCount` getter，以避免与“特殊鼠鼠”冲突。

`DiplomaticTradeLayoutUI.UpdateSlot()` Transpiler 只替换唯一的固定 7 次显示循环，循环次数为 `max(7, GetGoodsTradeCount())`，因此继续使用原版 `UIUtility.CreateOrGet`。列表的新协议槽和 `DiplomaticWorldDetailUI.Refresh()` 后的国家详情均显示 `当前/∞`。`IsFullTradeSheet()` 的 7 项清理逻辑保持原样。

## 失败策略

五个补丁逐项安装。任一 Transpiler 的精确 IL 匹配数不正确或补丁安装抛出异常时，插件调用 `UnpatchSelf()` 撤销自身所有补丁并停用。运行时 UI 扩容或文本更新捕获并记录异常，不让异常传播到游戏主循环；研究扩容失败时恢复原版上限 3。

Mod 不创建配置、不写自定义存档字段、不携带运行时依赖 DLL。
