using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Mono.Cecil;
using Xunit;

namespace StrongerWorkDistance.Tests
{
    public sealed class GameContractTests
    {
        private const string ExpectedAssemblySha256 =
            "C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D";

        [Fact]
        public void AssemblyCSharpMatchesTheInspectedBuild()
        {
            using (var stream = File.OpenRead(GetAssemblyPath()))
            using (var sha = SHA256.Create())
            {
                var actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
                Assert.Equal(ExpectedAssemblySha256, actual);
            }
        }

        [Fact]
        public void SystemManagerRuntimeContractKeepsTheInspectedSignatures()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                var systemManager = module.Types.Single(type => type.FullName == "SystemMgr");
                var awake = systemManager.Methods.Single(method =>
                    method.Name == "Awake" &&
                    method.ReturnType.FullName == "System.Void" &&
                    method.Parameters.Count == 0);

                Assert.False(awake.IsStatic);
                AssertField(systemManager, "List_WM_EnableArea");
                AssertField(systemManager, "List_BP_Ld_EnableArea");
                AssertField(systemManager, "List_Queen_EnableArea");
            }
        }

        [Fact]
        public void TargetListsStillFeedAllPlannedWorkablePositionConsumers()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                AssertMethodReadsField(module, "C_Tile", "MakeEnableList", "List_WM_EnableArea");
                AssertMethodReadsField(module, "Building", "MakeEnableList", "List_WM_EnableArea");
                AssertMethodReadsField(module, "BP_Building", "MakeEnableList", "List_WM_EnableArea");
                AssertMethodReadsField(module, "BP_Building", "MakeEnableList", "List_BP_Ld_EnableArea");
                AssertMethodReadsField(module, "TileObject", "MakeEnableList", "List_BP_Ld_EnableArea");
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

        private static void AssertField(TypeDefinition systemManager, string fieldName)
        {
            var field = systemManager.Fields.Single(item => item.Name == fieldName);
            Assert.Equal("System.Collections.Generic.List`1<UnityEngine.Vector2Int>", field.FieldType.FullName);
        }

        private static void AssertMethodReadsField(
            ModuleDefinition module,
            string typeName,
            string methodName,
            string fieldName)
        {
            var method = module.Types.Single(type => type.FullName == typeName)
                .Methods.Single(item => item.Name == methodName && item.Parameters.Count == 0);

            Assert.Contains(method.Body.Instructions, instruction =>
                instruction.Operand is FieldReference field &&
                field.DeclaringType.FullName == "SystemMgr" &&
                field.Name == fieldName);
        }
    }
}
