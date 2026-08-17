# 电力刷新兼容修复设计

## 背景与根因

`PortOverlayRegistry.Reconcile` 当前在每次成功验证端口坐标后都会无条件调用
`RefreshWire`、`ActRefreshByDynamo`、`RefreshElecUseBuilding` 和
`RefreshElecMakeBuilding`。会话初始化会验证所有已登记端口，之后每两秒验证所有
多所有者端口，因此即使端口代表和线路完全正确，也会反复重跑全城建筑供电判定。

“特殊鼠鼠”的“量子电网”会把远离实体电线的建筑加入统一线路。原版
`RefreshElecUseBuilding` 调用 `WireCheck(false)`，而量子电网在该路径中只登记连接，
不会把检查结果改为成功或清除 `NoElec`/`NoBattery`。两个 Mod 叠加后，建筑可能实际
位于超级电网中，却被周期性写回缺电警报。

## 设计

端口协调改为幂等、事件驱动：

- 读取每个已登记所有者的现有 `ElecLine_Info`，只为确实缺少线路的端口调用
  `NewConnectCheck`。
- 只在同格所有者位于不同线路时调用 `MergeTwoElecLine`。
- 比较 `Dic_PortTileMap` 中的现有代表与期望代表；ID、端口类型和坐标均一致时不写回。
- 只有线路被恢复、线路被合并或端口代表发生变化时，才刷新该坐标的电线显示。
- 只有线路拓扑确实改变时调用 `ActRefreshByDynamo`。
- 从协调器中彻底移除 `RefreshElecUseBuilding` 和 `RefreshElecMakeBuilding`；协调器不再
  重置或重评全城建筑的供电/警报状态。
- 保留会话初始化的全端口验证和每两秒的重叠端口验证，但无变化验证必须为只读操作。
- 保留失败坐标重试、端口代表优先级、拆除后的幸存端口提升和异常隔离。

该方案不探测、不引用“特殊鼠鼠”程序集，对原版以及其他修改电网规则的 Mod 同样安全。

## 测试

- 增加程序集合同回归：`PortOverlayRegistry.Reconcile` 不得调用两个全局供电刷新方法。
- 增加程序集合同回归：局部 `RefreshWire` 必须受状态变化条件控制，不能在每次成功验证时
  无条件执行。
- 保留并运行同格端口排序、登记、去重、删除和会话重置测试。
- 对目标 `Assembly-CSharp.dll` 运行真实签名合同测试。
- 运行干净 Release 全套测试；游戏退出后才允许安装并核对 DLL SHA-256。

## 非目标

- 不修改“特殊鼠鼠”源码或量子电网特性。
- 不新增配置、外部 API 或自定义存档字段。
- 不改变普通电线的放置、背景节点、拆除选择和 F 键交互规则。
