# 广播站信号覆盖全图设计

## 目标

让全图电视都能使用工作的广播站；电视面向居民的服务距离和广播站电路范围保持原版。

## 运行时设计

- `UI_StorageSelect.TelevisionSelectSet` 完成后补齐全图所有广播站候选，支持手动指定。
- `Building_ElecBandstand.Building_Update2` 完成后，仅对电视自动选择最近的工作广播站。
- 不修改 `BuildInfo.Range`、`Building.m_Range`、范围 UI 或存档对象。

## 安全边界

- 只识别 `BuildingName.BroadcastStation = 309`；`Television = 310` 不匹配。
- 不修改 `Defines.m_MaxCustomBuildingRange`、广播站电路范围或范围设置。
- 不修改居民娱乐检索或电视服务范围。
- 补丁逐类安装；任何安装失败都撤销本 GUID 的全部补丁。
