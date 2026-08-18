# “更强大的工作距离”实施计划

**目标：** 在独立目录交付、打包并安装一个 BepInEx 5 Mod，将通用鼠民工具范围扩展为 25 格矩形。

**架构：** 用纯逻辑生成并原子替换工作站位，Harmony 仅负责在 `SystemMgr.Awake()` 后调用适配器。所有游戏程序集引用设为不复制，构建、打包、安装和运行时验证分开设门禁。

**技术栈：** C#、.NET Framework 4.7.2、BepInEx 5.4.23.5、Harmony 2.9.0、xUnit、Mono.Cecil、PowerShell。

## 全局约束

- 插件名称 `更强大的工作距离`，GUID `cn.ratopia.strongerworkdistance`，版本 `0.1.0`。
- 范围固定为 `x=-2..2`、`y=+1..-3`，原版 12 个坐标保持顺序前缀。
- 同时修改 `List_WM_EnableArea` 与 `List_BP_Ld_EnableArea`，不改 `List_Queen_EnableArea`。
- 不写存档，不打包游戏、Unity、BepInEx、Harmony 或第三方 DLL。
- 严格执行测试先行；构建和测试禁止自动安装。

## 任务

1. 搭建独立 `net472` 解决方案、测试工程和 Git feature 分支。
2. 先写失败测试，再实现 `WorkOffset` 与 `WorkAreaRules.CreateExpandedOffsets()`。
3. 先写失败测试，再实现 `AtomicListUpdater.ReplaceBoth()` 的幂等替换和异常回滚。
4. 先写合同失败测试，再实现插件入口、`SystemMgr.Awake()` Postfix 和 Ratopia 运行时适配器。
5. 编写 README、打包脚本和发布输出合同。
6. 运行完整 Release 测试、构建和 Ratopia 包验证脚本。
7. Ratopia 退出后备份并安装 DLL，核对 SHA-256。
8. 验证发现、补丁安装、首次调用、25 项初始化、工作行为、保存重载和卸载恢复。

