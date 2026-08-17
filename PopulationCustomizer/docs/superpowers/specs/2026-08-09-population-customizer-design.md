# 人口自定义设计

## 目标

为 Ratopia BepInEx 5 Mono 环境提供“人口自定义”v0.1.0。玩家从原版人口栏旁的按钮打开面板，并为当前存档分别设置鼠民和机器鼠上限。

## 运行时设计

- `ProsperityUI.GetMaxCitizenCount()` 与 `SystemMgr.GetGBotMaxCount()` 使用 Harmony Postfix。未启用自定义时保留原版返回值；启用时返回 0 到 999 的配置值。
- `TileMgr.Awake()` 建立存档会话并从 `PlayDataMgr.Instance.m_GameData.ModsData` 读取设置。
- `CitizenUI.Awake()` 在原版人口栏旁创建一次“上限”按钮以及居中模态面板。
- 面板分别显示当前数量、原版上限和有效上限，并提供独立启用开关、数字输入框、“应用到当前存档”“恢复原版”和“关闭”。
- 当前数量超过新上限时不删除任何单位，只由原版判定阻止继续新增。

## 持久化与错误处理

- 键：`cn.ratopia.populationcustomizer.settings`。
- 值：`v1|鼠民启用|鼠民上限|机器鼠启用|机器鼠上限`，启用字段只能是 `0` 或 `1`，数值只能是 `0..999`。
- 点击应用后立即改变运行时设置，并更新当前 `D_Data.ModsData`；磁盘写入由玩家下一次正常保存完成。
- 首次使用、键缺失、数据损坏、存档尚未就绪或初始化异常时使用原版上限。损坏数据只记录一次警告，不自动覆盖。
- 恢复原版只移除本插件自己的键。

## 输入与生命周期

- 打开面板时保存 `InputMgr.NowActionMapKey` 并切换到 `INPUT_ACTIONMAP_UI`。
- 关闭、切场景或插件销毁时，仅在本面板仍持有 UI Action Map 时恢复先前 Map；先前值为空时恢复默认 Map。
- 面板根对象挂在当前人口 UI 场景对象下，随场景销毁；重复 Awake 不生成重复按钮。

## 测试与发布

- 纯逻辑测试覆盖边界、原版回退、输入解析和版本化编解码。
- 静态合同测试固定目标程序集哈希、目标方法、关键字段、插件元数据和禁止依赖。
- 发布包只包含 `BepInEx/plugins/PopulationCustomizer/PopulationCustomizer.dll` 与 `README.md`。
- 安装前备份存档和旧 DLL；实机覆盖两轮保存/重载、不同存档隔离、恢复原版及移除 Mod 后原版读档。

