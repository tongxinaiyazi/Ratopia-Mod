using System.Collections.Generic;
using BepInEx.Logging;

namespace EquipmentReforgeDodge.Core
{
    /// <summary>
    /// 把闪避属性注入饰品（且仅饰品）的一阶、二阶重铸候选列表。
    /// 只修改当前会话内存中的 ItemEnhanceInfo，不写回任何资产文件。
    /// </summary>
    public static class AccessoryEnhanceInjector
    {
        public static void Apply(DB_Mgr dbMgr, ManualLogSource logger)
        {
            if (dbMgr == null)
            {
                logger?.LogWarning("DB_Mgr 不存在，跳过闪避重铸注入。");
                return;
            }

            var enhanceDb = dbMgr.m_ItemEnhanceDB;
            var enhanceList = enhanceDb != null ? enhanceDb._list : null;
            if (enhanceList == null)
            {
                logger?.LogWarning("ItemEnhanceDB 候选列表缺失，跳过闪避重铸注入。");
                return;
            }

            var accessoryTypes = CollectAccessoryTypes(dbMgr);
            if (accessoryTypes.Count == 0)
            {
                logger?.LogWarning("没有找到任何饰品物品类型，跳过闪避重铸注入。");
                return;
            }

            var injectedCount = 0;
            var skippedCount = 0;
            foreach (var enhanceInfo in enhanceList)
            {
                if (enhanceInfo == null || !accessoryTypes.Contains(enhanceInfo.Type))
                {
                    continue;
                }

                var tier1Injected = DodgeCandidateRules.TryAppendDodge(
                    enhanceInfo.List_Ability1,
                    enhanceInfo.List_AbilityValue1,
                    DodgeReforgeConfig.Tier1DodgePercent);
                var tier2Injected = DodgeCandidateRules.TryAppendDodge(
                    enhanceInfo.List_Ability2,
                    enhanceInfo.List_AbilityValue2,
                    DodgeReforgeConfig.Tier2DodgePercent);

                if (tier1Injected && tier2Injected)
                {
                    injectedCount++;
                }
                else if (!tier1Injected || !tier2Injected)
                {
                    skippedCount++;
                    logger?.LogWarning(
                        $"饰品类型 {enhanceInfo.Type} 的重铸候选数据不完整（一阶注入：{tier1Injected}，二阶注入：{tier2Injected}），该类型保持原样。");
                }
            }

            logger?.LogInfo(
                $"闪避重铸注入完成：覆盖 {injectedCount} 个饰品类型（一阶 +{DodgeReforgeConfig.Tier1DodgePercent:0}%，二阶 +{DodgeReforgeConfig.Tier2DodgePercent:0}%），跳过 {skippedCount} 个。");
        }

        /// <summary>
        /// 收集饰品的 m_Type 集合。以 List_AccessoryDB 为准，
        /// 并用 Category == Accessory 双重校验，确保武器与装甲永远不会被注入。
        /// </summary>
        public static HashSet<int> CollectAccessoryTypes(DB_Mgr dbMgr)
        {
            var accessoryTypes = new HashSet<int>();
            var accessoryList = dbMgr != null ? dbMgr.List_AccessoryDB : null;
            if (accessoryList == null)
            {
                return accessoryTypes;
            }

            foreach (var item in accessoryList)
            {
                if (item != null && item.Category == ItemCategory.Accessory)
                {
                    accessoryTypes.Add(item.m_Type);
                }
            }

            return accessoryTypes;
        }
    }
}
