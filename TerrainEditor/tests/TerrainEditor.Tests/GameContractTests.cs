using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Xunit;

namespace TerrainEditor.Tests
{
    public sealed class GameContractTests
    {
        private static readonly string RatopiaDir = typeof(GameContractTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "RatopiaDir")
            .Value;

        [Fact]
        public void TargetEnvironmentIsBepInEx5MonoWithExpectedRuntimeVersions()
        {
            var assemblyCSharp = GamePath("Ratopia_Data", "Managed", "Assembly-CSharp.dll");
            var bepinex = GamePath("BepInEx", "core", "BepInEx.dll");
            var harmony = GamePath("BepInEx", "core", "0Harmony.dll");

            Assert.True(File.Exists(assemblyCSharp));
            Assert.Equal(new Version(5, 4, 23, 5), AssemblyName.GetAssemblyName(bepinex).Version);
            Assert.Equal(new Version(2, 9, 0, 0), AssemblyName.GetAssemblyName(harmony).Version);
        }

        [Fact]
        public void GatewayTypesAndMembersExistInCurrentGameAssembly()
        {
            using (var assembly = ReadGameAssembly())
            {
                AssertField(assembly, "GameMgr", "_TileMgr", "TileMgr");
                AssertField(assembly, "GameMgr", "_CamMgr", "CameraMgr");
                AssertField(assembly, "DebugMgr", "_PallateMgr", "PallateMgr");
                AssertField(assembly, "TileMgr", "IsSandBoxMode", "System.Boolean");
                AssertField(assembly, "PallateMgr", "Obj_Main", "UnityEngine.GameObject");
                AssertField(assembly, "PallateMgr", "m_Icons", "PallateIcon[]");
                AssertField(assembly, "PallateMgr", "m_BrushType", "System.Int32");
                AssertField(assembly, "PallateIcon", "m_Outline", "UnityEngine.UI.Outline");
                AssertField(assembly, "CameraMgr", "m_MainCam", "UnityEngine.Camera");
                AssertMethod(assembly, "PallateIcon", "MouseUp", "System.Void");
                AssertMethod(assembly, "CameraMgr", "ZoomSizeUpdate", "System.Void", "System.Single");
                AssertMethod(assembly, "TileMgr", "Update", "System.Void");
                AssertMethod(assembly, "LoadingSceneMgr", "Start", "System.Void");
            }
        }

        [Fact]
        public void NativePaletteStillImplementsAllRestoredEditorCategories()
        {
            using (var assembly = ReadGameAssembly())
            {
                var update = FindType(assembly, "PallateMgr").Methods.Single(method => method.Name == "Update");
                var calls = new HashSet<string>(
                    update.Body.Instructions
                        .Select(instruction => instruction.Operand as MethodReference)
                        .Where(method => method != null)
                        .Select(method => method.DeclaringType.Name + "." + method.Name));

                Assert.Contains("TileMgr.DestroyTile", calls);
                Assert.Contains("TileMgr.MakeTile", calls);
                Assert.Contains("C_Tile.DestroyTile", calls);
                Assert.Contains("EnvironmentMgr.SpawnPlant", calls);
                Assert.Contains("EnvironmentMgr.DestroyPlant", calls);
                Assert.Contains("MapObjMgr.MakeMapObj", calls);
                Assert.Contains("MapObjMgr.DestroyMapObj", calls);
                Assert.Contains("BuildingMgr.BuildSet", calls);
                Assert.Contains("CameraMgr.ZoomSizeUpdate", calls);
                Assert.Contains("Input.GetAxis", calls);
                Assert.Contains("Input.GetKey", calls);
            }
        }

        private static AssemblyDefinition ReadGameAssembly()
        {
            return AssemblyDefinition.ReadAssembly(GamePath("Ratopia_Data", "Managed", "Assembly-CSharp.dll"));
        }

        private static string GamePath(params string[] parts)
        {
            var path = RatopiaDir;
            foreach (var part in parts)
            {
                path = Path.Combine(path, part);
            }

            return path;
        }

        private static TypeDefinition FindType(AssemblyDefinition assembly, string name)
        {
            var type = assembly.MainModule.Types.SingleOrDefault(candidate => candidate.Name == name);
            Assert.NotNull(type);
            return type;
        }

        private static void AssertField(
            AssemblyDefinition assembly,
            string typeName,
            string fieldName,
            string fieldType)
        {
            var field = FindType(assembly, typeName).Fields.SingleOrDefault(candidate => candidate.Name == fieldName);
            Assert.NotNull(field);
            Assert.Equal(fieldType, field.FieldType.FullName);
        }

        private static void AssertMethod(
            AssemblyDefinition assembly,
            string typeName,
            string methodName,
            string returnType,
            params string[] parameterTypes)
        {
            var method = FindType(assembly, typeName).Methods.SingleOrDefault(candidate =>
                candidate.Name == methodName
                && candidate.Parameters.Select(parameter => parameter.ParameterType.FullName).SequenceEqual(parameterTypes));
            Assert.NotNull(method);
            Assert.Equal(returnType, method.ReturnType.FullName);
        }
    }
}
