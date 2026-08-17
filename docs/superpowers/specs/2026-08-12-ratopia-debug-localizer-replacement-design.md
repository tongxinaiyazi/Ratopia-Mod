# Ratopia 调试面板启用方式替换设计

## 目标

在本机 Ratopia v1.0.0600、Mono、BepInEx 5.4.23.5 环境中，停用此前通过 `Ratopia_Data/Log/Admin.txt` 开启的原生管理员模式，改为使用第三方 `YunQingLocalizer 0.2.0` 打开并汉化 Cheat 调试面板。

本次不修改游戏程序集、BepInEx 配置、其他 Mod 或任何存档。

## 已确认环境

- 游戏目录：`E:\steam\steamapps\common\Ratopia`
- 游戏类型：Mono，`Ratopia_Data/Managed/Assembly-CSharp.dll` 存在
- BepInEx：5.4.23.5
- Harmony：2.9.0.0
- 游戏程序集 SHA-256：`C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`
- 待安装压缩包：`D:\QQ\plugins-bep5.zip`
- 压缩包 SHA-256：`18900DE0D3FDC3B4155D97665050EDC973B9FB10903AD9B96F0353A12C4DA9DA`
- 已安装插件中没有 `RatopiaMod.YunQing.Localizer` GUID，也没有重复 GUID。

## 旧功能来源

旧调试功能只由以下空文件激活：

`E:\steam\steamapps\common\Ratopia\Ratopia_Data\Log\Admin.txt`

该文件的 SHA-256 为零字节文件的标准哈希 `E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855`。游戏检测到它后会设置 `Defines.IsPublicVersion=false` 和 `Defines.Cheat=true`，从而启用 F8、F3、F4 等原生调试入口。

替换时把该文件移到工作区内带时间戳的备份目录。游戏路径中不再存在 `Admin.txt`，因此旧激活方式失效；备份仍允许人工回滚。

## 新 Mod 内容与行为

压缩包只包含：

- `plugins/YunQingLocalizer/RatopiaMod.YunQing.Localizer.dll`
  - SHA-256：`3EA66A55C3220374E061751F953DC9B9B13E32A284657C697FE5CC183A4E9B10`
  - BepInEx GUID：`RatopiaMod.YunQing.Localizer`
  - 插件版本：0.2.0
- `plugins/YunQingLocalizer/CheatPanelChinese.json`
  - SHA-256：`610D5B2946A32AB83EA9B56B2BD07CAAB466E76110F56033EDEA9F0AAEC10730`
  - 共 180 个翻译项，171 项有中文内容，9 项为空

安装目标为：

`E:\steam\steamapps\common\Ratopia\BepInEx\plugins\YunQingLocalizer`

插件直接监听 F3，通过当前游戏实际存在的 `DebugMgr.Instance._CheatMgr` 和 `CheatMgr.SetActive(bool)` 切换面板，不依赖 `Admin.txt`。它为 `CheatMgr.Awake()` 与 `CheatMgr.SetActive(bool)` 安装 Postfix，将面板中的 Unity UI Text 和 TMP Text 按 JSON 映射替换为中文。

移除 `Admin.txt` 后，原先由管理员模式解锁的 F8/F4 等入口不再作为本方案的一部分；新入口是 F3。插件会在打开面板时重新保存自身的翻译 JSON，以记录新发现但尚未翻译的韩文条目；不会主动写入游戏存档。玩家点击作弊按钮仍可能改变当前游戏状态，并可能在之后手动保存时进入存档。

## 安装与回滚

仅在 Ratopia 进程不存在时执行：

1. 再次记录 `Admin.txt`、压缩包和受保护游戏程序集的状态与哈希。
2. 在工作区创建备份目录，名称格式为 `backups/ratopia-debug-replacement-yyyyMMdd-HHmmss`，其中末尾部分取实际执行时间。
3. 将 `Admin.txt` 移入备份目录，使旧功能在游戏目录中失效。
4. 只把 ZIP 中的 DLL 和 JSON 安装到 `BepInEx/plugins/YunQingLocalizer`。
5. 比较安装文件与 ZIP 条目的 SHA-256。
6. 不更改、移动或覆盖其他插件和存档。

回滚时关闭游戏，删除本次新建的 `YunQingLocalizer` 目录，并把备份的 `Admin.txt` 放回原路径。由于安装前目标插件目录不存在，回滚不需要恢复旧版 DLL。

## 验收

静态验收：

- Ratopia 进程不存在时完成安装。
- 游戏目录中不再存在 `Ratopia_Data/Log/Admin.txt`。
- 新 DLL 和 JSON 位于准确目标目录，且 SHA-256 与压缩包条目一致。
- `Assembly-CSharp.dll` 和其他已安装插件保持不变。

运行时验收：

1. 启动游戏并进入一个存档。
2. 在 BepInEx 日志中确认出现 `Loading [YunQingLocalizer 0.2.0]`；该日志只证明插件被发现。
3. 按 F3 确认 Cheat 面板能够打开和关闭。
4. 确认面板中已有映射的项目显示中文，并接受 9 个空翻译项可能仍不完整。
5. 检查新的 `BepInEx/LogOutput.log` 与 `Player.log`，确认没有由插件加载或 Harmony 补丁造成的新增异常。
6. 验收期间不要点击会改变资源、角色或世界状态的作弊按钮，避免无意修改后续存档。
