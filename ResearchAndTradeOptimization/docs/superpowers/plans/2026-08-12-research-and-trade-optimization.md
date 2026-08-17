# 研究与贸易优化 v0.2.0 实施计划

> 执行要求：严格采用测试先行；每项功能先让对应测试失败，再写最小实现并复测。当前目录不是 Git 仓库，因此以构建产物哈希、测试日志和发布包清单代替提交检查点。

**目标：** 将现有“贸易站和研究去除最大队列限制”升级为“研究与贸易优化”，保留已经验证的研究/贸易队列优化，并增加城市完整贸易商品池、执行中普通商品协议的数量与期限调整、无限期协议，以及每个原版最长签约周期结束时按市场价重定价。

**技术路线：** 保持 BepInEx 5 Mono + Harmony + .NET Framework 4.7.2。把不依赖游戏对象的判断和转换放入 `Core`，由单元测试覆盖；Harmony 层只负责接入原版数据/UI。城市商品池替换原版私有抽选函数，但继续调用原版 `RefreshResource`；协议调整复用原版详情与签约面板，并在独立工作副本上编辑；价格刷新挂接原版每日市场更新完成后的包级流程。

**兼容边界：** 不添加配置文件和自定义存档字段；不改变协议类型、资源方向、仓库、ID、累计历史与删除费用；不处理瓦特资源 4001、贡品、电力及其他特殊协议；保留插件 GUID 以延续同一 Mod 身份。

---

## 任务 1：基线、目录迁移与插件身份

**涉及文件：**

- 移动：`UnlimitedResearchAndTradeQueues/` → `ResearchAndTradeOptimization/`
- 移动：解决方案、主项目、测试项目文件与对应目录
- 修改：`src/ResearchAndTradeOptimization/PluginInfo.cs`
- 修改：所有源码与测试命名空间
- 修改：`ResearchAndTradeOptimization.sln`、两个 `.csproj`
- 测试：`tests/ResearchAndTradeOptimization.Tests/ReleaseContractTests.cs`

**步骤：**

1. 记录游戏进程、`Assembly-CSharp.dll` 哈希、旧安装 DLL 哈希与当前存档目录信息。
2. 增加失败测试，要求显示名为“研究与贸易优化”、版本 `0.2.0`、GUID 保持 `cn.ratopia.unlimitedresearchandtradequeues`、程序集名和发布目录为 `ResearchAndTradeOptimization`。
3. 运行 Release 测试并确认身份测试失败。
4. 在确认目标目录不存在后，将工程根目录整体移动；再迁移解决方案、项目目录和项目文件，更新项目引用、程序集名、根命名空间与源码命名空间。
5. 复测身份与原有研究/贸易队列测试，确认迁移没有改变现有功能。

## 任务 2：城市完整贸易商品池规则

**涉及文件：**

- 新增：`src/ResearchAndTradeOptimization/Core/FullTradeResourceRules.cs`
- 新增：`tests/ResearchAndTradeOptimization.Tests/FullTradeResourceRulesTests.cs`
- 新增：`src/ResearchAndTradeOptimization/Patches/FullTradeResourcePatches.cs`
- 新增：`src/ResearchAndTradeOptimization/Runtime/FullTradeResourceRuntime.cs`
- 修改：`src/ResearchAndTradeOptimization/PatchInstaller.cs`
- 修改：`tests/ResearchAndTradeOptimization.Tests/PatchTargetContractTests.cs`

**步骤：**

1. 增加规则测试：保留配置顺序；忽略未启用组和忽略资源；同方向重复资源只保留第一次；保留各资源的繁荣等级；不受 `PickCount` 与跨城市全局池影响。
2. 运行规则测试并确认因类型不存在而失败。
3. 实现纯规则并让测试通过。
4. 增加程序集契约测试，验证 `DiplomaticCountryData.PickUpTradeResources`、`SetTradeResource`、`SetSavableData` 的精确签名和原版 IL 结构。
5. 用 Prefix 替换私有商品抽选：从城市配置的每个启用资源组读取全部资源，应用忽略列表与同方向去重，输出 `KeyValuePair<int, TileType>[]`，不写入原版跨城市随机占用池；异常时记录日志并回退原版。
6. 对 `SetSavableData` 添加 Postfix，读档后立即调用原版 `SetTradeResource`，让旧存档无需等待换季即可重建完整商品池。
7. 保留原版 `RefreshResource`，从而继续应用当前繁荣度与正在执行同资源协议过滤。

## 任务 3：协议可调整资格、数值规则与无限期

**涉及文件：**

- 新增：`src/ResearchAndTradeOptimization/Core/TradeAgreementRules.cs`
- 新增：`tests/ResearchAndTradeOptimization.Tests/TradeAgreementRulesTests.cs`
- 新增：`src/ResearchAndTradeOptimization/Runtime/TradeAgreementEditSession.cs`
- 新增：`src/ResearchAndTradeOptimization/Patches/TradeAgreementEditPatches.cs`
- 新增：`src/ResearchAndTradeOptimization/Localization/ModLocalization.cs`
- 修改：`src/ResearchAndTradeOptimization/PatchInstaller.cs`
- 修改：`tests/ResearchAndTradeOptimization.Tests/PatchTargetContractTests.cs`

**步骤：**

1. 增加失败测试，覆盖：只有普通商品且状态为 Run(1) 或 trouble(10–17) 可调整；资源 4001 与 End/Stop 不可调整；期限 0 表示无限；未改变的超出当前繁荣上限数量允许确认，用户改动后必须落在当前范围；季度边界期间确认时沿用真实协议最新价格。
2. 实现纯规则并让单元测试通过。
3. 增加程序集契约测试，验证详情面板、签约面板、明细槽、确认回调、`ReplaceSheet`、`AgreementTradeSheet` 及所需私有字段。
4. 在贸易详情刷新后，把符合资格协议的原版“续约”槽复用为本地化“调整”，将其事件类型设为 Modify；已结束协议继续显示原版续约。
5. 拦截 `DiplomaticUI.OnTradeDetailEvent(..., Modify)`：克隆真实协议作为工作副本，打开原版签约面板；锁定方向、商品与仓库，只开放数量和期限。
6. 拦截编辑会话中的 `DiplomaticTradeSheetUI.OnSubmitedEvent`，只更新工作副本和面板状态，不调用原版 `ReplaceSheet`，确保取消操作不改动真实协议。
7. 数量继续使用原版当前繁荣等级上限；若繁荣下降导致旧值超限，未改数量仍可确认，改动数量则必须符合当前上限。期限沿用原版有限范围，并允许值 0 显示为无限期。
8. 点击最终确认后再显示本地化二次确认。确认时从真实协议拷贝当下最新价格，重置本期起始时间、当前交易次数和成功次数，保留原状态、ID、方向、资源、仓库、累计交易量和累计金额，并通过原版 `ReplaceSheet` 写回；取消则关闭工作副本且不修改真实数据。
9. UI 隐藏、场景清理或异常时清空编辑会话，避免引用跨场景残留。

## 任务 4：季度市场价自动更新

**涉及文件：**

- 修改：`src/ResearchAndTradeOptimization/Core/TradeAgreementRules.cs`
- 修改：`tests/ResearchAndTradeOptimization.Tests/TradeAgreementRulesTests.cs`
- 新增：`src/ResearchAndTradeOptimization/Patches/QuarterlyTradePricePatches.cs`
- 新增：`src/ResearchAndTradeOptimization/Runtime/QuarterlyTradePriceRuntime.cs`
- 修改：`src/ResearchAndTradeOptimization/PatchInstaller.cs`
- 修改：`tests/ResearchAndTradeOptimization.Tests/PatchTargetContractTests.cs`

**步骤：**

1. 增加失败测试：第 0 天不刷新；仅正数且能被 `Defines.DayOfQuarter` 整除的天刷新；普通商品 Run/trouble 刷新；End/Stop、资源 4001 与特殊类型不刷新。
2. 实现季度边界与协议资格纯规则，复测通过。
3. 在 `DiplomaticCountryPackage.RunProcessDaily` 原版逻辑完成后的 Postfix 中执行刷新，确保所有城市资源市场价已先更新。
4. 遍历每个城市的活动协议；仅将符合资格的普通商品协议 `TradeValue` 改为对应资源的当前 `NowValue`。不改累计历史、数量、期限、状态与本期进度；同一天重复调用写入同值，保持幂等。
5. 记录首次季度刷新及更新协议数量，单个城市/协议异常只写日志，不向游戏主循环传播。

## 任务 5：补丁安装可靠性与回归

**涉及文件：**

- 修改：`src/ResearchAndTradeOptimization/PatchInstaller.cs`
- 修改：`src/ResearchAndTradeOptimization/ResearchAndTradeOptimizationPlugin.cs`
- 修改：现有研究/贸易补丁中的命名空间与日志文本
- 修改：全部测试项目

**步骤：**

1. 将新增补丁加入逐补丁安装清单，并保持目标签名/IL 契约不匹配时取消本 Mod 全部 Harmony 补丁并停用。
2. 保持现有研究前五项加省略号布局、无限研究预约、无限贸易协议和 `当前/∞` 文本行为。
3. 运行全部 Release 测试，检查工程目录中不存在旧命名空间和旧程序集名残留（历史设计文档中的旧名称说明除外）。

## 任务 6：构建、文档、发布与安装

**涉及文件：**

- 修改：`README.md`
- 修改：`docs/Compatibility.md`
- 修改：`scripts/Install.ps1`
- 修改：`scripts/Package.ps1`
- 修改：发布契约测试
- 生成：`dist/研究与贸易优化-v0.2.0-BepInEx5.zip`

**步骤：**

1. 更新中文 README：完整功能、普通协议限定、季度定价时点、无限期语义、安装/卸载/升级、兼容性、存档风险和日志位置。
2. 更新脚本默认项目名、DLL 名、安装目录和 ZIP 名；构建与安装保持分离，默认 `InstallAfterBuild=false`。
3. 在禁止自动安装的条件下执行 Release 全测试与 Release 构建。
4. 运行发布脚本；检查 ZIP 只包含 `BepInEx/plugins/ResearchAndTradeOptimization/ResearchAndTradeOptimization.dll` 和 `README.md`，不包含游戏/Unity/BepInEx/Harmony DLL、PDB、日志或存档。
5. 再次确认 Ratopia 进程已退出；在工作区内建立带时间戳的测试存档备份并校验文件数/总大小。
6. 安装新 DLL 到 `E:\steam\steamapps\common\Ratopia\BepInEx\plugins\ResearchAndTradeOptimization`，校验构建 DLL 与安装 DLL SHA-256 一致。
7. 仅在新 DLL 安装并校验成功后，将旧插件目录移到工作区的可恢复备份位置，防止同一 GUID/补丁重复加载；报告恢复路径。
8. 最终报告测试数、构建结果、程序集/发布包哈希、存档备份、安装路径，以及仍需用户在游戏内完成的实际验收项目。

## 游戏内验收清单

- 研究队列超过 5 项时，先研究的 5 项完整显示，黑框位置正确，额外项目以 `...` 表示。
- 不同季节/重新读档后，每个城市都展示其配置允许且满足繁荣度的完整进出口商品。
- 正在执行与 trouble 状态的普通商品协议显示“调整”；瓦特、特殊、结束和停止协议不错误进入调整。
- 调整数量、有限期限、无限期后，下一次交易采用新值；取消编辑不产生任何变化。
- 繁荣下降时，保持原超限数量可确认；改成新的超限数量会被阻止。
- 跨越原版最长签约周期边界后，活动普通协议价格更新为当时市场价，已有累计历史不变。
- 保存、退出、读档两轮后，商品池、协议状态、数量、期限和进度保持正确。
- 与“特殊鼠鼠”并用，确认本 Mod 未修改其依赖的 `MaxTradeAgreementCount` getter。

