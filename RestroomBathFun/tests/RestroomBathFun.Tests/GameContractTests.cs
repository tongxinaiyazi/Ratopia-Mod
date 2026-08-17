using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Mono.Cecil;
using Xunit;

namespace RestroomBathFun.Tests
{
    public sealed class GameContractTests
    {
        private const string ExpectedAssemblySha256 =
            "C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D";

        [Fact]
        public void AssemblyCSharpMatchesTheInspectedBuild()
        {
            using (var stream = File.OpenRead(ContractTestPaths.GameAssembly))
            using (var sha = SHA256.Create())
            {
                var actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
                Assert.Equal(ExpectedAssemblySha256, actual);
            }
        }

        [Fact]
        public void FacilityEnumValuesMatchTheInspectedBuild()
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.GameAssembly))
            {
                AssertEnumValue(module, "BuildingName", "Toilet", 110);
                AssertEnumValue(module, "BuildingName", "Baths", 114);
                AssertEnumValue(module, "BuildingName", "ElecToilet", 308);
            }
        }

        [Fact]
        public void ServiceCompletionAndFunUpdateSignaturesRemainPatchable()
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.GameAssembly))
            {
                var citizen = module.Types.Single(type => type.FullName == "T_Citizen");
                AssertMethod(citizen, "OnServiceChoreographyEnd", "System.Void", "Building");
                AssertMethod(citizen, "FunUpdate", "System.Void", "System.Single");

                var abortedGetter = citizen.Methods.Single(method =>
                    method.Name == "get_ServiceAborted" && method.Parameters.Count == 0);
                Assert.True(abortedGetter.IsPublic);
                Assert.True(abortedGetter.IsVirtual);
                Assert.Equal("System.Boolean", abortedGetter.ReturnType.FullName);

                var building = module.Types.Single(type => type.FullName == "Building");
                var info = building.Fields.Single(field => field.Name == "m_Info");
                Assert.True(info.IsPublic);
                Assert.Equal("BuildInfo", info.FieldType.FullName);
            }
        }

        [Theory]
        [InlineData("<ToiletChoreographyC>d__13")]
        [InlineData("<BathsChoreographyC>d__15")]
        public void SupportedChoreographiesCallServiceCompletionExactlyOnce(string nestedTypeName)
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.GameAssembly))
            {
                var toiletInfo = module.Types.Single(type => type.FullName == "ToiletInfo");
                var moveNext = toiletInfo.NestedTypes
                    .Single(type => type.Name == nestedTypeName)
                    .Methods.Single(method => method.Name == "MoveNext");

                var calls = moveNext.Body.Instructions.Count(instruction =>
                    instruction.Operand is MethodReference method &&
                    method.Name == "OnServiceChoreographyEnd" &&
                    method.Parameters.Select(parameter => parameter.ParameterType.FullName)
                        .SequenceEqual(new[] { "Building" }));

                Assert.Equal(1, calls);
            }
        }

        private static void AssertEnumValue(
            ModuleDefinition module,
            string typeName,
            string fieldName,
            int value)
        {
            var field = module.Types.Single(type => type.FullName == typeName)
                .Fields.Single(item => item.Name == fieldName);
            Assert.Equal(value, Convert.ToInt32(field.Constant));
        }

        private static void AssertMethod(
            TypeDefinition type,
            string methodName,
            string returnType,
            params string[] parameterTypes)
        {
            var method = type.Methods.Single(item =>
                item.Name == methodName &&
                item.Parameters.Select(parameter => parameter.ParameterType.FullName)
                    .SequenceEqual(parameterTypes));
            Assert.True(method.IsPublic);
            Assert.True(method.IsVirtual);
            Assert.False(method.IsStatic);
            Assert.Equal(returnType, method.ReturnType.FullName);
        }
    }
}
