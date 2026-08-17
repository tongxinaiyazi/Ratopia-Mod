using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Mono.Cecil;
using Xunit;

namespace ExecutionPlatform.Tests
{
    public sealed class GameContractTests
    {
        private const string ExpectedAssemblySha256 =
            "C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D";

        [Fact]
        public void AssemblyCSharpMatchesRatopiaOnePointZeroPointZeroSixHundred()
        {
            using (var stream = File.OpenRead(ContractTestPaths.GameAssembly))
            using (var sha = SHA256.Create())
            {
                var actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
                Assert.Equal(ExpectedAssemblySha256, actual);
            }
        }

        [Fact]
        public void CriticalEnumValuesMatchTheInspectedBuild()
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.GameAssembly))
            {
                AssertEnum(module, "BuildingName", "Prison", 219);
                AssertEnum(module, "BuildAbility", "None", 0);
                AssertEnum(module, "DesireName", "None", 0);
                AssertEnum(module, "BuildState", "Basic", 0);
                AssertEnum(module, "UnitKind", "Citizen", 1);
                AssertEnum(module, "CharState", "Death", 7);
                AssertEnum(module, "CharState", "Injury", 10);
                AssertEnum(module, "AniState", "Idle", 0);
                AssertEnum(module, "CitizenState", "Nothing", 1);
                AssertEnum(module, "CitizenState", "Working", 12);
                AssertEnum(module, "C_Key", "JobReCompass", 3);
            }
        }

        [Fact]
        public void HarmonyTargetsKeepTheirCurrentSignatures()
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.GameAssembly))
            {
                AssertMethod(module, "DB_Mgr", "Build_DB_Setting", "System.Void");
                AssertMethod(module, "DB_Mgr", "IsLockBuilding", "System.Boolean", "System.Int32");
                AssertMethod(module, "Func", "LoadSprite", "UnityEngine.Sprite", "System.String");
                AssertMethod(module, "Helpers", "IsMagicianBuilding", "System.Boolean", "BuildInfo");
                AssertMethod(module, "BuildingMgr", "BuildSet", "Building",
                    "BuildingName", "UnityEngine.Vector2", "System.Int32");
                AssertMethod(module, "BuildingMgr", "AddToPool", "System.Void", "Building");
                AssertMethod(module, "T_Citizen", "JobSet", "System.Void", "Building");
                AssertMethod(module, "T_Citizen", "JobFire", "System.Void", "System.Boolean");
                AssertMethod(module, "T_Citizen", "UpdateFunction", "System.Void");
                AssertMethod(module, "PlayDataMgr", "BeforeLoad", "System.Void");
            }
        }

        [Fact]
        public void RuntimeCallsAndInjectedFieldsKeepTheirCurrentSignatures()
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.GameAssembly))
            {
                AssertMethod(module, "Building", "BuildingSet", "System.Void",
                    "BuildInfo", "UnityEngine.Vector2", "System.Int32");
                AssertMethod(module, "Building", "IsInArea", "System.Boolean",
                    "System.Int32", "UnityEngine.Vector2Int");
                AssertMethod(module, "Building", "BuildingDemolition", "System.Void", "System.Boolean");
                AssertMethod(module, "T_Citizen", "PathFindCall", "System.Boolean",
                    "UnityEngine.Vector3", "CitizenState", "C_Key", "System.Boolean");
                AssertMethod(module, "T_Citizen", "ForJob_WakeUp", "System.Void");
                AssertMethod(module, "T_Citizen", "BehaviorStop", "System.Void", "System.Boolean");
                AssertMethod(module, "T_Citizen", "DrownCheck", "System.Void");
                AssertMethod(module, "T_Citizen", "InjuryCheck", "System.Void");
                AssertMethod(module, "T_Citizen", "IsNotNormalState", "System.Boolean");
                AssertMethod(module, "GameUnit", "HpUpdate", "System.Void", "System.Single", "System.Single");
                AssertMethod(module, "T_Citizen", "DeathCheck", "System.Void", "System.Int32");
                AssertMethod(module, "MemoryPool", "GetNextObj", "UnityEngine.GameObject");

                AssertField(module, "DB_Mgr", "Dic_BuildDB",
                    "System.Collections.Generic.Dictionary`2<BuildingName,BuildInfo>");
                AssertField(module, "BuildingMgr", "List_Pool",
                    "System.Collections.Generic.List`1<MemoryPool>");
                AssertField(module, "Building", "m_Info", "BuildInfo");
                AssertField(module, "Building", "m_BuildState", "BuildState");
                AssertField(module, "Building", "m_Activation", "System.Boolean");
                AssertField(module, "Building", "m_Demolition", "System.Boolean");
                AssertField(module, "Building", "m_Body", "BuildingBody");
                AssertField(module, "Building", "m_ID", "System.Int32");
                AssertField(module, "BuildingBody", "m_Animator", "UnityEngine.Animator");
                AssertField(module, "GameUnit", "m_Job", "Building");
                AssertField(module, "GameUnit", "m_CurNode", "C_Node");
                AssertField(module, "GameUnit", "m_CurHP", "System.Single");
                AssertField(module, "GameUnit", "m_UnitKind", "UnitKind");
                AssertField(module, "T_Citizen", "m_ImprisonCheck", "System.Boolean");
                AssertField(module, "PlayDataMgr", "IsLoadGame", "System.Boolean");
            }
        }

        private static TypeDefinition FindType(ModuleDefinition module, string name)
        {
            return module.Types.Single(type => type.FullName == name);
        }

        private static void AssertEnum(ModuleDefinition module, string type, string field, int expected)
        {
            Assert.Equal(expected, Convert.ToInt32(FindType(module, type).Fields.Single(item => item.Name == field).Constant));
        }

        private static void AssertField(ModuleDefinition module, string type, string field, string fieldType)
        {
            var actual = FindType(module, type).Fields.Single(item => item.Name == field);
            Assert.True(actual.IsPublic);
            Assert.Equal(fieldType, actual.FieldType.FullName);
        }

        private static void AssertMethod(
            ModuleDefinition module,
            string type,
            string name,
            string returnType,
            params string[] parameters)
        {
            Assert.Contains(FindType(module, type).Methods, method =>
                method.Name == name &&
                method.ReturnType.FullName == returnType &&
                method.Parameters.Select(parameter => parameter.ParameterType.FullName).SequenceEqual(parameters));
        }
    }
}
