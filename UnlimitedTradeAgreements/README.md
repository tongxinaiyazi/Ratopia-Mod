# 贸易站去除最大队列限制

适用于 Ratopia（鼠托邦）Mono 版与 BepInEx 5 的独立模组。

## 功能

本模组只解除贸易协议数量限制：

- 不再限制每个国家最多同时存在 3 个贸易协议。
- 贸易协议超过原版显示容量时，继续使用原版槽位对象池显示全部协议。
- 贸易列表和国家详情中的协议数量显示为 `当前数量/∞`。

本模组不包含研究队列、全部贸易商品、商品预览调整、修改协议数量或期限、无限期限、价格定期刷新等“研究与贸易优化”的后续功能。

## 环境与兼容性

- `.NET Framework 4.7.2`
- BepInEx `5.4.23.5`
- Harmony `2.9.0`
- 适配的 `Assembly-CSharp.dll` SHA-256：`C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`

不能与“研究与贸易优化”同时启用，因为两个模组会修改相同的贸易队列方法。插件已声明与其 GUID `cn.ratopia.unlimitedresearchandtradequeues` 不兼容。

## 安装

1. 确认游戏已安装 BepInEx 5，并完全退出游戏。
2. 将 ZIP 内容直接解压到 Ratopia 游戏根目录。
3. DLL 最终路径应为：

```text
Ratopia/BepInEx/plugins/UnlimitedTradeAgreements/UnlimitedTradeAgreements.dll
```

如果已经安装“研究与贸易优化”，请先移除或停用其中一个模组。

## 存档风险

本模组不增加自定义存档字段，也不改变原版贸易协议的数据格式。启用后仍建议先备份重要存档；若在原版上限之外保留了很多正在执行的协议，移除模组后原版界面和新增协议操作会重新受原版限制，但存档格式本身不依赖本模组。

## 卸载

退出游戏后删除：

```text
BepInEx/plugins/UnlimitedTradeAgreements/UnlimitedTradeAgreements.dll
```

也可以删除整个 `UnlimitedTradeAgreements` 插件文件夹。不要在游戏运行时覆盖或删除 DLL。

## 日志

- BepInEx 日志：`BepInEx\LogOutput.log`
- Unity 日志：`C:\Users\<用户名>\AppData\LocalLow\CasselGames\Ratopia\Player.log`

正常加载时会记录三个补丁的安装信息，以及首次显示超过原版容量的贸易槽位数量。
