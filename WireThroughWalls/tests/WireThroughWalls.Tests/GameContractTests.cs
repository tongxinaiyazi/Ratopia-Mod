using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Mono.Cecil;
using Xunit;

namespace WireThroughWalls.Tests
{
    public sealed class GameContractTests
    {
        private const string ExpectedAssemblySha256 =
            "C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D";

        [Fact]
        public void AssemblyCSharpMatchesTheInspectedBuild()
        {
            var path = GetAssemblyPath();

            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
            {
                var actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
                Assert.Equal(ExpectedAssemblySha256, actual);
            }
        }

        [Fact]
        public void CriticalEnumsRetainTheirInspectedValues()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                AssertEnumValue(module, "BuildingName", "HeavyWire", 276);
                AssertEnumValue(module, "BuildingName", "Wireroad", 277);
                AssertEnumValue(module, "BuildAbility", "HeavyWire", 58);
                AssertEnumValue(module, "TileType", "None", -50);
                AssertEnumValue(module, "TileType", "Building", -40);
                AssertEnumValue(module, "TileType", "Door_C", -38);
                AssertEnumValue(module, "TileType", "Railroad", -37);
                AssertEnumValue(module, "TileType", "LiftRail", -36);
                AssertEnumValue(module, "TileType", "WallBuilding", -34);
                AssertEnumValue(module, "MiningBoxMode", "Demolition", 4);
            }
        }

        [Fact]
        public void HarmonyTargetsKeepTheirExactSignatures()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                AssertMethod(module, "MiningBox", "BuildEnableCheck", "System.Int32");
                AssertMethod(module, "MiningBox", "Update", "System.Void");
                AssertMethod(module, "MiniInfoBox", "Selected", "System.Void");
                AssertMethod(module, "QueenCheckBox", "OnTriggerEnter2D", "System.Void",
                    "UnityEngine.Collider2D");
                AssertMethod(module, "BP_Building", "BluePrintSet", "BP_Building",
                    "BuildInfo", "UnityEngine.Vector2", "System.Int32", "System.Int32");
                AssertMethod(module, "BP_Building", "EnableCheck", "System.Void");
                AssertMethod(module, "BP_Building", "BuildingUpdate_Call", "System.Void", "GameUnit");
                AssertMethod(module, "BP_Building", "CancelBP", "System.Void");
                AssertMethod(module, "Building_HeavyWire", "BuildingSet", "System.Void",
                    "BuildInfo", "UnityEngine.Vector2", "System.Int32");
                AssertMethod(module, "Building_HeavyWire", "BuildingDemolition", "System.Void", "System.Boolean");
                AssertMethod(module, "BuildingMgr", "FindBuildingByBpos", "Building", "UnityEngine.Vector2Int");
                AssertMethod(module, "BuildingMgr", "NewConnectCheck", "System.Void", "ElecPort", "System.Single");
                AssertMethod(module, "BuildingMgr", "DeleteConnectCheck", "System.Void", "ElecPort");
                AssertMethod(module, "BuildingMgr", "DeleteConnectCheck", "System.Void", "System.Int32",
                    "System.Collections.Generic.List`1<ElecPort>");
                AssertMethod(module, "BuildingMgr", "MergeTwoElecLine", "System.Void",
                    "ElecLine_Info", "ElecLine_Info");
                AssertMethod(module, "C_Tile", "DestroyTile", "System.Void",
                    "System.Boolean", "System.Boolean", "GameUnit");
            }
        }

        [Fact]
        public void HarmonyInjectedFieldsKeepTheirTypes()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                AssertField(module, "MiningBox", "Tf", "UnityEngine.Transform");
                AssertField(module, "MiningBox", "m_Mode", "MiningBoxMode");
                AssertField(module, "MiningBox", "m_BuildInfo", "BuildInfo");
                AssertField(module, "MiningBox", "m_BuildEnable", "System.Boolean");
                AssertField(module, "MiniInfoBox", "m_Info", "MiniInfo");
                AssertField(module, "MiniInfo", "m_Building", "Building");
                AssertField(module, "MiniInfo", "m_BP_Building", "BP_Building");
                AssertField(module, "QueenCheckBox", "m_Building", "Building");
                AssertField(module, "QueenCheckBox", "m_BP_Building", "BP_Building");
                AssertField(module, "BuildingMgr", "List_BP_BlueBuilding",
                    "System.Collections.Generic.List`1<BP_Building>");
                AssertField(module, "BuildingMgr", "List_Building",
                    "System.Collections.Generic.List`1<Building>");
                AssertField(module, "BuildingMgr", "List_HeavyWire",
                    "System.Collections.Generic.List`1<Building_HeavyWire>");
                AssertField(module, "BP_Building", "List_BuildPos",
                    "System.Collections.Generic.List`1<UnityEngine.Vector2Int>");
                AssertField(module, "C_Node", "m_TileType", "TileType");
                AssertField(module, "C_Node", "m_NodeType", "NodeType");
                AssertField(module, "C_Node", "m_BuildType", "BuildType");
                AssertField(module, "C_Node", "m_RailSlope", "System.Int32");
                AssertField(module, "C_Node", "m_WorldObj", "WorldObject");
                AssertField(module, "BuildingMgr", "Dic_PortTileMap",
                    "System.Collections.Generic.Dictionary`2<UnityEngine.Vector2Int,ElecPort>");
                AssertField(module, "ElecPort", "m_ID", "System.Int32");
                AssertField(module, "ElecPort", "m_PortType", "PortType");
                AssertField(module, "ElecPort", "m_X", "System.Int32");
                AssertField(module, "ElecPort", "m_Y", "System.Int32");
                AssertField(module, "TileMgr", "m_GameLoading", "System.Boolean");
            }
        }

        [Fact]
        public void SpecialBuildingsStillWriteDedicatedNodeState()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                AssertWritesField(module, "Buildiing_Railroad", "BuildingSet", "C_Node", "m_RailSlope");
                AssertCalls(module, "Building_Door", "BuildingSet", "C_Node", "TileSet");
                AssertWritesField(module, "Building_LiftPlatform", "BuildingSet", "C_Node", "m_TileType");
                AssertWritesField(module, "Building_ElecLiftPlatform", "BuildingSet", "C_Node", "m_TileType");
                AssertCalls(module, "Building_WaterScrew", "BuildingSet", "C_Node", "TileSet");
            }
        }

        [Fact]
        public void OriginalDangerousLifecycleCallsStillExist()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                var blueprintCompletion = FindMethod(module, "BP_Building", "BuildingUpdate_Call", "System.Void", "GameUnit");
                var blueprintSet = FindMethod(module, "BP_Building", "BluePrintSet", "BP_Building",
                    "BuildInfo", "UnityEngine.Vector2", "System.Int32", "System.Int32");
                var heavyWireSet = FindMethod(module, "Building_HeavyWire", "BuildingSet", "System.Void",
                    "BuildInfo", "UnityEngine.Vector2", "System.Int32");

                Assert.Contains(blueprintSet.Body.Instructions,
                    instruction => instruction.Operand is MethodReference method &&
                                   method.DeclaringType.FullName == "C_Tile" &&
                                   method.Name == "DestroyTile");
                Assert.Contains(blueprintCompletion.Body.Instructions,
                    instruction => instruction.Operand is MethodReference method &&
                                   method.DeclaringType.FullName == "C_Tile" &&
                                   method.Name == "DestroyTile");
                Assert.Contains(blueprintCompletion.Body.Instructions,
                    instruction => instruction.Operand is MethodReference method &&
                                   method.DeclaringType.FullName == "Building" &&
                                   method.Name == "BuildingDemolition");
                Assert.Contains(heavyWireSet.Body.Instructions,
                    instruction => instruction.Operand is FieldReference field &&
                                   field.DeclaringType.FullName == "C_Node" &&
                                   field.Name == "m_TileType");
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

            Assert.False(
                string.IsNullOrWhiteSpace(ratopiaDir),
                "RATOPIA_DIR or the RatopiaDir MSBuild property must point to the Ratopia root.");
            var path = Path.Combine(ratopiaDir, "Ratopia_Data", "Managed", "Assembly-CSharp.dll");
            Assert.True(File.Exists(path), $"Assembly-CSharp.dll not found: {path}");
            return path;
        }

        private static void AssertEnumValue(ModuleDefinition module, string typeName, string fieldName, int expected)
        {
            var field = FindType(module, typeName).Fields.Single(item => item.Name == fieldName);
            Assert.Equal(expected, Convert.ToInt32(field.Constant));
        }

        private static void AssertField(ModuleDefinition module, string typeName, string fieldName, string fieldType)
        {
            var field = FindType(module, typeName).Fields.Single(item => item.Name == fieldName);
            Assert.Equal(fieldType, field.FieldType.FullName);
        }

        private static void AssertMethod(
            ModuleDefinition module,
            string typeName,
            string methodName,
            string returnType,
            params string[] parameterTypes)
        {
            Assert.NotNull(FindMethod(module, typeName, methodName, returnType, parameterTypes));
        }

        private static void AssertWritesField(
            ModuleDefinition module,
            string typeName,
            string methodName,
            string declaringType,
            string fieldName)
        {
            var method = FindType(module, typeName).Methods.Single(item => item.Name == methodName);
            Assert.Contains(method.Body.Instructions,
                instruction => instruction.Operand is FieldReference field &&
                               field.DeclaringType.FullName == declaringType &&
                               field.Name == fieldName);
        }

        private static void AssertCalls(
            ModuleDefinition module,
            string typeName,
            string methodName,
            string declaringType,
            string calledName)
        {
            var method = FindType(module, typeName).Methods.Single(item => item.Name == methodName);
            Assert.Contains(method.Body.Instructions,
                instruction => instruction.Operand is MethodReference called &&
                               called.DeclaringType.FullName == declaringType &&
                               called.Name == calledName);
        }

        private static MethodDefinition FindMethod(
            ModuleDefinition module,
            string typeName,
            string methodName,
            string returnType,
            params string[] parameterTypes)
        {
            return FindType(module, typeName).Methods.Single(method =>
                method.Name == methodName &&
                method.ReturnType.FullName == returnType &&
                method.Parameters.Select(parameter => parameter.ParameterType.FullName).SequenceEqual(parameterTypes));
        }

        private static TypeDefinition FindType(ModuleDefinition module, string fullName)
        {
            return module.Types.Single(type => type.FullName == fullName);
        }
    }
}
