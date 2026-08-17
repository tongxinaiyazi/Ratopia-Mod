# 超级弓箭

`超级弓箭` 是 Ratopia（鼠托邦）的 BepInEx 5 Mono Mod，专门加强女王使用的原版 `WoodBow`。

## 功能

- 将 `WoodBow` 的基础 ATK 从 `2` 提升至 `3`，与 `NobleSword` 的基础 ATK 相同；其他武器不变。
- 仅在打开或重铸 `WoodBow` 时，重铸 1 候选池临时增加“范围攻击”：箭矢实际命中目标时，以主目标为圆心，对 1.5 格内其他存活、非友军且可被原版女王弓箭伤害的目标造成直击伤害 50% 的额外伤害。范围伤害不会递归产生下一轮范围伤害。
- 仅在打开或重铸 `WoodBow` 时，重铸 2 候选池临时增加“流血”：直击和本次范围攻击实际造成伤害的目标都会流血。除 `EnemyCategory.Boss` 外全部按普通目标处理，在第 1、2、3 秒各按最大生命值 3% 计算；Boss 每跳按 1% 计算。百分比结果四舍五入到最近整数（0.5 向上，正伤害至少为 1），真实扣血与伤害飘字始终相同。游戏内词条提示仅显示“流血”。
- 支持原版女王弓箭的四类受伤入口：敌对 `GameUnit`、可受伤 `AnimalBody`（包括岩浆水母）、可受伤 `MapObj`（包括巢穴），以及原版允许弓箭攻击的 `EnemyNexus` 建筑；不会把友方 `GameUnit` 或普通友方建筑加入范围目标。
- 重复命中只把流血持续时间刷新为当前游戏时间加 3 秒，不叠层，也不会推迟已经排定的下一跳。暂停游戏时不会结算。
- 无配置文件、无热键，数值固定。

## 版本与兼容性

- Mod 名称：`超级弓箭`
- 插件 GUID：`cn.ratopia.superbow`
- Mod 版本：`0.1.2`
- 目标运行时：`.NET Framework 4.7.2`、BepInEx `5.4.23.5`、Harmony `2.9.0.0`
- 已检查 Unity 版本：`2021.3.21f1`
- 已检查 `Assembly-CSharp.dll` SHA-256：`C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`

游戏更新后，如果程序集哈希变化，请先停用本 Mod 并等待兼容性确认。发布包不包含游戏、Unity、BepInEx 或 Harmony 的 DLL。

## 安装

1. 完全退出 Ratopia。
2. 备份 `Ratopia_Data\SaveFile` 中的重要存档。
3. 将发布包解压到 Ratopia 游戏根目录，保持包内目录结构不变。
4. 确认 DLL 位于 `BepInEx\plugins\SuperBow\SuperBow.dll`。
5. 启动游戏，在 `BepInEx\LogOutput.log` 中确认出现“发现插件：超级弓箭 0.1.2”。

不要同时放置同 GUID 的重复 DLL，也不要在游戏运行时覆盖插件文件。

## 重铸与存档说明

“范围攻击”保存为原版 `RangeAtk=1`；“流血”保存为原版 `BloodDrain=3` 组合，因此不会向存档写入未知枚举。只有精确的 `BloodDrain=3` 会被本 Mod 显示和处理为流血，其他原版吸血装备不受影响。

临时卸载本 Mod 后，已有流血词条会退化为没有弓箭流血行为的原版 `BloodDrain=3` 显示；存档仍使用原版结构。重新安装后会恢复“流血”提示和行为。Mod 会在场景切换、对象销毁和关闭时清理运行中的流血状态。

## 卸载

1. 完全退出游戏并再次备份存档。
2. 删除 `BepInEx\plugins\SuperBow\SuperBow.dll`，或移走整个 `SuperBow` 插件目录。
3. 启动测试存档确认可以正常读取，再继续使用正式存档。

如果与其他会修改 `WoodBow`、弓类重铸候选、`Bow_Arrow.OnTriggerEnter2D` 或能力提示的 Mod 同时使用，后加载的补丁可能产生冲突。排查时请一次只启用一个同类 Mod，并附上 `BepInEx\LogOutput.log`。
