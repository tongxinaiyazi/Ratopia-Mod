# 特殊鼠鼠 v0.1.3 完整迁移审计

## 审计范围

- 游戏：Ratopia 1.0.0600 Mono
- 模组框架：BepInEx 5.4.23.5
- 源数据：12 名特殊鼠鼠、24 个特性、24 个唯一图标
- 代码边界：独立版 `PatchRegistry`、`LegacyPatchAdapters`、特殊鼠运行时状态、皮肤、生成、读档和特性效果
- 存档边界：只读日志和已有备份；本次源码审计不直接写入或重新打包存档

## 已发现并修复的问题

1. **所有特殊候选都会临时覆盖女王外观。** 候选 `Sp_SkinInfo` 在原版无参 `Init()` 后仍绑定女王骨架；v0.1.2 提前调用 `UpdateCombinedSkin()`。v0.1.3 将候选设置为数据模式，只更新 `m_Skin`，正式市民绑定后才渲染到骨架。
2. **商鞅“秦律”繁荣基线为 0。** 独立迁移没有安装原整合模组的 `DB_Mgr.Awake`，也没有迁移其 `LoadProsperityDB()` 调用。v0.1.3 在已存在的 `Character_DB_Setting` 后置补丁中加载深复制基线，并在每次秦律更新前幂等校验。
3. **奥米伽-7的两个特性没有运行时状态。** CSV 和游戏特性数据库有 24 个特性，但 `CustomCharInfo` 只有 22 项，遗漏 `AMJ7_LZDW` 与 `AMJ7_LZJX`。v0.1.3 补齐为严格 24/24。
4. **切换存档时特殊鼠使用状态和缓存会泄漏。** `isUsed`、概率补偿、贸易商业值、电网对象和奥米伽三维可能沿用上一个世界。v0.1.3 在每次世界加载时先完整重置，再按当前市民重建。
5. **四条特性路径存在可证明的空引用边界。** 奥米伽无电网、皮卡丘发电站未连接电网、工作完成时无工人、出口贸易物品为空都可能抛出异常。v0.1.3 在读取字段前返回原版/安全行为。
6. **新招募特殊鼠的自身状态应用过早。** 特性使用者登记前已经执行一次自身状态更新，导致“奈奈的智慧”等状态可能要到读档后才出现。v0.1.3 在两个特性全部登记后再次执行统一状态应用。
7. **奥米伽未激活时仍计算除法表达式。** 默认 `value2` 为 0，会得到无意义的 NaN。v0.1.3 仅在特性有效时计算公式。

## 12 名特殊鼠 / 24 个特性矩阵

| 特殊鼠 | 特性 | 实现与补丁入口 | 触发时机 | 基线或清理规则 | 离线审计 | 游戏内验收 |
|---|---|---|---|---|---|---|
| 奈奈酱 | `NaiNai_Wisdom` 奈奈的智慧 | `UpdateSelfSpecialState`; `generation.citizen-created`; `session.loaded` | 招募完成、读档 | 先登记特性使用者，再清旧 Buff 并按 CSV 值重设 | PASS | 招募后立即确认经验增益，无需二次读档 |
| 奈奈酱 | `NaiNai_Benevolence` 奈奈的关爱 | `UpdateSpecialStateToAllCitizen`; `state.pdi` | 招募、读档、三维变化 | `preValueDic` 每世界清空；全体先清旧引用再更新 | PASS | 改变奈奈三维，检查全体幸福加成同步变化 |
| 伟大嘤联邦 | `LB_Sad` 联邦的哀伤 | `UpdateSelfSpecialState`; `generation.citizen-created`; `session.loaded` | 招募、读档 | 仅持有该特性的本人获得固定负面状态 | PASS | 招募后检查本人幸福状态 |
| 伟大嘤联邦 | `LB_Hope` 联邦的希望 | `T_Citizen_HungerUpdate`; `state.hunger` | 招募、读档、恢复饱食度 | 自身 Buff 重建；产量按当前三维与幸福计算 | PASS | 进食后检查金矿石产出及饥饿效果 |
| 商鞅 | `SY_KCL` 垦草令 | `FoodUI_AllFood_Update`; `state.food-total` | 招募、读档、食物总量变化 | 按当前食物/人口从零计算；`preValueDic` 每世界清空 | PASS | 改变库存食物，检查全体幸福加成与上限 |
| 商鞅 | `SY_QL` 秦律 | `SY_QL_Effect`; `state.pdi`; `data.character-db` | 招募、读档、智力或繁荣变化 | 原始繁荣表深复制；每次赋值为基线加成，不累计 | PASS | 检查法典数量且日志无“库长度 0” |
| 岳飞 | `YF_YJQ` 岳家枪 | `T_Citizen_SwdAtk_Call`; `combat.sword-attack` | 长枪近战命中 | 只读取攻击者自身特性；不保留跨次状态 | PASS | 长枪命中多个敌人，检查溅射和治疗 |
| 岳飞 | `YF_YJJ` 岳家军 | `YF_YJJ_Effect`; `citizen.job` | 招募、读档、职业变化 | 每次先 `RefKill`，仅士兵职业重新添加三项 Buff | PASS | 切换士兵/非士兵职业，检查状态增加和移除 |
| 华佗 | `HT_SYZS` 神医在世 | `T_Citizen_BeAttacked`; `combat.citizen-attacked` | 任意市民受伤 | 仅特性活跃且目标尚未受伤时限制伤害阈值 | PASS | 让普通市民承受高伤害，检查不直接越过受伤阈值 |
| 华佗 | `HT_WQX` 五禽戏 | `HT_WQX_Effect`; `generation.citizen-created`; `session.loaded` | 招募、读档、新市民加入 | 全体逐个清理旧引用再加最大生命、效率和速度 | PASS | 招募前后比较普通市民三项状态 |
| 王亥 | `WH_NC` 牛车 | `DiplomaticData_SetTerrainTotalDistance`; `economy.distance` | 外交地形距离设置 | 每次从传入原距离乘当前系数，最小为 1 | PASS | 打开外交路线，检查城市距离缩短 |
| 王亥 | `WH_SZ` 商祖 | `DiplomaticCountryData_MaxTradeAgreementCount`; `economy.agreement-count` | 读取最大贸易协议数 | 从原版常数 3 加当前智力/繁荣加成，不缓存 | PASS | 检查可签订协议数量 |
| 白圭 | `BG_NYQY` 能以取予 | 进出口价格、`DiplomaticMgr_OnTradeResultEvent_BGNYQY`; `economy.export-price`; `economy.trade-result` | 报价、成功贸易 | 价格每次重算；商业累计字典每世界清空 | PASS | 比较出口价并完成贸易，检查目标城市繁荣增长 |
| 白圭 | `BG_SS` 商圣 | `GetSSValue`; `economy.import-price`; `economy.export-price`; `economy.detail-price` | 季节性报价与详情 UI | 不保留价格基线；按物品类别和当前季节重算 | PASS | 春夏秋冬分别检查食物/日用品价格方向 |
| 李隆基 | `LLJ_KYSS` 开元盛世 | `Helpers_Get_MaximumGuestNum`; `industry.guest-capacity` | 服务建筑查询访客上限 | 原版访客数加当前智力/繁荣加成 | PASS | 检查不同服务建筑最大访客数 |
| 李隆基 | `LLJ_LY` 梨园 | `LLJ_LY_Effect`; `state.pdi` | 招募、读档、智力变化 | 全体先清旧引用，再按当前智力重建效率/娱乐消耗 | PASS | 改变李隆基智力，检查全体两项状态同步 |
| 大正 | `DZ_MGJZ` 蘑菇教主 | `MasonryInfo_WorkUpdate_Prefix`; `industry.work-prefix` | 蘑菇农场工作进度 | 只缩放本次 `d_time`，不修改建筑永久基线 | PASS | 对比蘑菇农场工作速度 |
| 大正 | `DZ_MGZL` 蘑菇之力 | `T_Citizen_ApplyFoodOrLife_ResAbility`; `industry.food-life` | 食用蘑菇、烤蘑菇或牛排 | 延长时取现有剩余时长较大值；按时 Buff | PASS | 三种食物分别检查持续时间和状态 |
| 赵云 | `ZY_QTSPQ` 七探蛇盘枪 | `T_Citizen_SwdAtk_Call`; `combat.sword-attack` | 长枪近战命中 | 只处理碰撞范围内存活敌人；临时击退/减速 | PASS | 长枪攻击多个敌人，检查击退和减速 |
| 赵云 | `ZY_LD` 龙胆 | `ZY_LD_Effect`; `generation.citizen-created`; `session.loaded` | 招募、读档 | 本人先清旧引用，再加速度与闪避 | PASS | 招募后立即检查本人状态 |
| 皮卡丘 | `PKQ_SWFT` 十万伏特 | `PKQ_SWFT_Effect`; `industry.work-postfix` | 鼠力发电站完成工作 | 未接电网时安全跳过；每次独立随机计算 | PASS | 已接/未接电网各完成一次工作，确认无异常 |
| 皮卡丘 | `PKQ_DQCD` 电气场地 | `PKQ_DQCD_Effect`; 由十万伏特溢出触发 | 发电超过电网容量 | 按本次溢出量和 CSV 除数计算时长，不跨世界缓存 | PASS | 制造溢出电力，检查全体速度状态与持续时间 |
| 奥米伽-7 | `AMJ7_LZDW` 量子电网 | `AMJ7_LZDW_Effect` 与 `power.wire-check-*`、`power.four-direction-grid`、`power.delete-connect`、`power.quantum-grid` | 招募、读档、电网连接/耗电 | 每世界将 `SuperElecLine` 置空后重建；缺电网安全返回 | PASS | 多电网合并、断线、远端建筑接入和耗电减免 |
| 奥米伽-7 | `AMJ7_LZJX` 量子机械 | `AMJ7_LZJX_Effect`; `power.robot-created`; `power.robot-fatigue`; `power.connect-building`; `power.add-watt`; `state.pdi`; `combat.citizen-attacked` | 招募、读档、机械生成/耗电、三维变化、奥米伽受伤 | 24/24 状态注册；机器人先清旧 Buff；无电网不充电；未激活不计算除法 | PASS | 检查机械三维、充电、倒地恢复与奥米伽防致死 |

## 共用入口与隔离

- `PatchRegistry` 保持 39 个白名单描述符；其中 38 个经过 `LegacyPatchAdapters` 故障隔离，`session.loaded` 使用独立 `SessionPatches`。
- 没有安装 `DB_Mgr.Awake`、女王、共享仓库、上帝视角或原整合模组其他功能补丁。
- 特殊候选调用 `RegisterCustomSkin(..., false)`；正式特殊市民调用 `RegisterCustomSkin(..., true)`。
- 候选数据模式会执行 `SkinSet` 生成 UI 预览，但条件分支阻止 `UpdateCombinedSkin()`。
- 普通候选继续只调用原版 `MakeSkinInfo` 和 `MakeCharacterList`；普通市民换装在 `CitizenIsSpecialUnit` 判断失败后回退原版。
- 特性图标、名称和描述共用 `state.buff-icon`、`state.icon-address`、`state.display-name`、`state.description`，仅在引用名属于 24 个自定义特性时接管。

## 会话与存档安全

- 世界加载顺序为：`ResetSpecialRatizensSession` → `LoadCitizenDatas` → `UpdateAllUsedSpecialEffects`。
- 重置内容包含特殊市民、候选皮肤、普通自定义皮肤缓存、候选选择、12 个 `isUsed`/概率值、24 个特性使用者、PDI 前值、贸易商业值、超级电网和奥米伽 PDI。
- 当前世界中的特殊鼠通过姓名和两个特性重新识别，不向存档添加新字段。
- 秦律在当前世界没有商鞅时以加成 0 恢复原始繁荣法典数量，避免上一个世界的运行时数值残留。
- 安装流程只替换模组目录；不启动游戏、不保存游戏，也不写入存档包。

## 自动验证结果

- Ratopia `Assembly-CSharp.dll` SHA-256 合同与已逆向版本一致。
- 原版 `DB_Mgr.Awake` 合同确认 `Prosperity_DB_Setting` 先于 `Character_DB_Setting`。
- 12/12 特殊鼠、24/24 CSV 特性、24/24 运行时状态、24/24 唯一图标通过。
- 24 个特性均有唯一特殊鼠拥有者；关键外观 `Skin`、`Face`、`Hair`、`Dress` 均非空。
- 公式使用的发行数据除数均非零。
- 38/38 适配入口与 39 个补丁白名单通过。
- Queen/候选数据模式、正式市民实时模式、普通鼠隔离、事务式皮肤恢复通过合同测试。
- 繁荣基线空列表、数量/等级不匹配、重复加成和零加成恢复通过纯逻辑测试。

## 仍需游戏内验证的项目

离线测试无法渲染 Spine，也不能模拟完整外交、电网、战斗和 UI 生命周期。发布安装后必须至少完成：

1. 连续刷新多个不同特殊候选，确认女王始终保持原外观，候选预览正确。
2. 刷新和招募普通鼠，确认候选、正式模型和工作服保持原版行为。
3. 招募商鞅并二次读档，确认法典数量正确且不再出现繁荣库错误。
4. 招募奈奈酱，确认“奈奈的智慧”无需读档即可生效。
5. 奥米伽-7在无电网、有多个电网和已有机械三种场景下验收。
6. 皮卡丘在发电站未接电网和已接电网时分别完成工作。
7. 在同一游戏进程切换两个存档，确认已招募标记、贸易累计和特殊效果没有串档。
8. 提供新的 `Player.log` 与 `BepInEx/LogOutput.log`，扫描特殊鼠补丁异常、皮肤恢复、繁荣基线错误和 Harmony 错误。

因此，“离线审计 PASS”表示迁移合同、数据、调用边界和可证明异常均已覆盖；最终的模型显示与玩法结果仍以这份运行验收清单为准。
