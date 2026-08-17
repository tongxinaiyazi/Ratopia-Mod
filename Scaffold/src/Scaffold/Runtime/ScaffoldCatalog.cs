using System.Collections.Generic;

namespace ScaffoldMod.Runtime
{
    internal static class ScaffoldCatalog
    {
        internal const int RuntimeBuildingValue = 10000;
        internal static readonly BuildingName RuntimeBuildingName = (BuildingName)RuntimeBuildingValue;

        internal static bool IsScaffold(BuildInfo info)
        {
            return info != null && info.Name.Equals(RuntimeBuildingName);
        }

        internal static void Register(DB_Mgr database)
        {
            if (database == null || database.Dic_BuildDB == null ||
                database.Dic_BuildDB.ContainsKey(RuntimeBuildingName) ||
                !database.Dic_BuildDB.TryGetValue(BuildingName.Ladder, out var ladder))
            {
                return;
            }

            var scaffold = new BuildInfo
            {
                Key = "Scaffold",
                Category = ladder.Category,
                Name = RuntimeBuildingName,
                BuildingNameToString = "Scaffold",
                Master = ladder.Master,
                ResearchType = ladder.ResearchType,
                Index = ladder.Index,
                Sort = ladder.Sort + 1,
                Enable = 1,
                Dungeon = 0,
                T_Name = "脚手架",
                T_Script = "消耗1个木板瞬间搭建；5个游戏日后自动拆除，并在原地返还1个木板。可与建筑同格。",
                Grade = ladder.Grade,
                Level = ladder.Level,
                Variation = 1,
                OriginLevel = ladder.OriginLevel,
                Width = 1,
                Height = 1,
                Range = 0,
                HP = ladder.HP,
                BP = 0,
                Cost = 0,
                Payment = 0,
                ElecCost = 0,
                Ability = BuildAbility.Ladder,
                List_Condition = new List<BuildCondition> { BuildCondition.BuildLadder },
                List_Material = new List<TileType> { TileType.Lumber },
                List_Material_Num = new List<int> { 1 },
                List_JobAbility = new List<Res_Ability>(),
                List_JobAbilityValue = new List<float>(),
                List_EffectAbility = new List<Res_Ability>(),
                List_EffectAbilityValue = new List<float>(),
                List_Effect3 = new List<TileType>(),
                List_Effect3_Num = new List<int>(),
                List_Effect3_Index = new List<int>()
            };

            database.Dic_BuildDB.Add(RuntimeBuildingName, scaffold);
            ScaffoldRuntime.LogInfo("已注册独立建造项目：脚手架。");
        }
    }
}
