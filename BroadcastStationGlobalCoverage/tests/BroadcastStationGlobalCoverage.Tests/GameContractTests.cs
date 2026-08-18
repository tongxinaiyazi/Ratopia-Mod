using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace BroadcastStationGlobalCoverage.Tests
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
        public void TargetEnumsAndFieldsKeepTheInspectedValues()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                AssertEnumValue(module, "BuildingName", "BroadcastStation", 309);
                AssertEnumValue(module, "BuildingName", "Television", 310);

                AssertField(module, "BuildInfo", "Name", "BuildingName", true);
                AssertField(module, "BuildInfo", "Range", "System.Int32", true);
                AssertField(module, "Building", "m_Info", "BuildInfo", true);
                AssertField(module, "Building", "m_Range", "System.Int32", true);
                AssertField(
                    module,
                    "UI_StorageSelect",
                    "List_Storage",
                    "System.Collections.Generic.List`1<Building>",
                    false);
            }
        }

        [Fact]
        public void HarmonyTargetsKeepTheInspectedSignatures()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                AssertMethod(module, "UI_StorageSelect", "TelevisionSelectSet", true, false, "Building");
                AssertMethod(module, "Building_ElecBandstand", "Building_Update2", true, false, "System.Single");
            }
        }

        [Fact]
        public void OriginalCustomBuildingRangeLimitRemains40()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                var defines = module.Types.Single(type => type.FullName == "Defines");
                AssertStaticInitializerValue(defines, "m_MaxCustomBuildingRange", 40);
            }
        }

        [Fact]
        public void ElectricalWireCheckUsesTheSharedBuildingRange()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                var wireCheck = module.Types.Single(type => type.FullName == "Building_ElecBandstand")
                    .Methods.Single(method =>
                        method.Name == "WireCheck" &&
                        method.Parameters.Select(parameter => parameter.ParameterType.FullName)
                            .SequenceEqual(new[] { "System.Boolean" }));

                Assert.Contains(wireCheck.Body.Instructions, instruction =>
                    instruction.OpCode == OpCodes.Ldfld &&
                    instruction.Operand is FieldReference field &&
                    field.DeclaringType.FullName == "Building" &&
                    field.Name == "m_Range");
                Assert.Contains(wireCheck.Body.Instructions, instruction =>
                    instruction.Operand is MethodReference method &&
                    method.DeclaringType.FullName == "Helpers" &&
                    method.Name == "IsDistanceSmaller");
            }
        }

        [Fact]
        public void TelevisionSignalPathsEnumerateBroadcastStationsWithoutReadingBuildingRange()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                var storageSelect = module.Types.Single(type => type.FullName == "UI_StorageSelect");
                var selection = storageSelect.Methods.Single(method =>
                    method.Name == "TelevisionSelectSet" &&
                    method.Parameters.Select(parameter => parameter.ParameterType.FullName)
                        .SequenceEqual(new[] { "Building" }));
                AssertCalls(selection, "System.Collections.Generic.List`1<Building>", "FindAll");
                AssertDoesNotReadBuildingRange(selection);

                var selectionPredicate = storageSelect.NestedTypes
                    .Single(type => type.Name == "<>c")
                    .Methods.Single(method => method.Name == "<TelevisionSelectSet>b__43_0");
                AssertLoadsInteger(selectionPredicate, 309);
                AssertDoesNotReadBuildingRange(selectionPredicate);

                var bandstand = module.Types.Single(type => type.FullName == "Building_ElecBandstand");
                var automaticSelection = bandstand.Methods.Single(method =>
                    method.Name == "Building_Update2" &&
                    method.Parameters.Select(parameter => parameter.ParameterType.FullName)
                        .SequenceEqual(new[] { "System.Single" }));
                AssertCalls(automaticSelection, "System.Collections.Generic.List`1<Building>", "FindAll");
                AssertDoesNotReadBuildingRange(automaticSelection);

                var automaticPredicate = bandstand.NestedTypes
                    .Single(type => type.Name == "<>c")
                    .Methods.Single(method => method.Name == "<Building_Update2>b__5_0");
                AssertLoadsInteger(automaticPredicate, 309);
                AssertDoesNotReadBuildingRange(automaticPredicate);
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

            var path = Path.Combine(ratopiaDir, "Ratopia_Data", "Managed", "Assembly-CSharp.dll");
            Assert.True(File.Exists(path), $"Assembly-CSharp.dll not found: {path}");
            return path;
        }

        private static void AssertEnumValue(ModuleDefinition module, string typeName, string fieldName, int value)
        {
            var field = module.Types.Single(type => type.FullName == typeName)
                .Fields.Single(item => item.Name == fieldName);
            Assert.Equal(value, Convert.ToInt32(field.Constant));
        }

        private static void AssertField(
            ModuleDefinition module,
            string typeName,
            string fieldName,
            string fieldType,
            bool isPublic)
        {
            var field = module.Types.Single(type => type.FullName == typeName)
                .Fields.Single(item => item.Name == fieldName);
            Assert.Equal(fieldType, field.FieldType.FullName);
            Assert.Equal(isPublic, field.IsPublic);
        }

        private static void AssertMethod(
            ModuleDefinition module,
            string typeName,
            string methodName,
            bool isPublic,
            bool isStatic,
            params string[] parameterTypes)
        {
            var method = module.Types.Single(type => type.FullName == typeName)
                .Methods.Single(item =>
                    item.Name == methodName &&
                    item.Parameters.Select(parameter => parameter.ParameterType.FullName).SequenceEqual(parameterTypes));
            Assert.Equal(isPublic, method.IsPublic);
            Assert.Equal(isStatic, method.IsStatic);
            Assert.Equal("System.Void", method.ReturnType.FullName);
        }

        private static void AssertStaticInitializerValue(TypeDefinition type, string fieldName, int value)
        {
            var field = type.Fields.Single(item => item.Name == fieldName);
            Assert.True(field.IsStatic);
            Assert.True(field.IsInitOnly);
            var initializer = type.Methods.Single(method => method.IsConstructor && method.IsStatic);
            var write = initializer.Body.Instructions.Single(instruction =>
                instruction.OpCode == OpCodes.Stsfld &&
                instruction.Operand is FieldReference target &&
                target.Name == fieldName);
            Assert.Equal(value, Convert.ToInt32(write.Previous.Operand));
        }

        private static void AssertCalls(MethodDefinition method, string typeName, string methodName)
        {
            Assert.Contains(method.Body.Instructions, instruction =>
                instruction.Operand is MethodReference called &&
                called.DeclaringType.FullName == typeName &&
                called.Name == methodName);
        }

        private static void AssertDoesNotReadBuildingRange(MethodDefinition method)
        {
            Assert.DoesNotContain(method.Body.Instructions, instruction =>
                instruction.OpCode == OpCodes.Ldfld &&
                instruction.Operand is FieldReference field &&
                field.DeclaringType.FullName == "Building" &&
                field.Name == "m_Range");
        }

        private static void AssertLoadsInteger(MethodDefinition method, int value)
        {
            Assert.Contains(method.Body.Instructions, instruction =>
                (instruction.OpCode == OpCodes.Ldc_I4 || instruction.OpCode == OpCodes.Ldc_I4_S) &&
                Convert.ToInt32(instruction.Operand) == value);
        }
    }
}
