using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Mono.Cecil;
using Xunit;

namespace GodViewManagement.Tests
{
    public sealed class GameContractTests
    {
        private const string ExpectedAssemblySha256 =
            "C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D";

        [Fact]
        public void AssemblyCSharpMatchesInspectedBuild()
        {
            using (var stream = File.OpenRead(GetAssemblyPath()))
            using (var sha = SHA256.Create())
            {
                var actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
                Assert.Equal(ExpectedAssemblySha256, actual);
            }
        }

        [Fact]
        public void RuntimeTargetsKeepExactSignatures()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                AssertMethod(module, "TileMgr", "Update", "System.Void");
                AssertMethod(module, "TileMgr", "TileChunkEnable_Update", "System.Void", "UnityEngine.Vector2");
                AssertMethod(module, "CameraMgr", "GetMouseIntPos", "UnityEngine.Vector2Int");
                AssertMethod(module, "CameraMgr", "Tf_Update_ByCut", "UnityEngine.Vector2", "UnityEngine.Vector2", "System.Boolean");
                AssertMethod(module, "BuildingMgr", "GetBuildingByBuildPos_Area", "Building", "UnityEngine.Vector2Int");
                AssertMethod(module, "BuildMidUI", "BuildMid_Open", "System.Void", "BuildInfoUI", "System.Action");
                AssertMethod(module, "BuildMidUI", "QueenBtn", "System.Void", "System.Boolean");
                AssertMethod(module, "BuildMidUI", "QueenBtn2", "System.Void", "System.Boolean");
                AssertMethod(module, "BuildMidUI", "QueenBtn3", "System.Void", "System.Boolean");
                AssertMethod(module, "BuildMidUI", "QueenBtn4", "System.Void", "System.Boolean");
                AssertMethod(module, "BuildMidUI", "QueenBtn5", "System.Void", "System.Boolean");
                AssertMethod(module, "T_Queen", "IsQueenSafeState", "System.Boolean");
                AssertMethod(module, "T_Queen", "CharacterStop", "System.Void");
                AssertMethod(module, "CasselGames.Input.InputMgr", "SetActionMap", "System.Void", "System.String");
                AssertMethod(module, "CasselGames.Input.InputMgr", "SetDefaultActionMap", "System.Void");
            }
        }

        [Fact]
        public void QueenInputRuntimeTargetsKeepExactSignatures()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                var queenUpdate = FindMethod(module, "T_Queen", "Update", "System.Void");
                Assert.False(queenUpdate.IsStatic);

                var directionalGetKey = FindMethod(
                    module,
                    "CasselGames.Input.InputMgr",
                    "GetKey",
                    "System.Boolean",
                    "HotKeyName",
                    "System.Boolean");
                Assert.True(directionalGetKey.IsStatic);
            }
        }

        [Fact]
        public void RuntimeFieldsAndEnumsKeepInspectedContract()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                AssertField(module, "TileMgr", "m_GameLoading");
                AssertField(module, "CameraMgr", "m_FixQueen", "System.Boolean");
                AssertField(module, "CameraMgr", "m_MainCam", "UnityEngine.Camera");
                AssertField(module, "Building", "m_BuildInfoUI", "BuildInfoUI");
                AssertField(module, "Building", "m_Info", "BuildInfo");
                AssertField(module, "Building", "m_BuildState", "BuildState");
                AssertField(module, "BuildInfo", "Ability", "BuildAbility");
                AssertField(module, "BuildInfo", "Name", "BuildingName");
                AssertField(module, "BuildMidUI", "m_QueenSlot");
                AssertField(module, "BuildMidUI", "m_QueenSlot2");
                AssertField(module, "BuildMidUI", "m_QueenSlot3");
                AssertField(module, "BuildMidUI", "m_QueenSlot4");
                AssertField(module, "BuildMidUI", "m_QueenSlot5");
                AssertField(module, "BuildMidUI", "Obj_Main", "UnityEngine.GameObject");
                AssertField(module, "BuildInfoUI", "m_Building", "Building");
                AssertField(module, "GameUnit", "Tf", "UnityEngine.Transform");
                AssertField(module, "T_Queen", "m_CamBottomHeight");

                AssertEnumValue(module, "BuildState", "Basic", 0);
                AssertEnumValue(module, "BuildAbility", "Wallpaper", 37);
                AssertEnumValue(module, "BuildingName", "EnemyNexus", 321);
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

        private static TypeDefinition FindType(ModuleDefinition module, string fullName)
        {
            return module.Types.Single(type => type.FullName == fullName);
        }

        private static void AssertMethod(ModuleDefinition module, string typeName, string methodName, string returnType, params string[] parameterTypes)
        {
            Assert.NotNull(FindMethod(module, typeName, methodName, returnType, parameterTypes));
        }

        private static MethodDefinition FindMethod(ModuleDefinition module, string typeName, string methodName, string returnType, params string[] parameterTypes)
        {
            return FindType(module, typeName).Methods.Single(item =>
                item.Name == methodName
                && item.ReturnType.FullName == returnType
                && item.Parameters.Select(parameter => parameter.ParameterType.FullName).SequenceEqual(parameterTypes));
        }

        private static void AssertField(ModuleDefinition module, string typeName, string fieldName, string fieldType = null)
        {
            var field = FindType(module, typeName).Fields.Single(item => item.Name == fieldName);
            if (fieldType != null)
            {
                Assert.Equal(fieldType, field.FieldType.FullName);
            }
        }

        private static void AssertEnumValue(ModuleDefinition module, string typeName, string fieldName, int expected)
        {
            var field = FindType(module, typeName).Fields.Single(item => item.Name == fieldName);
            Assert.Equal(expected, Convert.ToInt32(field.Constant));
        }
    }
}
