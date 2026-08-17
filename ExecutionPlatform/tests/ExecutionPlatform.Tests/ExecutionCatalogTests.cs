using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using ExecutionPlatform.Runtime;
using Xunit;

namespace ExecutionPlatform.Tests
{
    public sealed class ExecutionCatalogTests
    {
        [Fact]
        public void RegistrationClonesPrisonAndAppliesExecutionOverrides()
        {
            var prison = CreatePrison();
            var database = CreateDatabase(prison);

            var registered = ExecutionCatalog.TryRegister(database, out var failure);

            Assert.True(registered, failure);
            var execution = database.Dic_BuildDB[ExecutionCatalog.RuntimeBuildingName];
            Assert.Equal(ExecutionCatalog.RuntimeBuildingName, execution.Name);
            Assert.Equal(ExecutionCatalog.RuntimeBuildingValue, execution.Index);
            Assert.Equal("ExecutionPlatform", execution.Key);
            Assert.Equal("Prison", execution.BuildingNameToString);
            Assert.Equal("处刑台", execution.T_Name);
            Assert.Equal("指定一名鼠民后立即前来工作，工作1秒后生命值归零。", execution.T_Script);
            Assert.Equal(1, execution.Master);
            Assert.Equal(1, execution.Enable);
            Assert.Equal(0, execution.Level);
            Assert.Equal(0, execution.OriginLevel);
            Assert.Equal(0, execution.ResearchType);
            Assert.Equal(BuildAbility.None, execution.Ability);
            Assert.Equal(DesireName.None, execution.Desire_Name);
            Assert.Equal(prison.Width, execution.Width);
            Assert.Equal(prison.Height, execution.Height);
            Assert.Equal(prison.HP, execution.HP);
            Assert.Equal(prison.BP, execution.BP);
            Assert.Equal(prison.List_Material, execution.List_Material);
            Assert.Equal(prison.List_Material_Num, execution.List_Material_Num);
        }

        [Fact]
        public void EveryBuildInfoListIsDeepCopied()
        {
            var prison = CreatePrison();
            var database = CreateDatabase(prison);
            Assert.True(ExecutionCatalog.TryRegister(database, out _));
            var execution = database.Dic_BuildDB[ExecutionCatalog.RuntimeBuildingName];

            var listFields = typeof(BuildInfo).GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(field => typeof(IList).IsAssignableFrom(field.FieldType))
                .ToArray();

            Assert.NotEmpty(listFields);
            foreach (var field in listFields)
            {
                var source = Assert.IsAssignableFrom<IList>(field.GetValue(prison));
                var clone = Assert.IsAssignableFrom<IList>(field.GetValue(execution));
                Assert.NotSame(source, clone);
                Assert.Equal(source.Cast<object>(), clone.Cast<object>());
            }
        }

        [Fact]
        public void ExistingBuildingValueFailsClosedWithoutChangingTheDatabase()
        {
            var prison = CreatePrison();
            var existing = CreatePrison();
            existing.Name = ExecutionCatalog.RuntimeBuildingName;
            existing.Index = 9000;
            var database = CreateDatabase(prison, existing);
            var countBefore = database.Dic_BuildDB.Count;

            var registered = ExecutionCatalog.TryRegister(database, out var failure);

            Assert.False(registered);
            Assert.Contains("建筑值", failure);
            Assert.Equal(countBefore, database.Dic_BuildDB.Count);
            Assert.Same(existing, database.Dic_BuildDB[ExecutionCatalog.RuntimeBuildingName]);
        }

        [Fact]
        public void ExistingDatabaseIndexFailsClosedWithoutAddingTheBuilding()
        {
            var prison = CreatePrison();
            var existing = CreatePrison();
            existing.Name = BuildingName.WoodLadder;
            existing.Index = ExecutionCatalog.RuntimeBuildingValue;
            var database = CreateDatabase(prison, existing);
            var countBefore = database.Dic_BuildDB.Count;

            var registered = ExecutionCatalog.TryRegister(database, out var failure);

            Assert.False(registered);
            Assert.Contains("数据库索引", failure);
            Assert.Equal(countBefore, database.Dic_BuildDB.Count);
            Assert.False(database.Dic_BuildDB.ContainsKey(ExecutionCatalog.RuntimeBuildingName));
        }

        [Fact]
        public void BuildingValueCollisionInBlockDatabaseFailsClosed()
        {
            var prison = CreatePrison();
            var database = CreateDatabase(prison);
            var block = CreatePrison();
            block.Name = ExecutionCatalog.RuntimeBuildingName;
            block.Index = 7000;
            database.Dic_BlockBuildDB.Add(ExecutionCatalog.RuntimeBuildingName, block);

            Assert.False(ExecutionCatalog.TryRegister(database, out var failure));
            Assert.Contains("建筑值", failure);
            Assert.False(database.Dic_BuildDB.ContainsKey(ExecutionCatalog.RuntimeBuildingName));
        }

        [Fact]
        public void DatabaseIndexCollisionInRailDatabaseFailsClosed()
        {
            var prison = CreatePrison();
            var database = CreateDatabase(prison);
            var rail = CreatePrison();
            rail.Name = BuildingName.Railroad;
            rail.Index = ExecutionCatalog.RuntimeBuildingValue;
            database.Dic_RailDB.Add((TileType)9999, rail);

            Assert.False(ExecutionCatalog.TryRegister(database, out var failure));
            Assert.Contains("数据库索引", failure);
            Assert.False(database.Dic_BuildDB.ContainsKey(ExecutionCatalog.RuntimeBuildingName));
        }

        [Fact]
        public void MissingPrisonTemplateFailsClosed()
        {
            var database = (DB_Mgr)FormatterServices.GetUninitializedObject(typeof(DB_Mgr));
            database.Dic_BuildDB = new Dictionary<BuildingName, BuildInfo>();
            database.Dic_BlockBuildDB = new Dictionary<BuildingName, BuildInfo>();
            database.Dic_RailDB = new Dictionary<TileType, BuildInfo>();
            database.Dic_LiftRailDB = new Dictionary<TileType, BuildInfo>();

            Assert.False(ExecutionCatalog.TryRegister(database, out var failure));
            Assert.Contains("监狱", failure);
        }

        [Fact]
        public void RegistrationOwnershipDetectsConflictsAddedLater()
        {
            var database = CreateDatabase(CreatePrison());
            Assert.True(ExecutionCatalog.TryRegister(database, out _));
            Assert.True(ExecutionCatalog.IsOwnedRegistration(database));

            var laterConflict = CreatePrison();
            laterConflict.Name = BuildingName.WoodLadder;
            laterConflict.Index = ExecutionCatalog.RuntimeBuildingValue;
            database.Dic_BlockBuildDB.Add(BuildingName.WoodLadder, laterConflict);

            Assert.False(ExecutionCatalog.IsOwnedRegistration(database));
        }

        private static DB_Mgr CreateDatabase(params BuildInfo[] entries)
        {
            var database = (DB_Mgr)FormatterServices.GetUninitializedObject(typeof(DB_Mgr));
            database.Dic_BuildDB = entries.ToDictionary(item => item.Name);
            database.Dic_BlockBuildDB = new Dictionary<BuildingName, BuildInfo>();
            database.Dic_RailDB = new Dictionary<TileType, BuildInfo>();
            database.Dic_LiftRailDB = new Dictionary<TileType, BuildInfo>();
            return database;
        }

        private static BuildInfo CreatePrison()
        {
            return new BuildInfo
            {
                Key = "Prison",
                Category = (BuildCategory)2,
                Name = BuildingName.Prison,
                BuildingNameToString = "Prison",
                List_Condition = new List<BuildCondition> { (BuildCondition)3 },
                Master = 0,
                ResearchType = 7,
                Index = 219,
                Sort = 44,
                Enable = 1,
                Dungeon = 0,
                T_Name = "监狱",
                T_Script = "template",
                Grade = 2,
                Level = 4,
                Variation = 1,
                OriginLevel = 4,
                Width = 3,
                Height = 2,
                Range = 5,
                HP = 800,
                BP = 120,
                Cost = 13,
                Payment = 2,
                ElecCost = 0,
                m_Religion = (Religion)1,
                m_PDI = (PDI)2,
                List_JobAbility = new List<Res_Ability> { (Res_Ability)1 },
                List_JobAbilityValue = new List<float> { 2.5f },
                List_EffectAbility = new List<Res_Ability> { (Res_Ability)2 },
                List_EffectAbilityValue = new List<float> { 3.5f },
                List_Material = new List<TileType> { (TileType)1008, (TileType)1009 },
                List_Material_Num = new List<int> { 4, 5 },
                List_Effect3 = new List<TileType> { (TileType)1010 },
                List_Effect3_Num = new List<int> { 6 },
                List_Effect3_Index = new List<int> { 7 },
                Ability = (BuildAbility)8,
                Desire_Name = (DesireName)3,
                Dungeon_Order = (DungeonOrder)1,
                Dungeon_Group = (DungeonGroup)1,
                EffectValue1 = "a",
                EffectValue1_Num = 1.25f,
                EffectValue2 = "b",
                EffectValue2_Num = 2,
                EffectValue3 = "c",
                EffectValue3_Num = 3
            };
        }
    }
}
