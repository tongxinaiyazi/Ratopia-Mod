using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Mono.Cecil;
using Xunit;

namespace Scaffold.Tests
{
    public sealed class GameContractTests
    {
        private const string ExpectedAssemblySha256 =
            "C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D";

        [Fact]
        public void AssemblyCSharpMatchesRatopiaOnePointZeroPointZeroSixHundred()
        {
            using (var stream = File.OpenRead(GetAssemblyPath()))
            using (var sha = SHA256.Create())
            {
                var actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
                Assert.Equal(ExpectedAssemblySha256, actual);
            }
        }

        [Fact]
        public void CriticalEnumValuesAreStable()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                AssertEnum(module, "BuildingName", "Ladder", 0);
                AssertEnum(module, "TileType", "Ladder", 50);
                AssertEnum(module, "TileType", "Lumber", 1008);
                AssertEnum(module, "NodeType", "Ladder", 3);
                AssertEnum(module, "MiningBoxMode", "Demolition", 4);
                AssertEnum(module, "WorkMarkKind", "Demolition", 6);
            }
        }

        [Fact]
        public void HarmonyTargetsKeepTheirInspectedSignatures()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                AssertMethod(module, "DB_Mgr", "Build_DB_Setting", "System.Void");
                AssertMethod(module, "MiningBox", "BuildEnableCheck", "System.Int32");
                AssertMethod(module, "MiningBox", "Update", "System.Void");
                AssertMethod(module, "MiningBox", "IsMiningEnableTile", "System.Boolean", "UnityEngine.Vector2");
                AssertMethod(module, "BP_Building", "BluePrintSet", "BP_Building",
                    "BuildInfo", "UnityEngine.Vector2", "System.Int32", "System.Int32");
                AssertMethod(module, "BP_Building", "CancelBP", "System.Void");
                AssertMethod(module, "TileMgr", "Update", "System.Void");
                AssertMethod(module, "TileMgr", "MapDataMapping", "System.Void", "D_Data");
                AssertMethod(module, "TileMgr", "NodeTypeCheck", "System.Void",
                    "System.Int32", "System.Int32", "System.Boolean");
                AssertMethod(module, "WorkMark", "MarkRefresh", "System.Void", "Building");
                AssertMethod(module, "PlayDataMgr", "LoadData", "System.Void", "D_Data");
                AssertMethod(module, "PlayDataMgr", "BeforeLoad", "System.Void");
            }
        }

        [Fact]
        public void HarmonyInjectedFieldsKeepTheirTypes()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                AssertField(module, "MiningBox", "Tf", "UnityEngine.Transform");
                AssertField(module, "MiningBox", "m_Mode", "MiningBoxMode");
                AssertField(module, "MiningBox", "m_DeleteMode", "System.Boolean");
                AssertField(module, "MiningBox", "m_BuildInfo", "BuildInfo");
                AssertField(module, "MiningBox", "m_BuildEnable", "System.Boolean");
                AssertField(module, "BP_Building", "Pos_Tile", "UnityEngine.Vector2");
                AssertField(module, "C_Node", "m_NodeType", "NodeType");
                AssertField(module, "C_Tile", "m_X", "System.Int32");
                AssertField(module, "C_Tile", "m_Y", "System.Int32");
                AssertField(module, "D_Data", "ModsData", "Utility.Savable.SavableData");
            }
        }

        private static string GetAssemblyPath()
        {
            var ratopiaDir = Environment.GetEnvironmentVariable("RATOPIA_DIR");
            if (string.IsNullOrWhiteSpace(ratopiaDir))
            {
                ratopiaDir = typeof(GameContractTests).Assembly
                    .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                    .Cast<AssemblyMetadataAttribute>()
                    .Single(attribute => attribute.Key == "RatopiaDir")
                    .Value;
            }

            Assert.False(string.IsNullOrWhiteSpace(ratopiaDir));
            var path = Path.Combine(ratopiaDir, "Ratopia_Data", "Managed", "Assembly-CSharp.dll");
            Assert.True(File.Exists(path), $"Assembly-CSharp.dll not found: {path}");
            return path;
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
            Assert.Equal(fieldType, FindType(module, type).Fields.Single(item => item.Name == field).FieldType.FullName);
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
