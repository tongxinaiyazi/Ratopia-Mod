# 卫生间澡堂加乐趣实施计划

**目标：** 测试先行实现普通卫生间与澡堂完成服务后的可配置乐趣奖励，并生成可直接发布的五文件包。

**技术栈：** C#、net472、BepInEx 5、Harmony 2、xUnit、Mono.Cecil、PowerShell。

## 任务 1：纯奖励策略

- 先创建失败的 xUnit 测试，覆盖普通卫生间、澡堂、中断服务和不支持设施。
- 运行测试确认因生产类型缺失而失败。
- 实现最小 `FacilityKind`、`RewardSettings`、`FunRewardPolicy`，运行测试变绿。

## 任务 2：游戏适配与配置

- 先添加游戏程序集、插件元数据、配置和 Harmony 目标的静态合同测试。
- 运行测试确认因插件与补丁尚不存在而失败。
- 实现 `Plugin`、`FunRewardRuntime` 和 `ServiceCompletionPatch`；配置限制为 0–100，补丁捕获中断状态并只调用原版 `FunUpdate(float)`。
- 运行全部测试并保持无警告。

## 任务 3：文档和打包

- 先添加 README、打包脚本、Release 输出和 ZIP 结构合同测试。
- 运行测试确认缺少文档和打包资产。
- 完成双语 README、无安装构建脚本、Nexus 英文标题/简介/完整介绍。
- 生成并视觉检查原创无文字 PNG 封面。

## 任务 4：最终验证

- 运行 Release 全量测试和显式 Release 构建，所有命令使用 `InstallAfterBuild=false`。
- 运行 Ratopia 包验证和 Nexus 五文件验证。
- 检查最终目录恰好五个文件，ZIP 恰好两个文件，游戏目录没有新增 `RestroomBathFun`。
- 记录未进行游戏内验收，不安装或启动游戏。

