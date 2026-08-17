using System.Collections.Generic;

namespace ExecutionPlatform.Runtime
{
    internal static class ExecutionCatalog
    {
        internal const int RuntimeBuildingValue = 10001;
        internal static readonly BuildingName RuntimeBuildingName = (BuildingName)RuntimeBuildingValue;

        internal static bool IsExecution(BuildInfo info)
        {
            return info != null && info.Name.Equals(RuntimeBuildingName);
        }

        internal static bool IsExecution(Building building)
        {
            return building != null && IsExecution(building.m_Info);
        }

        internal static bool TryRegister(DB_Mgr database, out string failure)
        {
            failure = null;
            if (database?.Dic_BuildDB == null)
            {
                failure = "建筑数据库尚未初始化。";
                return false;
            }

            if (ContainsBuildingValue(database))
            {
                failure = $"建筑值 {RuntimeBuildingValue} 已被占用。";
                return false;
            }

            if (ContainsDatabaseIndex(database))
            {
                failure = $"数据库索引 {RuntimeBuildingValue} 已被占用。";
                return false;
            }

            if (!database.Dic_BuildDB.TryGetValue(BuildingName.Prison, out var prison) || prison == null)
            {
                failure = "找不到原版监狱建筑模板。";
                return false;
            }

            var execution = Clone(prison);
            execution.Key = "ExecutionPlatform";
            execution.Name = RuntimeBuildingName;
            execution.BuildingNameToString = "Prison";
            execution.Master = 1;
            execution.ResearchType = 0;
            execution.Index = RuntimeBuildingValue;
            execution.Enable = 1;
            execution.T_Name = "处刑台";
            execution.T_Script = "指定一名鼠民后立即前来工作，工作1秒后生命值归零。";
            execution.Level = 0;
            execution.OriginLevel = 0;
            execution.Ability = BuildAbility.None;
            execution.Desire_Name = DesireName.None;

            database.Dic_BuildDB.Add(RuntimeBuildingName, execution);
            return true;
        }

        internal static bool IsOwnedRegistration(DB_Mgr database)
        {
            if (database?.Dic_BuildDB == null ||
                !database.Dic_BuildDB.TryGetValue(RuntimeBuildingName, out var owned) ||
                owned == null ||
                owned.Key != "ExecutionPlatform" ||
                !owned.Name.Equals(RuntimeBuildingName) ||
                owned.Index != RuntimeBuildingValue)
            {
                return false;
            }

            if (database.Dic_BlockBuildDB?.ContainsKey(RuntimeBuildingName) == true)
            {
                return false;
            }

            foreach (var info in EnumerateBuildInfos(database))
            {
                if (!ReferenceEquals(info, owned) &&
                    (info.Name.Equals(RuntimeBuildingName) || info.Index == RuntimeBuildingValue))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ContainsBuildingValue(DB_Mgr database)
        {
            if (database.Dic_BuildDB.ContainsKey(RuntimeBuildingName) ||
                database.Dic_BlockBuildDB?.ContainsKey(RuntimeBuildingName) == true)
            {
                return true;
            }

            foreach (var info in EnumerateBuildInfos(database))
            {
                if (info.Name.Equals(RuntimeBuildingName))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsDatabaseIndex(DB_Mgr database)
        {
            foreach (var info in EnumerateBuildInfos(database))
            {
                if (info.Index == RuntimeBuildingValue)
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<BuildInfo> EnumerateBuildInfos(DB_Mgr database)
        {
            if (database?.Dic_BuildDB != null)
            {
                foreach (var info in database.Dic_BuildDB.Values)
                {
                    if (info != null)
                    {
                        yield return info;
                    }
                }
            }

            if (database?.Dic_BlockBuildDB != null)
            {
                foreach (var info in database.Dic_BlockBuildDB.Values)
                {
                    if (info != null)
                    {
                        yield return info;
                    }
                }
            }

            if (database?.Dic_RailDB != null)
            {
                foreach (var info in database.Dic_RailDB.Values)
                {
                    if (info != null)
                    {
                        yield return info;
                    }
                }
            }

            if (database?.Dic_LiftRailDB != null)
            {
                foreach (var info in database.Dic_LiftRailDB.Values)
                {
                    if (info != null)
                    {
                        yield return info;
                    }
                }
            }
        }

        private static BuildInfo Clone(BuildInfo source)
        {
            return new BuildInfo
            {
                Key = source.Key,
                Category = source.Category,
                Name = source.Name,
                BuildingNameToString = source.BuildingNameToString,
                List_Condition = Copy(source.List_Condition),
                Master = source.Master,
                ResearchType = source.ResearchType,
                Index = source.Index,
                Sort = source.Sort,
                Enable = source.Enable,
                Dungeon = source.Dungeon,
                T_Name = source.T_Name,
                T_Script = source.T_Script,
                Grade = source.Grade,
                Level = source.Level,
                Variation = source.Variation,
                OriginLevel = source.OriginLevel,
                Width = source.Width,
                Height = source.Height,
                Range = source.Range,
                HP = source.HP,
                BP = source.BP,
                Cost = source.Cost,
                Payment = source.Payment,
                ElecCost = source.ElecCost,
                m_Religion = source.m_Religion,
                m_PDI = source.m_PDI,
                List_JobAbility = Copy(source.List_JobAbility),
                List_JobAbilityValue = Copy(source.List_JobAbilityValue),
                List_EffectAbility = Copy(source.List_EffectAbility),
                List_EffectAbilityValue = Copy(source.List_EffectAbilityValue),
                List_Material = Copy(source.List_Material),
                List_Material_Num = Copy(source.List_Material_Num),
                List_Effect3 = Copy(source.List_Effect3),
                List_Effect3_Num = Copy(source.List_Effect3_Num),
                List_Effect3_Index = Copy(source.List_Effect3_Index),
                Ability = source.Ability,
                Desire_Name = source.Desire_Name,
                Dungeon_Order = source.Dungeon_Order,
                Dungeon_Group = source.Dungeon_Group,
                EffectValue1 = source.EffectValue1,
                EffectValue1_Num = source.EffectValue1_Num,
                EffectValue2 = source.EffectValue2,
                EffectValue2_Num = source.EffectValue2_Num,
                EffectValue3 = source.EffectValue3,
                EffectValue3_Num = source.EffectValue3_Num
            };
        }

        private static List<T> Copy<T>(List<T> source)
        {
            return source == null ? new List<T>() : new List<T>(source);
        }
    }
}
