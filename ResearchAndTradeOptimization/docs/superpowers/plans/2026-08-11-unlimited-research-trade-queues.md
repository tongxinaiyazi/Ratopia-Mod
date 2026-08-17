# 贸易站和研究去除最大队列限制：实施计划

1. 建立独立的 BepInEx 5 Mono、.NET Framework 4.7.2 解决方案，所有游戏运行时引用设为 `Private=false`，默认不安装。
2. 先写纯规则测试：研究扩容门禁、贸易显示数量、无限文本、节点间距与复用所需的位置计算。
3. 固化当前游戏程序集 SHA-256、目标签名、私有字段以及精确 IL 匹配数。
4. 先写 Transpiler 与插件结构失败测试，再实现研究/贸易补丁、原版 UI 扩容和失败回退。
5. 先写发布契约测试，再完成中文 README、游戏内验收清单、构建/安装分离脚本。
6. 在 `InstallAfterBuild=false` 下运行完整 Release 测试和构建，生成只含插件 DLL 与 README 的 ZIP，并扫描禁止 DLL、PDB、日志和存档。
7. 检测 Ratopia 进程；游戏退出前不得安装。退出后安装单个 DLL并校验源/目标 SHA-256，游戏内验收由 `docs\TESTING.md` 执行。
