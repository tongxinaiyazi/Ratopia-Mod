# “贸易站去除最大队列限制”独立模组设计

## 目标

在不修改 `ResearchAndTradeOptimization` 的前提下，新建一个完全独立的 BepInEx 5 Mono 模组，只解除贸易协议数量限制，并保证超过原版数量后仍可见、可操作。

## 插件身份

- 名称：`贸易站去除最大队列限制`
- GUID：`cn.ratopia.unlimitedtradeagreements`
- 版本：`0.1.0`
- 目标：Ratopia Mono、BepInEx `5.4.23.5`、Harmony `2.9.0`、`.NET Framework 4.7.2`
- 适配的 `Assembly-CSharp.dll` SHA-256：`C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`

## 功能边界

仅保留以下行为：

1. 把 `DiplomaticCountryData.IsFullTradeAgreement()` 的结果改为 `false`。
2. 把 `DiplomaticTradeLayoutUI.UpdateSlot()` 原版固定 7 槽显示循环扩展为 `max(7, GetGoodsTradeCount())`。
3. 在贸易列表和国家详情中显示 `当前数量/∞`。

明确不包含研究队列、全部贸易商品、商品预览折叠、协议数量与期限修改、无限期限、季度价格刷新或任何其他后续功能。

## 兼容和安全

- 使用独立目录、程序集、命名空间和 GUID。
- 声明与 `研究与贸易优化` 当前 GUID `cn.ratopia.unlimitedresearchandtradequeues` 不兼容，避免两个模组重复修改同一方法。
- 不修改 `MaxTradeAgreementCount`，减少与其他修改贸易上限的模组冲突。
- 不增加存档字段，不更改原版贸易协议数据结构；移除模组后原版仍读取原有存档。
- 补丁逐项安装；任一补丁安装失败时撤销本模组全部补丁并停用。

## 工程和发布

- 独立工程目录：`UnlimitedTradeAgreements`。
- 构建与安装分离，默认不自动安装。
- 发布包：`贸易站去除最大队列限制-v0.1.0-BepInEx5.zip`。
- ZIP 只包含：

```text
BepInEx/
└── plugins/
    └── UnlimitedTradeAgreements/
        └── UnlimitedTradeAgreements.dll
README.md
```

- 不安装到游戏目录；本次请求只生成独立发布包。

## 验证

- 纯逻辑测试覆盖槽位数量和 `当前/∞` 文本。
- Transpiler 测试覆盖精确 1 处 IL 匹配以及结构异常拒绝。
- 合同测试覆盖插件身份、互斥声明、三个补丁目标、真实游戏方法和私有字段。
- Release 测试、构建及发布包禁止 DLL 扫描全部通过后才交付。
