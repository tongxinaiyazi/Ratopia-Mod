# “人口自定义”入口迁移设计

## 背景与根因

当前 v0.1.0 已成功加载全部 Harmony 补丁，运行日志也记录“人口栏旁的‘上限’按钮已创建”，但按钮作为原人口文本父容器的子对象被放到 HUD 容器边界之外，最终被裁剪或被相邻控件覆盖。人口上限补丁、每存档设置和居中设置面板本身不受影响。

## 用户体验

移除人口 HUD 旁的旧入口。玩家点击原版人口数量进入“鼠民名单”界面后，在右侧名单顶部绿色标题栏内、放大镜按钮左侧看到一个与原版按钮风格一致的“上限”按钮。

点击“上限”打开现有居中设置面板。面板关闭后继续停留在鼠民名单界面；`Esc`、场景切换和插件卸载仍安全恢复进入面板前的 Action Map。鼠民与机器鼠上限、每存档保存格式和恢复原版行为保持不变。

## 接入点与组件

- 使用 Harmony Postfix 接入 `CasselGames.UI.StatisticsCitizenListUI.Initialize()`。
- `PopulationUiController` 分别接收 `CitizenUI` 和 `StatisticsCitizenListUI`，仅在两者都可用时创建入口与设置面板；重复初始化必须幂等。
- 从 `StatisticsCitizenListUI` 的私有 `_filterBtn` 取得原版按钮模板。克隆到同一父级，清除原按钮监听，移除或隐藏原图标内容，添加“上限”文字与本 Mod 点击监听。
- 将克隆按钮插入到放大镜按钮左侧。若模板、父级、字体或运行时对象未就绪，则不创建半成品按钮，记录一次警告并保持人口上限补丁继续工作。
- `PopulationSettingsPanel` 不再在 `CitizenUI.Txt_Num` 的父级创建入口；设置遮罩继续使用独立根 Canvas。
- 场景卸载或控制器重置时销毁本 Mod 的入口和遮罩，不删除或修改任何原版按钮。

## 兼容性

已安装的 `RatopiaCitizenListUpdateMod` 只补丁 `CitizenCaveUI`，本次目标为 `StatisticsCitizenListUI.Initialize()`，两者没有相同 Harmony 目标。入口只复制原版按钮视觉对象并清除克隆对象的监听，不修改原按钮事件。

游戏程序集合同继续固定 `Assembly-CSharp.dll` SHA-256：`C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`。

## 测试与验收

严格采用 TDD：

1. 先增加合同测试，要求 `StatisticsCitizenListUI.Initialize()`、`_filterBtn` 和新 Harmony 补丁存在，并要求旧的 HUD 入口挂载方式消失。
2. 运行目标测试确认因新补丁和挂载接口缺失而失败。
3. 最小实现新入口，目标测试转绿后运行全部测试。
4. Release 构建和 ZIP 检查不得复制游戏、Unity、BepInEx、Harmony 或测试依赖。
5. Ratopia 完全退出后备份已安装 DLL，再安装新构建并比较 SHA-256。
6. 实机进入鼠民名单，确认按钮位于放大镜左侧、只出现一次、点击可打开面板、输入焦点正常、关闭返回名单，并复验设置立即生效与保存重载。

## 安全边界

- 将修复版插件版本升级为 `0.1.1`，发布包命名为 `人口自定义-v0.1.1-BepInEx5.zip`，以便从日志和文件区分不可见入口的 `0.1.0`。
- 游戏运行时只允许读取日志和构建，不覆盖已加载 DLL。
- 不修改存档格式或现有 `ModsData` 键。
- 不使用子智能体；规划、实现、审查、测试、打包、安装和运行验证均由主智能体顺序完成。
- 工作区不是 Git 仓库，因此保留设计与实施文档，但不创建提交。
