# “更强大的工作距离”设计规格

## 目标

创建独立的 Ratopia BepInEx 5 Mono Mod，把所有使用通用鼠民工具站位表的工作范围，从原版横向 1 格、最高 3 格扩展为横向 2 格、最高 4 格。

## 已确认环境

- 游戏目录：`E:\steam\steamapps\common\Ratopia`
- 运行时：Mono，存在 `Ratopia_Data/Managed/Assembly-CSharp.dll`
- BepInEx：5.4.23.5
- Harmony：2.9.0.0
- `Assembly-CSharp.dll` SHA-256：`C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`
- 目标入口：实例方法 `System.Void SystemMgr::Awake()`
- 目标字段：`List_WM_EnableArea` 与 `List_BP_Ld_EnableArea`，类型均为 `List<UnityEngine.Vector2Int>`

## 行为

新范围是完整 25 格矩形：`x=-2..2`、`y=+1..-3`。原版 12 个站位保持原顺序作为前缀，新增 13 个站位追加在后，使原版可达位置继续拥有寻路优先级。

Mod 同时替换常规工作和梯子/特殊蓝图工作站位表，不修改女王专用范围、战斗射程、建筑效果范围或存档。范围固定，不增加配置界面。

## 架构

- 纯 C# 的 `WorkOffset` 与 `WorkAreaRules` 负责生成确定性站位。
- 纯 C# 的 `AtomicListUpdater` 负责两张列表的快照、替换和异常回滚。
- Ratopia 适配器将 `WorkOffset` 转换为 `Vector2Int`，并在 `SystemMgr.Awake()` Harmony Postfix 中调用。
- 插件逐个安装补丁；安装失败时撤销本插件补丁并停用功能。

## 安全与验收

应用失败不得留下只替换一张表的半完成状态。重复 `Awake` 不产生重复坐标。Mod 不写持久数据；移除 DLL 并重新启动后，由原版恢复站位表。

自动验收覆盖纯逻辑、游戏程序集合同、插件合同、Release 输出和发布包结构。安装仅在 Ratopia 退出后进行，并验证构建 DLL 与安装 DLL 的 SHA-256。一轮实机验收覆盖采矿、建造、拆除、维修、保存重载和卸载恢复。

